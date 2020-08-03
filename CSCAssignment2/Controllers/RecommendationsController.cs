using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CSCAssignment2.Controllers
{
    public class RecommendationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddUserRecommendation()
        {
            return View();
        }
        public IActionResult AddItemRecommendation()
        {
            return View();
        }
    }
}
