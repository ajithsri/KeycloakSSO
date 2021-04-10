using Newtonsoft.Json;
using System;

namespace KeycloakCore.Keycloak
{
    /// <summary>
    /// The User info class.
    /// Converted from json using https://quicktype.io/csharp/
    /// </summary>
    public class UserInfo
    {
        [JsonProperty("sub")]
        public Guid Sub { get; set; }

        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("preferred_username")]
        public string PreferredUsername { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("mobile_number")]
        public string MobileNumber { get; set; }

        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
