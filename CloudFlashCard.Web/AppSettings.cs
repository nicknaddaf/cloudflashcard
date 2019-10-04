using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudFlashCard.Web
{
    public class AppSettings
    {
        public AppSettings()
        {
        }

        public string AwsRegion { get; set; }

        public string ClientId { get; set; }

        public string PoolId { get; set; }

        public string IdentityId { get; set; }

        public string Idp { get; set; }


        public string ApiGatewayBaseAddress { get; set; }
    }
}
