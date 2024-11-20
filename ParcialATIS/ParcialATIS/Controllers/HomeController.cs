using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParcialATIS.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace ParcialATIS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbController _dbController;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _dbController = new DbController();
        }

        public IActionResult Index()
        {
            var autosDisponibles = new List<Auto>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM autos WHERE estado = 'disponible'";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        autosDisponibles.Add(new Auto
                        {
                            IdAuto = Convert.ToInt32(reader["idauto"]),
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Placa = reader["placa"].ToString(),
                            Tipo = reader["tipo"].ToString(),
                            CostoDia = Convert.ToDouble(reader["costodia"])
                        });
                    }
                }
            }

            return View(autosDisponibles);
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
