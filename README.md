# KeycloakSSO
[Keycloak](https://www.keycloak.org/) single sign on integration with ASP .NET MVC application.

It contains 2 projects.
### KeycloakCore - It's a library.
*It communicates with the Keycloak Server to get the authenticated user info.

*It uses the [OpenID Connect](https://www.keycloak.org/docs/latest/securing_apps/#openid-connect-2).

### KeycloakSSO - It's a client.
*When the user clicks on 'Login' button, get the Keycloak login session info, and display it.
*You need to configure the Keyclaock server authentication information as follows.
```c#
        SingleSignOnSettings settings = new SingleSignOnSettings()
        {
            KeycloakUrl = "https://keycloack_auth.com/auth",
            Realm = "Your Realm",
            ClientId = "Your ClientId",
            ClientSecret = "Your ClientSecret",
            BaseUri = "https://localhost:44389/",
            CallbackUrl = "https://localhost:44389/Home/Callback"
        };
 ```
## How it works
* When the user clicks on login button in the client app, the library will call the Keycloak server to get the authentication session information.
* If there is a session already, then that session user info will be displayed in the client.
* If there is no session with Keycloak, the client will be redirected to Keycloak site to login.
* Once login succeeded, the session user info will be displayed in the client.



## Screenshots
*Display user info
![Display user info](http://ajithsri.com/wp-content/uploads/2021/04/user-info-1.png)

*When there is an error
![When there is an error](http://ajithsri.com/wp-content/uploads/2021/04/error-1.png)
