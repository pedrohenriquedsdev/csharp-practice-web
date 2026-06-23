using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.Models;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
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
