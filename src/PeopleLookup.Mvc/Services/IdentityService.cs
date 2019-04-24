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
        Task<SearchResult> Lookup(string search);
    }

    public class IdentityService : IIdentityService
    {
        private readonly AuthSettings _authSettings;

        public IdentityService(IOptions<AuthSettings> authSettings)
        {
            _authSettings = authSettings.Value;
        }

        public async Task<SearchResult> Lookup(string search)
        {
            SearchResult searchResult = null;
            if (search.Contains("@"))
            {
                searchResult = await LookupEmail(search);
            }
            else
            {
                searchResult = await LookupKerb(search);
            }

            if (searchResult.Found)
            {
                LookupAssociations(searchResult.IamId, searchResult);
            }

            return searchResult;
        }

        private async void LookupAssociations(string iamId, SearchResult searchResult)
        {
            var clientws = new IetClient(_authSettings.IamKey);
            var result = await clientws.PPSAssociations.Search(PPSAssociationsSearchField.iamId, iamId);
            if (result.ResponseData.Results.Length > 0)
            {
                var depts = new List<string>();
                foreach (var ppsAssociationsResult in result.ResponseData.Results)
                {
                    depts.Add(ppsAssociationsResult.apptDeptDisplayName);
                }

                searchResult.Departments = string.Join(", ", depts.Distinct());
            }
            return;
        }

        private async Task<SearchResult> LookupEmail(string email)
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
                PopulateSearchResult(searchResult, kerbPerson, email);
                return searchResult;
            }
            return searchResult;
        }

        private async Task<SearchResult> LookupKerb(string kerb)
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

            var email = ucdContactResult.ResponseData.Results.First().Email ?? ucdContactResult.ResponseData.Results.First().CampusEmail;
            PopulateSearchResult(searchResult, ucdKerbPerson, email);


            return searchResult;

        }

        private void PopulateSearchResult(SearchResult searchResult, KerberosResult kerbResult, string email)
        {
            searchResult.Found = true;
            searchResult.KerbId = kerbResult.UserId;
            searchResult.IamId = kerbResult.IamId;
            searchResult.Email = email;
            searchResult.FullName = kerbResult.FullName;
            searchResult.IsEmployee = kerbResult.IsEmployee;
            searchResult.IsFaculty = kerbResult.IsFaculty;
            searchResult.IsStudent = kerbResult.IsStudent;
            searchResult.IsHSEmployee = kerbResult.IsHSEmployee;
            searchResult.IsExternal = kerbResult.IsExternal;
        }
    }
}
