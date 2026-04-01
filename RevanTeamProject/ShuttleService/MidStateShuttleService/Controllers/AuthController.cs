using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MidStateShuttleService.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet] // DEV NOTE: Public endpoint that starts the OpenID Connect login flow.
        public IActionResult Login()
        {
            return Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = "/Home/Index" // DEV NOTE: Redirect after successful authentication.
                },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize] // DEV NOTE: Only authenticated users should trigger logout.
        [HttpGet] // DEV NOTE: Kept as GET so the current logout UI continues to work.
        public IActionResult Logout()
        {
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = "/" // DEV NOTE: Return to home page after logout.
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}