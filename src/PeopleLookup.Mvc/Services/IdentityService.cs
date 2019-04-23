using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ietws;
using Microsoft.Extensions.Options;
using PeopleLookup.Mvc.Models;

namespace PeopleLookup.Mvc.Services
{
    public interface IIdentityService
    {
        Task<string> Test();
    }

    public class IdentityService : IIdentityService
    {
        private readonly AuthSettings _authSettings;

        public IdentityService(IOptions<AuthSettings> authSettings)
        {
            _authSettings = authSettings.Value;
        }

        public async Task<string> Test()
        {
            var clientws = new IetClient(_authSettings.IamKey);
            // get IAM from email
            var iamResult = await clientws.Contacts.Search(ContactSearchField.email, "jsylvestre@ucdavis.edu");
            var iamId = iamResult.ResponseData.Results.Length > 0
                ? iamResult.ResponseData.Results[0].IamId
                : string.Empty;
            if (string.IsNullOrWhiteSpace(iamId))
            {
                return null;
            }

            // return info for the user identified by this IAM 
            var result = await clientws.Kerberos.Search(KerberosSearchField.iamId, iamId);

            if (result.ResponseData.Results.Length > 0)
            {
                var ucdKerbPerson = result.ResponseData.Results.First();
                var user = ucdKerbPerson.FullName;
                return user;
            }
            return null;
        }
    }
}
