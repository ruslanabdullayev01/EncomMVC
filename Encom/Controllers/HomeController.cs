using Encom.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Encom.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
