using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.CognitoIdentityProvider;

namespace CloudFlashCard.Web.Models
{
    public class AwsIdentityUser : CognitoUserAttributes
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public UserStatusType Status { get; set; }

        public string Full_Name
        {
            get
            {
                return this.Given_Name + " " + this.Family_Name;
            }
        }
    }
}
