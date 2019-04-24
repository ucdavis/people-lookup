using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PeopleLookup.Mvc.Models;
using PeopleLookup.Mvc.Services;

namespace PeopleLookup.Mvc.Controllers
{
    public class LookupController : Controller
    {
        private readonly IIdentityService _identityService;

        public LookupController(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        public IActionResult Bulk()
        {
            var model = new BulkModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Bulk(BulkModel model)
        {
            const string regexEmailPattern = @"\b[A-Z0-9._-]+@[A-Z0-9][A-Z0-9.-]{0,61}[A-Z0-9]\.[A-Z.]{2,6}\b";
            const string regexKerbPattern = @"\b[A-Z0-9]{2,10}\b";

            model.Results = new List<SearchResults>();

            // Find matches
            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(model.BulkEmail, regexEmailPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (var match in matches)
            {
                var result = new SearchResults{SearchValue = match.ToString()};
                var kerbResults = await _identityService.LookupEmail(result.SearchValue);
                if (kerbResults != null)
                {
                    result.Found = true;
                    result.FullName = kerbResults.FullName;
                }
                model.Results.Add(result);
            }


            return View(model);
        }

    }
}