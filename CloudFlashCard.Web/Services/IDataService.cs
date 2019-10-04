using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudFlashCard.Web.Models;

namespace CloudFlashCard.Web.Services
{
    public interface IDataService
    {
        // Topic
        Task<List<Topic>> GetTopicList(string userId);

        Task<Result<TopicViewModel>> GetTopicById(string topicId);

        Task<Result<CloudFlashCardServiceResponse>> CreateTopic(NewTopicViewModel model);

        Task<Result<CloudFlashCardServiceResponse>> UpdateTopic(string id, string name);

        Task<Result<CloudFlashCardServiceResponse>> DeleteTopic(string id);

        // Flash Card
        Task<List<FlashCard>> GetCardList(string topicId);

        Task<Result<FlashCardViewModel>> GetCardById(string cardId);

        Task<Result<CloudFlashCardServiceResponse>> CreateCard(NewCardViewModel model);

        Task<Result<CloudFlashCardServiceResponse>> UpdateCard(string id, string content);

        Task<Result<CloudFlashCardServiceResponse>> DeleteCard(string id);
    }
}
