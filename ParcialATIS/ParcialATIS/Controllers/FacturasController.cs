using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;
using System;
using System.Collections.Generic;

namespace ParcialATIS.Controllers
{
    public class FacturasController : Controller
    {
        private readonly DbController _dbController;

        public FacturasController()
        {
            _dbController = new DbController();
        }

        // Vista para Crear Factura (GET)
        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.Clientes = ObtenerClientes();
            ViewBag.Autos = ObtenerAutos();
            return View();
        }

        // Crear Factura (POST)
        [HttpPost]
        public IActionResult Crear(int idCliente, int idAuto, int dias, DateTime fecha)
        {
            double costoDia = ObtenerCostoDia(idAuto);
            double subtotal = costoDia * dias;
            double iva = subtotal * 0.13;
            double total = subtotal + iva;

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO facturas (idcliente, idauto, fecha, subtotal, iva, total) 
                                 VALUES (@IdCliente, @IdAuto, @Fecha, @Subtotal, @IVA, @Total)";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                cmd.Parameters.AddWithValue("@IdAuto", idAuto);
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                cmd.Parameters.AddWithValue("@IVA", iva);
                cmd.Parameters.AddWithValue("@Total", total);

                cmd.ExecuteNonQuery();
            }

            ViewBag.Success = "Factura generada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            List<Factura> facturas = new List<Factura>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM facturas";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        facturas.Add(new Factura
                        {
                            IdFactura = Convert.ToInt32(reader["idfactura"]),
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            IdAuto = Convert.ToInt32(reader["idauto"]),
                            IdEmpleado = Convert.ToInt32(reader["idempleado"]),
                            Fecha = reader["fecha"] as DateTime?,
                            Subtotal = reader["subtotal"] as double?,
                            IVA = reader["iva"] as double?,
                            Total = reader["total"] as double?
                        });
                    }
                }
            }

            return View(facturas);
        }


        private List<Cliente> ObtenerClientes()
        {
            var clientes = new List<Cliente>();
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM clientes";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            Nombre = reader["nombre"].ToString()
                        });
                    }
                }
            }
            return clientes;
        }

        private List<Auto> ObtenerAutos()
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
                            Marca = reader["marca"].ToString()
                        });
                    }
                }
            }
            return autos;
        }

        private double ObtenerCostoDia(int idAuto)
        {
            double costoDia = 0;
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "SELECT costodia FROM autos WHERE idauto = @IdAuto";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdAuto", idAuto);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        costoDia = Convert.ToDouble(reader["costodia"]);
                    }
                }
            }
            return costoDia;
        }
    }
}
