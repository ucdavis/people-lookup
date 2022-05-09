using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ietws;
using Microsoft.Extensions.Options;
using PeopleLookup.Mvc.Models;

namespace PeopleLookup.Mvc.Services
{
    public interface IIdentityService
    {
        Task<SearchResult> Lookup(string search);
        Task<SearchResult> LookupId(PeopleSearchField searchField, string search);
        Task<User> GetByKerberos(string kerb);
        Task<SearchResult[]> LookupLastName(string search);

        Task<SearchResult[]> LookupPpsaCode(string search);
    }

    public class IdentityService : IIdentityService
    {
        private readonly AuthSettings _authSettings;
        private readonly IetClient _clientws;
        public IdentityService(IOptions<AuthSettings> authSettings, IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient("identity");

            _authSettings = authSettings.Value;
            _clientws = new IetClient(httpClient, _authSettings.IamKey);
        }

        public async Task<SearchResult> Lookup(string search)
        {
            SearchResult searchResult = new SearchResult(); ;
            try
            {
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
                    await LookupAssociations(searchResult.IamId, searchResult);
                    await LookupEmployeeId(searchResult.IamId, searchResult);
                }
            }
            catch (Exception e)
            {
                searchResult.SearchValue = search;
                searchResult.ErrorMessage = "Error Occurred";
                searchResult.ExceptionMessage = $"(Lookup) Error: {e.Message} Inner: {e.InnerException?.Message} {e}";
            }

            return searchResult;
        }

        public async Task<SearchResult[]> LookupLastName(string search)
        {
            var rtValue = new List<SearchResult>();
            try
            {
                //var clientws = new IetClient(_authSettings.IamKey);
                var peopleResult = await _clientws.People.Search(PeopleSearchField.oLastName, search);
                var iamIds = peopleResult.ResponseData.Results.Select(a => a.IamId).Distinct().ToArray();
                var results = iamIds.Select(a => LookupId(PeopleSearchField.iamId, a)).ToArray(); //These have their own try catch
                var tempResults = await Task.WhenAll(results);
                if (tempResults.Length <= 0)
                {
                    var sr = new SearchResult();
                    sr.SearchValue = search;
                    sr.Found = false;
                    rtValue.Add(sr);
                }
                foreach (var result in tempResults)
                {
                    result.SearchValue = search;
                    rtValue.Add(result);
                }
            }
            catch (Exception e)
            {
                SearchResult searchResult = new SearchResult();
                searchResult.SearchValue = search;
                searchResult.ErrorMessage = "Error Occurred";
                searchResult.ExceptionMessage = $"(LookupLastName) Error: {e.Message} Inner: {e.InnerException?.Message} {e}";
                rtValue.Add(searchResult);
            }


            return rtValue.ToArray();
        }

        public async Task<SearchResult[]> LookupPpsaCode(string search)
        {
            var rtValue = new List<SearchResult>();
            try
            {
                var Ppsaresults = await _clientws.PPSAssociations.GetIamIds(PPSAssociationsSearchField.adminDeptCode, search);

                var iamIds = Ppsaresults.ResponseData.Results.Select(a => a.IamId).ToArray();
                var results =
                    iamIds.Select(a => LookupId(PeopleSearchField.iamId, a)).ToArray(); //These have their own try catch
                var tempResults = await Task.WhenAll(results);

                if (tempResults.Length <= 0)
                {
                    var sr = new SearchResult();
                    sr.SearchValue = search;
                    sr.Found = false;
                    rtValue.Add(sr);
                }
                foreach (var result in tempResults)
                {
                    result.SearchValue = search;
                    rtValue.Add(result);
                }
            }
            catch (Exception e)
            {
                SearchResult searchResult = new SearchResult();
                searchResult.SearchValue = search;
                searchResult.ErrorMessage = "Error Occurred";
                searchResult.ExceptionMessage = $"(Lookup PPSA Code) Error: {e.Message} Inner: {e.InnerException?.Message} {e}";
                rtValue.Add(searchResult);
            }


            return rtValue.ToArray();
        }

