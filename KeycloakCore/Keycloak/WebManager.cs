using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;

namespace KeycloakCore.Keycloak
{
    public class WebManager
    {

        private MetaData Metadata { get; set; }
        private SingleSignOnSettings SsoSettings { get; set; }

        private Uri BaseUri
        {
            get { return new Uri(SsoSettings.BaseUri); }
        }

        private Uri MetadataEndpoint
        {
            get { return new Uri($"{SsoSettings.KeycloakUrl}/realms/{SsoSettings.Realm}/{OpenIdProviderMetadataNames.Discovery}"); }
        }

        public WebManager(SingleSignOnSettings settings)
        {
            SsoSettings = settings;
            Metadata = RefreshMetadata();
        }

        /// <summary>
        /// Process the call back from Keycloak
        /// </summary>
        /// <param name="Request">The request</param>
        public UserInfo Callback(HttpRequestBase Request)
        {
            try
            {
                if (Request?.QueryString != null)
                {
                    var authResult = new AuthorizationResponse(Request.QueryString.ToString());

                    if (!string.IsNullOrEmpty(authResult.Code))
                    {
                        var tokenEndpointUrl = Metadata.TokenEndpoint;
                        var parameters = BuildAccessTokenEndpointParams(authResult.Code);
                        var response = SendHttpPostRequest(tokenEndpointUrl, parameters);

                        if (!string.IsNullOrEmpty(response))
                        {
                            var token = new TokenResponse(response);

                            var userData = GetUserInfo(token.AccessToken);
                            var userInfo = JsonConvert.DeserializeObject<UserInfo>(userData);

                            return userInfo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        /// <summary>
        /// Get the user info.
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns></returns>
        public string GetUserInfo(string token)
        {
            var userData = SendHttpPostRequest(Metadata.UserInfoEndpoint, null, token);
            return userData;
        }

        /// <summary>
        /// Create access token endpoint parameters.
        /// </summary>
        /// <param name="code">The access code.</param>
        /// <returns></returns>
        public List<string> BuildAccessTokenEndpointParams(string code)
        {
            // Create parameter dictionary
            var parameters = new List<string>
            {
                $"{OpenIdConnectParameterNames.RedirectUri}={GetCallbackUri()}",
                $"{OpenIdConnectParameterNames.GrantType}={Constants.OpenIdConnectParameterValues.AuthorizationCode}",
                $"{OpenIdConnectParameterNames.Code}={code}"
            };

            // Add optional parameters
            if (!string.IsNullOrWhiteSpace(SsoSettings.ClientId))
            {
                parameters.Add($"{OpenIdConnectParameterNames.ClientId}={SsoSettings.ClientId}");

                if (!string.IsNullOrWhiteSpace(SsoSettings.ClientSecret))
                    parameters.Add($"{OpenIdConnectParameterNames.ClientSecret}={SsoSettings.ClientSecret}");
            }

            return parameters;
        }

        /// <summary>
        /// Post method caller.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        protected string SendHttpPostRequest(Uri uri, List<string> parameters = null, string token = null)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    if (!string.IsNullOrEmpty(token))
                        webClient.Headers.Add("Authorization", $"Bearer {token}");

                    var data = parameters != null ? string.Join("&", parameters) : string.Empty;
                    webClient.Headers[HttpRequestHeader.ContentType] = Constants.ContentType.UrlEncoded;

                    return webClient.UploadString(uri.ToString(), data);
                }
            }
            catch (Exception exception)
            {
                throw new KeycloakException("URL is not accessible", exception);
            }
        }

        /// <summary>
        ///     Generates the OpenID Connect compliant Keycloak login URL
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Uri GenerateLoginUri()
        {
            if (BaseUri == null)
                throw new ArgumentNullException(nameof(BaseUri));

            // Generate login URI and data
            var state = Guid.NewGuid().ToString();
            var loginParams = BuildAuthorizationEndpointContent(state);
            var loginUrl = Metadata.AuthorizationEndpoint;

            // Return login URI
            var loginQueryString = loginParams.ReadAsStringAsync().Result;
            return new Uri($"{loginUrl}{(!string.IsNullOrEmpty(loginQueryString) ? $"?{loginQueryString}" : "")}");
        }

        /// <summary>
        /// Create authorization endpoint content.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public HttpContent BuildAuthorizationEndpointContent(string state)
        {
            // Create parameter dictionary
            var parameters = new Dictionary<string, string>
            {
                {OpenIdConnectParameterNames.ClientId, SsoSettings.ClientId},
                {OpenIdConnectParameterNames.RedirectUri, GetCallbackUri().ToString()},
                {OpenIdConnectParameterNames.ResponseType, Constants.ResponseType},
                {OpenIdConnectParameterNames.Scope, Constants.Scope},
                {OpenIdConnectParameterNames.State, state}
            };

            return new FormUrlEncodedContent(parameters);
        }

        /// <summary>
        /// Initialize all end point using the info URL (one time).
        /// </summary>
        private MetaData RefreshMetadata()
        {
            // Get Metadata from endpoint
            var dataTask = HttpApiGet(MetadataEndpoint);
            var metadata = new MetaData();

            // Try to get the JSON metadata object
            JObject json;
            try
            {
                json = JObject.Parse(dataTask);
            }
            catch (JsonReaderException exception)
            {
                // Fail on invalid JSON
                throw new KeycloakException($"Metadata address returned invalid JSON object ('{MetadataEndpoint}')", exception);
            }

            // Set internal URI properties
            try
            {
                // Preload required data fields
                var jwksEndpoint = new Uri(json[OpenIdProviderMetadataNames.JwksUri].ToString());
                var jwks = new JsonWebKeySet(HttpApiGet(jwksEndpoint));
                {
                    metadata.Jwks = jwks;
                    metadata.JwksEndpoint = jwksEndpoint;
                    metadata.Issuer = json[OpenIdProviderMetadataNames.Issuer].ToString();
                    metadata.AuthorizationEndpoint =
                        new Uri(json[OpenIdProviderMetadataNames.AuthorizationEndpoint].ToString());
                    metadata.TokenEndpoint =
                        new Uri(json[OpenIdProviderMetadataNames.TokenEndpoint].ToString());
                    metadata.UserInfoEndpoint =
                        new Uri(json[OpenIdProviderMetadataNames.UserInfoEndpoint].ToString());
                    metadata.EndSessionEndpoint =
                        new Uri(json[OpenIdProviderMetadataNames.EndSessionEndpoint].ToString());

                    // Check for values
                    if (metadata.AuthorizationEndpoint == null || metadata.TokenEndpoint == null ||
                        metadata.UserInfoEndpoint == null)
                    {
                        throw new KeycloakException("One or more metadata endpoints are missing");
                    }
                }
            }
            catch (Exception exception)
            {
                // Fail on invalid URI or metadata
                throw new KeycloakException($"RefreshMetadataAsync: Metadata address returned incomplete data ('{MetadataEndpoint}')", exception);
            }

            return metadata;
        }

        /// <summary>
        /// Get call back Uri.
        /// </summary>
        /// <returns></returns>
        private Uri GetCallbackUri()
        {
            return new Uri(SsoSettings.CallbackUrl);
        }

        /// <summary>
        /// HttpGet request handler.
        /// </summary>
        /// <param name="uri">The Uri</param>
        /// <returns></returns>
        private string HttpApiGet(Uri uri)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    return webClient.DownloadString(uri);
                }
            }
            catch (Exception exception)
            {
                throw new KeycloakException("webClient URI is inaccessible", exception);
            }
        }

        /// <summary>
        /// The basic info about the URLs
        /// </summary>
        private class MetaData
        {
            public Uri AuthorizationEndpoint;
            public Uri EndSessionEndpoint;
            public string Issuer;
            public JsonWebKeySet Jwks;
            public Uri JwksEndpoint;
            public Uri TokenEndpoint;
            public Uri UserInfoEndpoint;
        }
    }
}