using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PeopleLookup.Mvc.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Ietws;
using Microsoft.AspNetCore.Authorization;
using PeopleLookup.Mvc.Services;

namespace PeopleLookup.Mvc.Controllers
{
    public class HomeController : SuperController
    {
        private readonly IIdentityService _identityService;
        private readonly IPermissionService _permissionService;

        public HomeController(IIdentityService identityService, IPermissionService permissionService)
        {
            _identityService = identityService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.AllowSensitiveInfo = _permissionService.CanSeeSensitiveInfo();

            var model = new BulkModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(BulkModel model)
        {
            var allowSensitiveInfo = _permissionService.CanSeeSensitiveInfo();
            ViewBag.AllowSensitiveInfo = allowSensitiveInfo;

            const string regexEmailPattern = @"\b[A-Z0-9._-]+@[A-Z0-9][A-Z0-9.-]{0,61}[A-Z0-9]\.[A-Z.]{2,6}\b";
            const string regexKerbPattern = @"\b[A-Z0-9]{2,10}\b";
            const string regexStudentIdPattern = @"\b[0-9]{2,10}\b";
            const string regexPpsIdPattern = @"\b[0-9]{2,10}\b"; //Based off StudentId???
            const string regexIamIdPattern = @"\b[0-9]{2,10}\b"; 

            model.Results = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(model.BulkEmail) && string.IsNullOrWhiteSpace(model.BulkKerb) && string.IsNullOrWhiteSpace(model.BulkStudentIds) && string.IsNullOrWhiteSpace(model.BulkPpsIds) && string.IsNullOrWhiteSpace(model.BulkIamIds))
            {
                ErrorMessage = "You must select something to search";
                return View(model);
            }

            System.Text.RegularExpressions.MatchCollection matches = null;
            // Find matches
            if (!string.IsNullOrWhiteSpace(model.BulkEmail))
            {
                matches = System.Text.RegularExpressions.Regex.Matches(model.BulkEmail, regexEmailPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                foreach (var match in matches)
                {
                    var result = await _identityService.Lookup(match.ToString());
                    if (!allowSensitiveInfo)
                    {
                        result.HideSensitiveFields();
                    }
                    model.Results.Add(result);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.BulkKerb))
            {
                matches = System.Text.RegularExpressions.Regex.Matches(model.BulkKerb, regexKerbPattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (var match in matches)
                {
                    var result = await _identityService.Lookup(match.ToString());
                    if (!allowSensitiveInfo)
                    {
                        result.HideSensitiveFields();
                    }
                    model.Results.Add(result);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.BulkIamIds))
            {
                matches = System.Text.RegularExpressions.Regex.Matches(model.BulkIamIds, regexIamIdPattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (var match in matches)
                {
                    var result = await _identityService.LookupId(PeopleSearchField.iamId, match.ToString());

                    model.Results.Add(result);
                }
            }

            if (allowSensitiveInfo)
            {
                if (!string.IsNullOrWhiteSpace(model.BulkStudentIds))
                {
                    matches = System.Text.RegularExpressions.Regex.Matches(model.BulkStudentIds, regexStudentIdPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    foreach (var match in matches)
                    {
                        var result = await _identityService.LookupId(PeopleSearchField.studentId, match.ToString());

                        model.Results.Add(result);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.BulkPpsIds))
                {
                    matches = System.Text.RegularExpressions.Regex.Matches(model.BulkPpsIds, regexPpsIdPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    foreach (var match in matches)
                    {
                        var result = await _identityService.LookupId(PeopleSearchField.ppsId, match.ToString());

                        model.Results.Add(result);
                    }
                }
            }

            if (model.Results.Count == 0)
            {
                Message = "No results found. Maybe you didn't paste in emails?";
            }

            return View(model);
        }
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
