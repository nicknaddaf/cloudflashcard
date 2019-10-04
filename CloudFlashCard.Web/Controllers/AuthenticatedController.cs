using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CloudFlashCard.Web.Controllers
{
    [Authorize]
    //[Authorize(Policy = "")]
    public class AuthenticatedController : CloudFlashCardController
    {
        protected AuthenticatedController()
        {
        }

        public Uri BaseAddressUri { get; private set; }
    }
}