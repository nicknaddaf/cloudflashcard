using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CloudFlashCard.Web.Controllers
{
    public abstract class CloudFlashCardController : Controller
    {
        private readonly AppSettings appSettings = new AppSettings();

        protected CloudFlashCardController()
        {
        }

        protected AppSettings AppSettings { get; private set; }
    }
}