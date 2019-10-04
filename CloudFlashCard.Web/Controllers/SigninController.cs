using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CloudFlashCard.Web.Models;
using CloudFlashCard.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CloudFlashCard.Web.Controllers
{
    public class SigninController : CloudFlashCardController
    {
        private ICognitoService _cognitoService;
        private readonly AppSettings appSettings = new AppSettings();

        public SigninController(IOptions<AppSettings> settings, ICognitoService cognitoService)
        {
            _cognitoService = cognitoService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SigninViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authResp = await _cognitoService.LoginAsync(model.Email, model.Password);

            if (authResp.IsFailure)
            {
                // Checking for PasswordResetRequiredException
                if (authResp.Error.Contains("reset"))
                {
                    return RedirectToAction("ForgotPassword", "home",
                        new { Email = model.Email });
                }

                ModelState.AddModelError("", authResp.Error);
                return View(model);
            }

            if (authResp.Value.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                ModelState.AddModelError("", "Response: " + authResp.Value.HttpStatusCode.ToString());
                return View(model);
            }

            if (authResp.Value.AuthenticationResult != null)
            {
                var user = await _cognitoService.GetUser(model.Email);

                if (user.IsFailure)
                {
                    ModelState.AddModelError("", user.Error);
                    return View(model);
                }

                if (user.Value.Status != Amazon.CognitoIdentityProvider.UserStatusType.CONFIRMED)
                {
                    ModelState.AddModelError("", "Invalid Status: " + user.Value.Status);
                    return View(model);
                }

                AuthenticateUser(user.Value.Sub);

                return RedirectToAction("index", "dashboard", new { id = user.Value.Sub });
            }

            // in case you need to pass another challenge
            switch (authResp.Value.ChallengeName.Value)
            {
                case "NEW_PASSWORD_REQUIRED":
                    return RedirectToAction("ResetPassword", "home",
                        new { token = authResp.Value.Session, email = model.Email });

                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        public async Task<IActionResult> UserAttributes(string id)
        {
            var userResult = await _cognitoService.GetUserBySub(id);
            return View(userResult.Value);
        }

        private async void AuthenticateUser(string id)
        {
            const string Issuer = "AWSCognito";

            //var userResult = await _cognitoService.GetUserBySub(id).ConfigureAwait(false);
            var userResult = _cognitoService.GetUserBySub(id).Result;

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, userResult.Value.Full_Name, ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.Email, userResult.Value.Email, ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.Sid, userResult.Value.Sub, ClaimValueTypes.String, Issuer),
                new Claim("UID", userResult.Value.Id, ClaimValueTypes.String, Issuer),
                new Claim(ClaimTypes.Country, "CA", ClaimValueTypes.String, Issuer)
            };

            var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var userPrincipal = new ClaimsPrincipal(userIdentity);

            var authenticationProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(2),
                IsPersistent = true,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(userPrincipal, authenticationProperties);
        }
    }
}