namespace KeycloakCore.Keycloak
{
    public static class Constants
    {
        public const string ResponseType = "code";
        public const string Scope = "openid";

        public class ContentType
        {
            public const string UrlEncoded = "application/x-www-form-urlencoded";
        }

        public class OpenIdConnectParameterNames
        {
            public const string RefreshToken = "refresh_token";

        }

        public class OpenIdConnectParameterValues
        {
            public const string AuthorizationCode = "authorization_code";

        }
    }
}
