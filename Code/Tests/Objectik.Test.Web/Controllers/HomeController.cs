using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Objectik.Test.Web.Models;
using Objectiks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Objectik.Test.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ObjectiksOf _repository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ObjectiksOf repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public IActionResult Index()
        {
            var page = _repository
                 .TypeOf<Pages>()
                 .PrimaryOf(1)
                 .First();

            return View(page);
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
