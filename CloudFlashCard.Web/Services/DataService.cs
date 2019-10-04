using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CloudFlashCard.Web.Models;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace CloudFlashCard.Web.Services
{
    public class DataService : IDataService
    {
        private readonly AppSettings _appSettings = new AppSettings();

        public DataService(IOptions<AppSettings> settings)
        {
            _appSettings = settings.Value;
        }

        public async Task<List<Topic>> GetTopicList(string userId)
        {
            var list = new List<Topic>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_appSettings.ApiGatewayBaseAddress);

                var response = await client.GetAsync("usertopics?UserId=" + userId);

                if (response.IsSuccessStatusCode)
                {
                    var readTask = response.Content.ReadAsStringAsync();

                    var topics = JsonConvert.DeserializeObject<List<Topic>>(readTask.Result);

                    list = topics.OrderBy(x => x.Name.Value).ToList<Topic>();
                }
            }

            return list;
        }

        public async Task<Result<TopicViewModel>> GetTopicById(string topicId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_appSettings.ApiGatewayBaseAddress);

                    var response = await client.GetAsync("topics?TopicId=" + topicId);

                    if (response.IsSuccessStatusCode)
                    {
                        var readTask = response.Content.ReadAsStringAsync();

                        var topic = JsonConvert.DeserializeObject<Topic>(readTask.Result);

                        var model = new TopicViewModel();

                        if (topic != null)
                        {
                            model.Name = topic.Name.Value;
                            model.TopicId = topic.TopicId.Value;
                            model.CreateDate = topic.CreateDate.Value;
                            model.UpdateDate = topic.UpdateDate.Value;
                        }

                        return Result.Ok(model);
                    }
                    else
                    {
                        return Result.Fail<TopicViewModel>(
                            string.Format("Error has occured with code {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Fail<TopicViewModel>(ex.Message);
            };
        }

        public async Task<Result<CloudFlashCardServiceResponse>> CreateTopic(NewTopicViewModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_appSettings.ApiGatewayBaseAddress);

                    var jsonContent = new JsonContent(model);

                    var response = await client.PostAsync(
                        "topics?UserId=" + model.UserId.ToString() + "&Name=" + model.Name,
                        jsonContent);

                    var serviceResponse = new CloudFlashCardServiceResponse
                    {
                        HttpStatusCode = response.StatusCode
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        return Result.Ok(serviceResponse);
                    }
                    else
                    {
                        return Result.Fail<CloudFlashCardServiceResponse>(
                            string.Format("Error has occured with code {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Fail<CloudFlashCardServiceResponse>(ex.Message);
            }
        }

        public Task<Result<CloudFlashCardServiceResponse>> UpdateTopic(string id, string name)
        {
            throw new NotImplementedException();
        }

        public Task<Result<CloudFlashCardServiceResponse>> DeleteTopic(string id)
        {
            throw new NotImplementedException();
        }


        public async Task<List<FlashCard>> GetCardList(string topicId)
        {
            var list = new List<FlashCard>(); 

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_appSettings.ApiGatewayBaseAddress);

                var response = await client.GetAsync("topiccards?TopicId=" + topicId);

                if (response.IsSuccessStatusCode)
                {
                    var readTask = response.Content.ReadAsStringAsync();

                    var cards = JsonConvert.DeserializeObject<List<FlashCard>>(readTask.Result);

                    list = cards.OrderByDescending(x => x.CreateDate.Value).ToList<FlashCard>();
                }
            }

            return list;
        }

        public async Task<Result<FlashCardViewModel>> GetCardById(string cardId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_appSettings.ApiGatewayBaseAddress);

                    var response = await client.GetAsync("cards?CardId=" + cardId);

                    if (response.IsSuccessStatusCode)
                    {
                        var readTask = response.Content.ReadAsStringAsync();

                        var card = JsonConvert.DeserializeObject<FlashCard>(readTask.Result);

                        var model = new FlashCardViewModel();

                        if (card != null)
                        {
                            model.CardId = card.CardId.Value;
                            model.TopicId = card.TopicId.Value;
                            model.Content = card.Content.Value;
                            model.CreateDate = card.CreateDate.Value;
                            model.UpdateDate = card.UpdateDate.Value;
                        }

                        return Result.Ok(model);
                    }
                    else
                    {
                        return Result.Fail<FlashCardViewModel>(
                            string.Format("Error has occured with code {0}", response.StatusCode));
                    }
                }
            }
            catch (Exception ex)
            {
                return Result.Fail<FlashCardViewModel>(ex.Message);
            };
        }

        public async Task<Result<CloudFlashCardServiceResponse>> CreateCard(NewCardViewModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_appSettings.ApiGatewayBaseAddress);

                    var jsonContent = new JsonContent(model);

                    var response = await client.PostAsync(
                        "cards?UserId=" + model.UserId.ToString() + "&TopicId=" + model.TopicId + "&Content=" + model.Content,
                        jsonContent);

                    var serviceResponse = new CloudFlashCardServiceResponse
                    {
                        HttpStatusCode = response.StatusCode
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        return Result.Ok<CloudFlashCardServiceResponse>(serviceResponse);
                    }
                    else
                    {
                        return Result.Fail<CloudFlashCardServiceResponse>(
                            string.Format("Error has occured with code {0}", response.StatusCode));
                    }
                }
            }
            catch(Exception ex)
            {
                return Result.Fail<CloudFlashCardServiceResponse>(ex.Message);
            }
        }

        public Task<Result<CloudFlashCardServiceResponse>> UpdateCard(string id, string content)
        {
            throw new NotImplementedException();
        }

        public Task<Result<CloudFlashCardServiceResponse>> DeleteCard(string id)
        {
            throw new NotImplementedException();
        }
    }
}
