using Microsoft.AspNetCore.Mvc;
using ParcialATIS.Models;
using System;
using System.Collections.Generic;
using System.IO; // Para manejo de archivos
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

        // Listar autos (GET)
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
                            CostoDia = Convert.ToDouble(reader["costodia"]),
                            Imagen = reader["imagen"].ToString() // Incluye la imagen
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
        public IActionResult Create(Auto auto, IFormFile Imagen)
        {
            if (Imagen != null && Imagen.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var filePath = Path.Combine(uploads, Imagen.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Imagen.CopyTo(stream);
                }

                auto.Imagen = Imagen.FileName; // Guardar solo el nombre del archivo
            }
            else
            {
                auto.Imagen = "default.jpg"; // Imagen por defecto si no se carga ninguna
            }

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO autos (marca, modelo, placa, tipo, estado, costodia, imagen) VALUES (@Marca, @Modelo, @Placa, @Tipo, @Estado, @CostoDia, @Imagen)";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Marca", auto.Marca);
                cmd.Parameters.AddWithValue("@Modelo", auto.Modelo);
                cmd.Parameters.AddWithValue("@Placa", auto.Placa);
                cmd.Parameters.AddWithValue("@Tipo", auto.Tipo);
                cmd.Parameters.AddWithValue("@Estado", auto.Estado);
                cmd.Parameters.AddWithValue("@CostoDia", auto.CostoDia);
                cmd.Parameters.AddWithValue("@Imagen", auto.Imagen);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Index));
        }

        // Editar auto (GET)
        public IActionResult Editar(int id)
        {
            Console.WriteLine($"Intentando cargar el auto con ID: {id}");

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
                            CostoDia = Convert.ToDouble(reader["costodia"]),
                            Imagen = reader["imagen"].ToString()
                        };

                        Console.WriteLine("Auto encontrado.");
                    }
                    else
                    {
                        Console.WriteLine("No se encontró ningún auto con ese ID.");
                    }
                }
            }

            if (auto == null)
            {
                return NotFound("El auto especificado no existe.");
            }

            return View(auto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Auto auto)
        {
            if (string.IsNullOrEmpty(auto.Newimagen))
            {
                auto.Newimagen = auto.Imagen; // Si no hay nueva imagen, usa la actual
            }

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE autos 
                         SET marca = @Marca, modelo = @Modelo, placa = @Placa, 
                             tipo = @Tipo, estado = @Estado, costodia = @CostoDia, 
                             imagen = @Imagen 
                         WHERE idauto = @Id";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Marca", auto.Marca);
                cmd.Parameters.AddWithValue("@Modelo", auto.Modelo);
                cmd.Parameters.AddWithValue("@Placa", auto.Placa);
                cmd.Parameters.AddWithValue("@Tipo", auto.Tipo);
                cmd.Parameters.AddWithValue("@Estado", auto.Estado);
                cmd.Parameters.AddWithValue("@CostoDia", auto.CostoDia);
                cmd.Parameters.AddWithValue("@Imagen", auto.Newimagen);
                cmd.Parameters.AddWithValue("@Id", auto.IdAuto);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public JsonResult UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Archivo inválido" });
            }

            try
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                // Generar un nombre único para la imagen
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return Json(new { success = true, fileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
        // Crear auto (GET)
        public IActionResult Crear()
        {
            return View();
        }
    }
}