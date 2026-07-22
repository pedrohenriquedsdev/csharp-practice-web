using Microsoft.AspNetCore.Mvc;

namespace GestaoDeEquipamentosWeb.ConsoleApp.Controllers;

// MVC - Model, View, Controller

public class HomeController : Controller
{
    // GET: HomeController
    public ActionResult Index() // página inicial de um controlador
    {
        return View();
    }

}