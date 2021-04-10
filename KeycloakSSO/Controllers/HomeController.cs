using KeycloakCore.Keycloak;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KeycloakSSO.Controllers
{
    public class HomeController : Controller
    {
        SingleSignOnSettings settings = new SingleSignOnSettings()
        {
            KeycloakUrl = "https://keycloack_auth.com/auth",
            Realm = "Your Realm",
            ClientId = "Your ClientId",
            ClientSecret = "Your ClientSecret",
            BaseUri = "https://localhost:44389/",
            CallbackUrl = "https://localhost:44389/Home/Callback"
        };

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            try
            {
                var keycloakManager = new WebManager(settings);
                var url = keycloakManager.GenerateLoginUri();
                return Redirect(url.ToString());
            }
            catch (Exception ex)
            {
                return RedirectToAction("CallbackError", new { error = $"Exception while calling Keycloak. Exception {ex.Message}" });
            }
        }

        public ActionResult Callback()
        {
            var keycloakManager = new WebManager(settings);
            var userInfo = keycloakManager.Callback(Request);
            if (userInfo != null)
            {
                return View(userInfo);
            }
            else
            {
                return RedirectToAction("CallbackError");
            }
        }

        public ActionResult CallbackError(string error)
        {
            ViewBag.Message = "Error : " + error;

            return View();
        }
    }
}