using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CloudFlashCard.Web.Models
{
    public class JsonContent : StringContent
    {
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        {
        }
    }

    public class FlashCardViewModel
    {
        public string CardId { get; set; }

        public string TopicId { get; set; }

        public string UserId { get; set; }

        public string Content { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }

    public class TopicViewModel
    {
        public string TopicId { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }

    public class NewTopicViewModel
    {
        public string UserId { get; set; }

        public string Name { get; set; }
    }

    public class NewCardViewModel
    {
        public string UserId { get; set; }

        public string TopicId { get; set; }

        public string TopicName { get; set; }

        public string Content { get; set; }
    }

    public class CreateDate
    {
        public DateTime Value { get; set; }
        public int Type { get; set; }
    }

    public class Content
    {
        public string Value { get; set; }
        public int Type { get; set; }
    }

    public class UserId
    {
        public string Value { get; set; }
        public int Type { get; set; }
    }

    public class UpdateDate
    {
        public DateTime Value { get; set; }
        public int Type { get; set; }
    }

    public class TopicId
    {
        public string Value { get; set; }
        public int Type { get; set; }
    }

    public class CardId
    {
        public string Value { get; set; }
        public int Type { get; set; }
    }

    public class Name
    {
        public string Value { get; set; }
        public int Type { get; set; }
    }

    public class FlashCard
    {
        public CreateDate CreateDate { get; set; }
        public Content Content { get; set; }
        public UserId UserId { get; set; }
        public UpdateDate UpdateDate { get; set; }
        public TopicId TopicId { get; set; }
        public CardId CardId { get; set; }
    }

    public class Topic
    {
        public CreateDate CreateDate { get; set; }
        public UserId UserId { get; set; }
        public UpdateDate UpdateDate { get; set; }
        public TopicId TopicId { get; set; }
        public Name Name { get; set; }
    }


    public class CloudFlashCardServiceResponse
    {
        // Returns the custom error message
        public long CustomErrorMessage { get; set; }

        // Returns the content length of the HTTP response.
        public long ContentLength { get; set; }

        // Returns the status code of the HTTP response.
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
