using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PeopleLookup.Mvc.Models;

namespace PeopleLookup.Mvc.Controllers
{
    public class LookupController : Controller
    {
        public IActionResult Bulk()
        {
            var model = new BulkModel();
            return View(model);
        }

        [HttpPost]
        public Task<ActionResult> Bulk(BulkModel model)
        {
            throw new NotImplementedException();
        }
    }
}