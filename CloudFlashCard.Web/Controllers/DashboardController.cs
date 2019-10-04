using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using CloudFlashCard.Web.Models;
using CloudFlashCard.Web.Services;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Amazon;
using System.Threading;

namespace CloudFlashCard.Web.Controllers
{
    public class DashboardController : AuthenticatedController
    {
        private readonly AppSettings _appSettings = new AppSettings();
        private readonly IDataService _dataService = null;

        public DashboardController(IOptions<AppSettings> settings, IDataService dataService)
        {
            _appSettings = settings.Value;
            _dataService = dataService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _dataService.GetTopicList(GetSignedInUserId());

            return View(model);
        }

        public IActionResult NewTopic()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewTopic(NewTopicViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UserId = GetSignedInUserId();

            var responseTask = await _dataService.CreateTopic(model);

            if (responseTask.IsFailure)
            {
                ModelState.AddModelError("", responseTask.Error);
                return View(model);
            }

            if (responseTask.IsSuccess)
            {
                ViewBag.Success = "You have successfully created a new topic.";
            }

            return View(model);
        }

        public IActionResult EditTopic(Guid id)
        {
            var responseTask = _dataService.GetTopicById(id.ToString());

            var response = responseTask.Result;

            if(response.IsFailure)
            {
                ModelState.AddModelError("", response.Error);

                return View(response.Value);
            }

            if(response.IsSuccess)
            {
                ViewBag.TopicId = id.ToString();

                return View(response.Value);
            }

            return View(response.Value);
        }

        public async Task<IActionResult> CardsByTopic(Guid id)
        {
            var model = await _dataService.GetCardList(id.ToString());

            ViewBag.TopicId = id.ToString();

            return View(model);
        }

        public IActionResult NewCard(Guid id)
        {
            var model = new NewCardViewModel()
            {
                TopicId = id.ToString()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> NewCard(NewCardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UserId = GetSignedInUserId();

            var responseTask = await _dataService.CreateCard(model);

            if (responseTask.IsFailure)
            {
                ModelState.AddModelError("", responseTask.Error);
                return View(model);
            }

            if (responseTask.IsSuccess)
            {
                ViewBag.Success = "You have successfully created a new flash card.";
            }

            return View(model);
        }

        // Audio card
        public async Task<IActionResult> NewCard2(Guid id)
        {
            // https://github.com/Audior/Recordmp3js
            var model = new NewCardViewModel()
            {
                TopicId = id.ToString()
            };

            AmazonTranscribeServiceClient tran = new AmazonTranscribeServiceClient(RegionEndpoint.USEast1);

            StartTranscriptionJobRequest startJobRequest = new StartTranscriptionJobRequest
            {
                Media = new Media() { MediaFileUri = "https://s3.amazonaws.com/cloudflashcard2/audio_recording_1537885865066.mp3" },
                MediaFormat = MediaFormat.Mp3,
                LanguageCode = LanguageCode.EnUS,
                OutputBucketName = "cloudflashcard2",
                TranscriptionJobName = "CFC" + Guid.NewGuid()
            };

            var transTask = await tran.StartTranscriptionJobAsync(startJobRequest);

            //Thread.Sleep(20000);

            //var getJobRequest = new GetTranscriptionJobRequest
            //{
            //    TranscriptionJobName = startJobRequest.TranscriptionJobName
            //};

            //var job = await tran.GetTranscriptionJobAsync(getJobRequest);

            return View(model);
        }

        [HttpPost]
        public IActionResult UploadFlashCardMp3(object fd)
        {
            return Json("");
        }

        public IActionResult EditCard(Guid id)
        {
            var responseTask = _dataService.GetCardById(id.ToString());

            var response = responseTask.Result;

            if (response.IsFailure)
            {
                ModelState.AddModelError("", response.Error);

                return View(response.Value);
            }

            if (response.IsSuccess)
            {
                ViewBag.CardId = id.ToString();

                return View(response.Value);
            }

            return View(response.Value);
        }

        public IActionResult ViewCard()
        {
            return View();
        }

        public IActionResult DeleteCard()
        {
            return View();
        }

        private string GetSignedInUserId()
        {
            var identity = (ClaimsPrincipal)HttpContext.User;

            var sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid)
                   .Select(c => c.Value).SingleOrDefault();

            if(string.IsNullOrEmpty(sid))
            {
                RedirectToAction("index", "signin");
            }

            return sid;
        }
    }
}