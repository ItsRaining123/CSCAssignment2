using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CSCAssignment2.APIs
{
    public class PaymentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Prices()
        {
            return View();
        }
        public IActionResult Account()
        {
            return View();
        }
    }
}
