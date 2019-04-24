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
        Task<SearchResult> LookupEmail(string email);
        Task<SearchResult> LookupKerb(string kerb);
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

        public async Task<SearchResult> LookupEmail(string email)
        {
            var searchResult = new SearchResult();
            searchResult.SearchValue = email;

            var clientws = new IetClient(_authSettings.IamKey);
            var iamResult = await clientws.Contacts.Search(ContactSearchField.email, email);
            var iamId = iamResult.ResponseData.Results.Length > 0
                ? iamResult.ResponseData.Results[0].IamId
                : string.Empty;
            if (string.IsNullOrWhiteSpace(iamId))
            {
                return searchResult;
            }
            // return info for the user identified by this IAM 
            var result = await clientws.Kerberos.Search(KerberosSearchField.iamId, iamId);
            if (result.ResponseData.Results.Length > 0)
            {
                var kerbPerson = result.ResponseData.Results.First();
                searchResult.Found = true;
                searchResult.FullName = kerbPerson.FullName;
                searchResult.SearchValue = email;
                searchResult.LoginId = kerbPerson.UserId;
                searchResult.Email = email;
                return searchResult;
            }
            return searchResult;
        }

        public async Task<SearchResult> LookupKerb(string kerb)
        {
            var searchResult = new SearchResult();
            searchResult.SearchValue = kerb;
            var clientws = new IetClient(_authSettings.IamKey);
            var ucdKerbResult = await clientws.Kerberos.Search(KerberosSearchField.userId, kerb);

            if (ucdKerbResult.ResponseData.Results.Length == 0)
            {
                return searchResult;
            }

            if (ucdKerbResult.ResponseData.Results.Length != 1)
            {
                var iamIds = ucdKerbResult.ResponseData.Results.Select(a => a.IamId).Distinct().ToArray();
                var userIDs = ucdKerbResult.ResponseData.Results.Select(a => a.UserId).Distinct().ToArray();
                if (iamIds.Length != 1 && userIDs.Length != 1)
                {
                    searchResult.ErrorMessage =
                        $"IAM issue with non unique values for kerbs: {string.Join(',', userIDs)} IAM: {string.Join(',', iamIds)}";
                    return searchResult;
                }
            }

            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.First();
            // find their email
            var ucdContactResult = await clientws.Contacts.Get(ucdKerbPerson.IamId);

            if (ucdContactResult.ResponseData.Results.Length == 0)
            {
                searchResult.ErrorMessage = "Contact Info not found.";
                return searchResult;
            }

            searchResult.Found = true;
            searchResult.FullName = ucdKerbPerson.FullName;
            searchResult.SearchValue = kerb;
            searchResult.LoginId = ucdKerbPerson.UserId;
            searchResult.Email = ucdContactResult.ResponseData.Results.First().CampusEmail;

            return searchResult;

        }
    }
}
