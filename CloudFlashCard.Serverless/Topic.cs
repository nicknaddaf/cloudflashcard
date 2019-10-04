using System;
using System.Collections.Generic;
using System.Text;

namespace CloudFlashCard.Serverless
{
    public class Topic
    {
        public string TopicId { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
