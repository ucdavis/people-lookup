using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleLookup.Mvc.Models
{
    public class AuthSettings
    {
        public string Authority { get; set; }
        public string IamKey { get; set; }
        public string CasBaseUrl { get; set; }
        public string ShowSensitiveInfo { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
