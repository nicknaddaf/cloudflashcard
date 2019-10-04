using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CloudFlashCard.Web.Controllers
{
    public class FeaturesController : CloudFlashCardController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}