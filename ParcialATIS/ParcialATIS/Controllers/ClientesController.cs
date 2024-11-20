using Microsoft.AspNetCore.Mvc;
using ParcialATIS.Models;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ParcialATIS.Controllers
{
    public class ClientesController : Controller
    {
        private readonly DbController _dbController;

        public ClientesController()
        {
            _dbController = new DbController();
        }

        // Listar clientes (GET)
        public IActionResult Index()
        {
            var clientes = new List<Cliente>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT idcliente, nombre, direccion, telefono, email, DUI FROM clientes"; // Excluyendo 'dui'
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            Nombre = reader["nombre"].ToString(),
                            Direccion = reader["direccion"].ToString(),
                            Telefono = reader["telefono"].ToString(),
                            Email = reader["email"].ToString()
                        });
                    }
                }
            }

            return View(clientes);
        }

        // Editar cliente (GET)
        [HttpGet]
        public IActionResult Editar(int id)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM clientes WHERE idcliente = @Id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var cliente = new Cliente
                        {
                            IdCliente = reader.GetInt32("idcliente"),
                            Nombre = reader.GetString("nombre"),
                            Direccion = reader.GetString("direccion"),
                            Telefono = reader.GetString("telefono"),
                            DUI = reader.GetString("dui"),
                            Email = reader.GetString("email")
                        };
                        return View(cliente);
                    }
                }
            }

            ViewBag.Error = "Cliente no encontrado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                // Imprimir errores en consola para depuración
                foreach (var error in ModelState.Values)
                {
                    foreach (var subError in error.Errors)
                    {
                        Console.WriteLine($"Error: {subError.ErrorMessage}");
                    }
                }

                ViewBag.Error = "Datos inválidos. Por favor verifica los campos.";
                return View(cliente);
            }

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE clientes 
                         SET nombre = @Nombre, 
                             direccion = @Direccion, 
                             telefono = @Telefono, 
                             dui = @DUI, 
                             email = @Email 
                         WHERE idcliente = @Id";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion);
                cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                cmd.Parameters.AddWithValue("@DUI", cliente.DUI);
                cmd.Parameters.AddWithValue("@Email", cliente.Email);
                cmd.Parameters.AddWithValue("@Id", cliente.IdCliente);

                int filasAfectadas = cmd.ExecuteNonQuery();

                if (filasAfectadas == 0)
                {
                    ViewBag.Error = "No se pudo actualizar el cliente. Por favor intenta nuevamente.";
                    return View(cliente);
                }
            }

            ViewBag.Success = "Cliente actualizado con éxito.";
            return RedirectToAction(nameof(Index));
        }


        // Eliminar cliente (GET)
        public IActionResult Eliminar(int id)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM clientes WHERE idcliente = @Id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }

            ViewBag.Success = "Cliente eliminado con éxito.";
            return RedirectToAction(nameof(Index));
        }
    }

}
