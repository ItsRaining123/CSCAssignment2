using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CSCAssignment2.Controllers
{
    public class TalentsController : Controller
    {
        public IActionResult CreateTalent()
        {
            return View();
        }
        public IActionResult UpdateTalent()
        {
            return View();
        }
        public IActionResult ViewTalents()
        {
            return View();
        }

        public IActionResult UploadTalents()
        {
            return View();
        }

    }
}
