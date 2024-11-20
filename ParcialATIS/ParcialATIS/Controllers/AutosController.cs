using Microsoft.AspNetCore.Mvc;
using ParcialATIS.Models;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ParcialATIS.Controllers
{
    public class AutosController : Controller
    {
        private readonly DbController _dbController;

        public AutosController()
        {
            _dbController = new DbController();
        }

        // listar autos
        public IActionResult Index()
        {
            var autos = new List<Auto>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM autos";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        autos.Add(new Auto
                        {
                            IdAuto = Convert.ToInt32(reader["idauto"]),
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Placa = reader["placa"].ToString(),
                            Tipo = reader["tipo"].ToString(),
                            Estado = reader["estado"].ToString(),
                            CostoDia = Convert.ToDouble(reader["costodia"])
                        });
                    }
                }
            }

            return View(autos);
        }

        // Crear auto (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Crear auto (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Auto auto)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO autos (marca, modelo, placa, tipo, estado, costodia) VALUES (@Marca, @Modelo, @Placa, @Tipo, @Estado, @CostoDia)";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Marca", auto.Marca);
                cmd.Parameters.AddWithValue("@Modelo", auto.Modelo);
                cmd.Parameters.AddWithValue("@Placa", auto.Placa);
                cmd.Parameters.AddWithValue("@Tipo", auto.Tipo);
                cmd.Parameters.AddWithValue("@Estado", auto.Estado);
                cmd.Parameters.AddWithValue("@CostoDia", auto.CostoDia);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Index));
        }

        // Editar auto (GET)
        public IActionResult Editar(int id)
        {
            Auto auto = null;

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM autos WHERE idauto = @Id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        auto = new Auto
                        {
                            IdAuto = Convert.ToInt32(reader["idauto"]),
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Placa = reader["placa"].ToString(),
                            Tipo = reader["tipo"].ToString(),
                            Estado = reader["estado"].ToString(),
                            CostoDia = Convert.ToDouble(reader["costodia"])
                        };
                    }
                }
            }

            return auto == null ? NotFound() : View("Editar", auto);
        }


        // Editar auto (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Auto auto)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "UPDATE autos SET marca = @Marca, modelo = @Modelo, placa = @Placa, tipo = @Tipo, estado = @Estado, costodia = @CostoDia WHERE idauto = @Id";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Marca", auto.Marca);
                cmd.Parameters.AddWithValue("@Modelo", auto.Modelo);
                cmd.Parameters.AddWithValue("@Placa", auto.Placa);
                cmd.Parameters.AddWithValue("@Tipo", auto.Tipo);
                cmd.Parameters.AddWithValue("@Estado", auto.Estado);
                cmd.Parameters.AddWithValue("@CostoDia", auto.CostoDia);
                cmd.Parameters.AddWithValue("@Id", auto.IdAuto);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Index));
        }

        // Eliminar auto (GET)
        public IActionResult Delete(int id)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM autos WHERE idauto = @Id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Crear()
        {
            return View();
        }

    }
}
