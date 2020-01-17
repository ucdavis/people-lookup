using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Security.CAS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PeopleLookup.Mvc.Models;

namespace PeopleLookup.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthSettings _authSettings;
        public AccountController(IOptions<AuthSettings> authSettings)
        {

            _authSettings = authSettings.Value;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect($"{_authSettings.CasBaseUrl}logout");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [Route("login")]
        public async Task Login(string returnUrl)
        {
            var props = new AuthenticationProperties { RedirectUri = returnUrl };
            await HttpContext.ChallengeAsync(CasDefaults.AuthenticationScheme, props);
        }

    }
}