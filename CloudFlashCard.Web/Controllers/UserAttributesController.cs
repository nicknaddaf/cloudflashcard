using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CloudFlashCard.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CloudFlashCard.Web.Controllers
{
    public class UserAttributesController : AuthenticatedController
    {
        private ICognitoService _cognitoService;
        private readonly AppSettings appSettings = new AppSettings();

        public UserAttributesController(IOptions<AppSettings> settings, ICognitoService cognitoService)
        {
            _cognitoService = cognitoService;
        }

        public async Task<IActionResult> Index()
        {
            var userResult = await _cognitoService.GetUserBySub(GetSignedInUserId());

            return View(userResult.Value);
        }

        private string GetSignedInUserId()
        {
            var identity = (ClaimsPrincipal)HttpContext.User;

            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid)
                   .Select(c => c.Value).SingleOrDefault();

            return sid;
        }
    }
}