using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PeopleLookup.Mvc.Models;
using PeopleLookup.Mvc.Services;

namespace PeopleLookup.Mvc.Controllers
{
    public class LookupController : SuperController
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
            
            model.Results = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(model.BulkEmail) && string.IsNullOrWhiteSpace(model.BulkKerb))
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
                    model.Results.Add(await _identityService.Lookup(match.ToString()));
                }
            }

            if (!string.IsNullOrWhiteSpace(model.BulkKerb))
            {
                matches = System.Text.RegularExpressions.Regex.Matches(model.BulkKerb, regexKerbPattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (var match in matches)
                {
                    model.Results.Add(await _identityService.Lookup(match.ToString()));
                }
            }

            if (model.Results.Count == 0)
            {
                Message = "No results found. Maybe you didn't paste in emails?";
            }

            return View(model);
        }


    }
}