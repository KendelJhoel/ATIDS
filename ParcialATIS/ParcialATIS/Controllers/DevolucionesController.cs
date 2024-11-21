using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;

namespace ParcialATIS.Controllers
{
    public class DevolucionesController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        

    }
}
