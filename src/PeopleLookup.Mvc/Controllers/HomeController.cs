using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PeopleLookup.Mvc.Models;
using PeopleLookup.Mvc.Services;

namespace PeopleLookup.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIdentityService _identityService;

        public HomeController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Test = await _identityService.Test();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