        public async Task<SearchResult> LookupId(PeopleSearchField searchField, string search)
        {
            var searchResult = new SearchResult();
            searchResult.SearchValue = search;
            try
            {
                //var clientws = new IetClient(_authSettings.IamKey);
                var peopleResult = await _clientws.People.Search(searchField, search);
                var iamId = peopleResult.ResponseData.Results.Length > 0
                    ? peopleResult.ResponseData.Results[0].IamId
                    : string.Empty;
                if (string.IsNullOrWhiteSpace(iamId))
                {
                    return searchResult;
                }

                // find their email
                var ucdContactResult = await _clientws.Contacts.Get(iamId);
                if (ucdContactResult.ResponseData.Results.Length == 0)
                {
                    // No contact details
                    var kerbResult = await _clientws.Kerberos.Search(KerberosSearchField.iamId, iamId);
                    if (kerbResult.ResponseData.Results.Length > 0)
                    {
                        var kerbPerson = kerbResult.ResponseData.Results.First();
                        kerbPerson.EmployeeId = peopleResult.ResponseData.Results.First().EmployeeId;
                        PopulateSearchResult(searchResult, kerbPerson, ucdContactResult);
                    }
                    else
                    {
                        var person = peopleResult.ResponseData.Results.First();
                        PopulatePartialSearchResult(searchResult, person, ucdContactResult);
                    }

                    searchResult.ErrorMessage = "No Contact details";
                    return searchResult;
                }

                // return info for the user identified by this IAM 
                var result = await _clientws.Kerberos.Search(KerberosSearchField.iamId, iamId);
                if (result.ResponseData.Results.Length > 0)
                {
                    var kerbPerson = result.ResponseData.Results.First();
                    kerbPerson.EmployeeId = peopleResult.ResponseData.Results.First().EmployeeId;
                    PopulateSearchResult(searchResult, kerbPerson, ucdContactResult);
                }
                else
                {
                    if (ucdContactResult.ResponseData.Results.Length > 0)
                    {
                        var person = peopleResult.ResponseData.Results.First();
                        PopulatePartialSearchResult(searchResult, person, ucdContactResult);
                        searchResult.ErrorMessage = "Kerb Not Found";
                    }
                }

                if (searchResult.Found)
                {
                    await LookupAssociations(searchResult.IamId, searchResult);
                    await LookupEmployeeId(searchResult.IamId,
                        searchResult); //Shouldn't need it as it should be in the if above
                }
            }
            catch (Exception e)
            {
                searchResult.ErrorMessage = "Error Occurred";
                searchResult.ExceptionMessage = $"(LookupId) Error: {e.Message} Inner: {e.InnerException?.Message} {e}";
            }

            return searchResult;
        }

        //Just used for logging in.
        public async Task<User> GetByKerberos(string kerb)
        {
            //var clientws = new IetClient(_authSettings.IamKey);
            var ucdKerbResult = await _clientws.Kerberos.Search(KerberosSearchField.userId, kerb);

            if (ucdKerbResult.ResponseData.Results.Length == 0)
            {
                return null;
            }

            if (ucdKerbResult.ResponseData.Results.Length != 1)
            {
                var iamIds = ucdKerbResult.ResponseData.Results.Select(a => a.IamId).Distinct().ToArray();
                var userIDs = ucdKerbResult.ResponseData.Results.Select(a => a.UserId).Distinct().ToArray();
                if (iamIds.Length != 1 && userIDs.Length != 1)
                {
                    throw new Exception($"IAM issue with non unique values for kerbs: {string.Join(',', userIDs)} IAM: {string.Join(',', iamIds)}");
                }
            }

            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.First();

            // find their email
            var ucdContactResult = await _clientws.Contacts.Get(ucdKerbPerson.IamId);

            if(ucdContactResult.ResponseData.Results.Length == 0)
            {
                return null;
            }

            var ucdContact = ucdContactResult.ResponseData.Results.First();
            var rtValue = CreateUser(ucdContact.Email, ucdKerbPerson, ucdKerbPerson.IamId);

            if (string.IsNullOrWhiteSpace(rtValue.Email))
            {
                if (!string.IsNullOrWhiteSpace(ucdKerbPerson.UserId))
                {
                    rtValue.Email = $"{ucdKerbPerson.UserId}@ucdavis.edu";
                }
            }

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(rtValue, new ValidationContext(rtValue), results);
            if (!isValid)
            {
                return null;
            }


            return rtValue;
        }



        private User CreateUser(string email, KerberosResult ucdKerbPerson, string iamId)
        {
            var user = new User()
            {
                FirstName = ucdKerbPerson.FirstName,
                LastName = ucdKerbPerson.LastName,
                Id = ucdKerbPerson.UserId,
                Email = email,
                Iam = iamId
            };
            return user;
        }

