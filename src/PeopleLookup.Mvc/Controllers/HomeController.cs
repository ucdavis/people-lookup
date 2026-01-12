using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PeopleLookup.Mvc.Models;
using System.Diagnostics;
using System.Linq;
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
        const int BatchSize = 20;

        public HomeController(IIdentityService identityService, IPermissionService permissionService)
        {
            _identityService = identityService;
            _permissionService = permissionService;
        }

        public IActionResult Index()
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
            const string regexLastNamePattern = @"\b[A-Z0-9\-]{2,50}\b";
            const string regexEmpIdPattern = @"\b[0-9]{2,10}\b"; //Based off StudentId???

            model.Results = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(model.BulkEmail) 
                && string.IsNullOrWhiteSpace(model.BulkKerb) 
                && string.IsNullOrWhiteSpace(model.BulkStudentIds) 
                && string.IsNullOrWhiteSpace(model.BulkPpsIds) 
                && string.IsNullOrWhiteSpace(model.BulkIamIds)
                && string.IsNullOrWhiteSpace(model.BulkLastnames)
                && string.IsNullOrWhiteSpace(model.BulkEmployeeId)
                && string.IsNullOrWhiteSpace(model.PpsaDeptCode))
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

                var results = new List<SearchResult>();
                var batches = matches.Select(m => m.ToString())
                    .Select((value, index) => new { value, index })
                    .GroupBy(x => x.index / BatchSize) // Process 20 at a time
                    .Select(g => g.Select(x => x.value).ToList());

                foreach (var batch in batches)
                {
                    var batchResults = await Task.WhenAll(
                        batch.Select(item => _identityService.Lookup(item))
                    );
                    results.AddRange(batchResults);
                }

                foreach (var tempResult in results)
                {
                    //I could change this to AddRange if I move the AllowSensitive to the lookup service and change it to a List from IList.
                    if (!allowSensitiveInfo)
                    {
                        tempResult.HideSensitiveFields();
                    }
                    model.Results.Add(tempResult);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.BulkKerb))
            {
                matches = System.Text.RegularExpressions.Regex.Matches(model.BulkKerb, regexKerbPattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                var results = new List<SearchResult>();
                var batches = matches.Select(m => m.ToString())
                    .Select((value, index) => new { value, index })
                    .GroupBy(x => x.index / BatchSize) // Process 20 at a time
                    .Select(g => g.Select(x => x.value).ToList());

                foreach (var batch in batches)
                {
                    var batchResults = await Task.WhenAll(
                        batch.Select(item => _identityService.Lookup(item))
                    );
                    results.AddRange(batchResults);
                }

                foreach (var tempResult in results)
                {
                    //I could change this to AddRange if I move the AllowSensitive to the lookup service and change it to a List from IList.
                    if (!allowSensitiveInfo)
                    {
                        tempResult.HideSensitiveFields();
                    }
                    model.Results.Add(tempResult);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.BulkIamIds))
            {
                matches = System.Text.RegularExpressions.Regex.Matches(model.BulkIamIds, regexIamIdPattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                var results = new List<SearchResult>();
                var batches = matches.Select(m => m.ToString())
                    .Select((value, index) => new { value, index })
                    .GroupBy(x => x.index / BatchSize) // Process 20 at a time
                    .Select(g => g.Select(x => x.value).ToList());

                foreach (var batch in batches)
                {
                    var batchResults = await Task.WhenAll(
                        batch.Select(item => _identityService.LookupId(PeopleSearchField.iamId, item))
                    );
                    results.AddRange(batchResults);
                }

                foreach (var tempResult in results)
                {
                    //I could change this to AddRange if I move the AllowSensitive to the lookup service and change it to a List from IList.
                    if (!allowSensitiveInfo) //Was missing?
                    {
                        tempResult.HideSensitiveFields();
                    }
                    model.Results.Add(tempResult);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.BulkLastnames))
            {
                matches = System.Text.RegularExpressions.Regex.Matches(model.BulkLastnames, regexLastNamePattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                var results = matches.Select(a => _identityService.LookupLastName(a.ToString())).ToArray();
                var tempResults = await Task.WhenAll(results);
                foreach (var tempResult in tempResults)
                {
                    foreach (var result in tempResult)
                    {
                        //I could change this to AddRange if I move the AllowSensitive to the lookup service and change it to a List from IList.
                        if (!allowSensitiveInfo) //Was missing?
                        {
                            result.HideSensitiveFields();
                        }

                        model.Results.Add(result);
                    }

                }
            }

            if (!string.IsNullOrWhiteSpace(model.PpsaDeptCode))
            {
                var results = await _identityService.LookupPpsaCode(model.PpsaDeptCode);
                foreach (var result in results)
                {
                    if (!allowSensitiveInfo)
                    {
                        result.HideSensitiveFields();
                    }
                    model.Results.Add(result);
                }
            }

            if (allowSensitiveInfo)
            {
                if (!string.IsNullOrWhiteSpace(model.BulkStudentIds))
                {
                    matches = System.Text.RegularExpressions.Regex.Matches(model.BulkStudentIds, regexStudentIdPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    var results = new List<SearchResult>();
                    var batches = matches.Select(m => m.ToString())
                        .Select((value, index) => new { value, index })
                        .GroupBy(x => x.index / BatchSize) // Process 20 at a time
                        .Select(g => g.Select(x => x.value).ToList());

                    foreach (var batch in batches)
                    {
                        var batchResults = await Task.WhenAll(
                            batch.Select(item => _identityService.LookupId(PeopleSearchField.studentId, item))
                        );
                        results.AddRange(batchResults);
                    }

                    foreach (var tempResult in results)
                    {
                        model.Results.Add(tempResult);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.BulkPpsIds))
                {
                    matches = System.Text.RegularExpressions.Regex.Matches(model.BulkPpsIds, regexPpsIdPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    var results = matches.Select(a => _identityService.LookupId(PeopleSearchField.ppsId, a.ToString())).ToArray();
                    var tempResults = await Task.WhenAll(results);
                    foreach (var tempResult in tempResults)
                    {
                        model.Results.Add(tempResult);
                    }
                }

                if (!string.IsNullOrWhiteSpace(model.BulkEmployeeId))
                {
                    matches = System.Text.RegularExpressions.Regex.Matches(model.BulkEmployeeId, regexEmpIdPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    var results = new List<SearchResult>();
                    var batches = matches.Select(m => m.ToString())
                        .Select((value, index) => new { value, index })
                        .GroupBy(x => x.index / BatchSize) // Process 20 at a time
                        .Select(g => g.Select(x => x.value).ToList());

                    foreach (var batch in batches)
                    {
                        var batchResults = await Task.WhenAll(
                            batch.Select(item => _identityService.LookupId(PeopleSearchField.employeeId, item))
                        );
                        results.AddRange(batchResults);
                    }

                    foreach (var tempResult in results)
                    {
                        model.Results.Add(tempResult);
                    }
                }
            }

            if (model.Results.Count == 0)
            {
                Message = "No results found. Maybe you didn't paste in emails?";
            }

            return View(model);
        }

        [HttpGet]
        [Route("Detail/{id?}")]
        public async Task<IActionResult> Detail(string id)
        {
            var allowSensitiveInfo = _permissionService.CanSeeSensitiveInfo();
            ViewBag.AllowSensitiveInfo = allowSensitiveInfo;

            SearchResult result = null;

            if (string.IsNullOrWhiteSpace(id))
            {
                result = new SearchResult();
                result.ErrorMessage = "No parameter passed";
            }
            else
            {
                if(allowSensitiveInfo && id.Contains("@health."))
                {
                    //Health emails are (probably) the same as ucd emails, so we can just swap out the domain to get correct results.
                    id = id.Replace("@health.", "@");
                }

                result = await _identityService.Lookup(id);
                if (!allowSensitiveInfo)
                {
                    result.HideSensitiveFields();
                }
            }

            return View(result);

        }

        [HttpGet]
        public IActionResult Test()
        {
            throw new System.Exception("This is a test exception for demo purposes only. Please ignore.");
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

        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        public ActionResult Ping()
        {
            return Content("Pong");
        }
    }
}
