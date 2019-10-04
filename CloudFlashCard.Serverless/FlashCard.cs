using System;
using System.Collections.Generic;
using System.Text;

namespace CloudFlashCard.Serverless
{
    public class FlashCard
    {
        public string CardId { get; set; }

        public string TopicId { get; set; }

        public string UserId { get; set; }

        public string Content { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