        private async Task LookupAssociations(string iamId, SearchResult searchResult)
        {
            //var clientws = new IetClient(_authSettings.IamKey);

            var result = await _clientws.PPSAssociations.Search(PPSAssociationsSearchField.iamId, iamId);
            if (result.ResponseData.Results.Length > 0)
            {
                var depts = new List<string>();
                var deptCodes = new List<string>();
                foreach (var ppsAssociationsResult in result.ResponseData.Results)
                {
                    depts.Add(ppsAssociationsResult.apptDeptDisplayName);
                    deptCodes.Add(ppsAssociationsResult.apptDeptCode);
                }

                searchResult.Departments = $"{string.Join(", ", depts.Distinct())} ({string.Join(", ", deptCodes.Distinct())})" ;
                searchResult.Title = string.Empty;
                if (result.ResponseData.Results.Any(a => a.titleOfficialName != null))
                {
                    searchResult.Title = string.Join(", ", result.ResponseData.Results.Where(a => a.titleOfficialName != null).Select(a => a.titleOfficialName).Distinct().ToArray());
                }

                
                searchResult.ReportsToIamId = result.ResponseData.Results.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.reportsToIAMID))?.reportsToIAMID;
            }
            
            return;
        }

        private async Task LookupEmployeeId(string iamId, SearchResult searchResult)
        {
            if (!string.IsNullOrWhiteSpace(searchResult.EmployeeId))
            {
                //Already got it
                return;
            }
            //var clientws = new IetClient(_authSettings.IamKey);
            var peopleResult = await _clientws.People.Get(iamId);
            if (peopleResult.ResponseData.Results.Length > 0)
            {
                searchResult.EmployeeId = peopleResult.ResponseData.Results.First().EmployeeId;
            }
        }

        private async Task<SearchResult> LookupEmail(string email)
        {
            var searchResult = new SearchResult();
            searchResult.SearchValue = email;

            //var clientws = new IetClient(_authSettings.IamKey);
            var iamResult = await _clientws.Contacts.Search(ContactSearchField.email, email);
            var iamId = iamResult.ResponseData.Results.Length > 0
                ? iamResult.ResponseData.Results[0].IamId
                : string.Empty;
            if (string.IsNullOrWhiteSpace(iamId))
            {
                return searchResult;
            }
            // return info for the user identified by this IAM 
            var result = await _clientws.Kerberos.Search(KerberosSearchField.iamId, iamId);
            if (result.ResponseData.Results.Length > 0)
            {
                var kerbPerson = result.ResponseData.Results.First();
                PopulateSearchResult(searchResult, kerbPerson, iamResult);
                return searchResult;
            }
            return searchResult;
        }

       

        private async Task<SearchResult> LookupKerb(string kerb)
        {
            var searchResult = new SearchResult();
            searchResult.SearchValue = kerb;
            //var clientws = new IetClient(_authSettings.IamKey);
            var ucdKerbResult = await _clientws.Kerberos.Search(KerberosSearchField.userId, kerb);

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

            // find their contact info
            var ucdContactResult = await _clientws.Contacts.Get(ucdKerbPerson.IamId);

            PopulateSearchResult(searchResult, ucdKerbPerson, ucdContactResult);

            if (ucdContactResult.ResponseData.Results.Length == 0)
            {                
                searchResult.ErrorMessage = "Contact Info not found.";
            }

            return searchResult;

        }

        private void PopulateSearchResult(SearchResult searchResult, KerberosResult kerbResult, ContactResults contactResults)
        {
            var contact = contactResults.ResponseData.Results.FirstOrDefault();

            searchResult.Found = true;
            searchResult.KerbId = kerbResult.UserId;
            searchResult.IamId = kerbResult.IamId;
            searchResult.Email = contact?.Email;
            searchResult.WorkPhone = contact?.WorkPhone;
            searchResult.FullName = kerbResult.FullName;
            searchResult.FirstName = kerbResult.FirstName;
            searchResult.LastName = kerbResult.LastName;
            searchResult.Pronouns = kerbResult.DPronouns;
            searchResult.IsEmployee = kerbResult.IsEmployee;
            searchResult.IsFaculty = kerbResult.IsFaculty;
            searchResult.IsStudent = kerbResult.IsStudent;
            searchResult.IsHSEmployee = kerbResult.IsHSEmployee;
            searchResult.IsExternal = kerbResult.IsExternal;
            searchResult.IsStaff = kerbResult.IsStaff;
            searchResult.PpsId = kerbResult.PpsId;
            searchResult.StudentId = kerbResult.StudentId;
            searchResult.BannerPidm = kerbResult.BannerPidm;
            searchResult.EmployeeId = kerbResult.EmployeeId;
            searchResult.MothraId = kerbResult.MothraId;
        }

        private void PopulatePartialSearchResult(SearchResult searchResult, PeopleResult kerbResult, ContactResults contactResults)
        {
            var contact = contactResults.ResponseData.Results.FirstOrDefault();

            searchResult.Found = true;
            searchResult.KerbId = null;
            searchResult.IamId = kerbResult.IamId;
            searchResult.Email = contact?.Email;
            searchResult.WorkPhone = contact?.WorkPhone;
            searchResult.FullName = kerbResult.FullName;
            searchResult.FirstName = kerbResult.FirstName;
            searchResult.Pronouns = kerbResult.DPronouns;
            searchResult.LastName = kerbResult.LastName;
            searchResult.IsEmployee = kerbResult.IsEmployee;
            searchResult.IsFaculty = kerbResult.IsFaculty;
            searchResult.IsStudent = kerbResult.IsStudent;
            searchResult.IsHSEmployee = kerbResult.IsHSEmployee;
            searchResult.IsExternal = kerbResult.IsExternal;
            searchResult.IsStaff = kerbResult.IsStaff;
            searchResult.PpsId = kerbResult.PpsId;
            searchResult.StudentId = kerbResult.StudentId;
            searchResult.BannerPidm = kerbResult.BannerPidm;
            searchResult.EmployeeId = kerbResult.EmployeeId;
            searchResult.MothraId = kerbResult?.MothraId;
        }
    }
}
