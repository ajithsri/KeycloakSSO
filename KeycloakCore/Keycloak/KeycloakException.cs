using System;

namespace KeycloakCore.Keycloak
{
    /// <summary>
    /// The exception is thrown in the case of the Keycloak related errors.
    /// </summary>
    [Serializable]
    public class KeycloakException : Exception
    {
        public KeycloakException()
            : base() { }

        public KeycloakException(string message)
            : base(message) { }

        public KeycloakException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
