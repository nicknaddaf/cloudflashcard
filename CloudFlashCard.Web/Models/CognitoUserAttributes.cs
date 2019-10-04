using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudFlashCard.Web.Models
{
    public class CognitoUserAttributes
    {
        public string Sub { get; set; }
        public string Address { get; set; }
        public string Birthdate { get; set; }
        public string Email { get; set; }
        public string Family_Name { get; set; }
        public string Gender { get; set; }
        public string Given_Name { get; set; }
        public string Locale { get; set; }
        public string Middle_Name { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Phone_Number { get; set; }
        public string Picture { get; set; }
        public string Preferred_Username { get; set; }
        public string Profile { get; set; }
        public string Timezone { get; set; }
        public DateTime Updated_At { get; set; }
        public string Username { get; set; }
        public string Website { get; set; }
    }
}
