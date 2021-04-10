namespace KeycloakCore.Keycloak
{
    public class SingleSignOnSettings
    {
        /// <summary>
        /// Gets or sets the value Keycloak Url.
        /// </summary>
        public string KeycloakUrl { get; set; }

        /// <summary>
        /// Gets or sets the value Keycloak Realm.
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the value Keycloak ClientId.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the value Keycloak ClientSecret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the BaseUri.
        /// </summary>
        public string BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the CallbackUrl.
        /// </summary>
        public string CallbackUrl { get; set; }
    }
}