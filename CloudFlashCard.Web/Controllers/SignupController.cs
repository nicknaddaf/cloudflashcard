using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudFlashCard.Web.Models;
using CloudFlashCard.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CloudFlashCard.Web.Controllers
{
    public class SignupController : CloudFlashCardController
    {
        private readonly ICognitoService _cognitoService;
        private readonly AppSettings appSettings = new AppSettings();

        public SignupController(IOptions<AppSettings> settings, ICognitoService cognitoService)
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
        public async Task<IActionResult> Index(SignupViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var identityUser = new AwsIdentityUser()
            {
                Email = model.Email,
                Username = model.UserName,
                Password = model.Password
            };

            var authResp = await _cognitoService.CreateAsync(identityUser);

            if (authResp.IsFailure)
            {
                // Checking for PasswordResetRequiredException
                if (authResp.Error.Contains("reset"))
                {
                    return RedirectToAction("ForgotPassword", "Cognito",
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

            if (authResp.Value.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                ViewBag.Success = "You have successfully created a you user.";
                return View(model);
            }

            return View(model);
        }

    }
}