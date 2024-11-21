using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;
using System.Collections.Generic;

namespace ParcialATIS.Controllers
{
    public class DevolucionesController : Controller
    {
        private readonly DbController _dbController;

        public DevolucionesController()
        {
            _dbController = new DbController();
        }

        public IActionResult Index()
        {
            var viewModel = new DevolucionViewModel();

            // Obtener clientes y autos alquilados del usuario en sesión
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Obtener clientes con autos alquilados
                string clientesQuery = @"
            SELECT DISTINCT c.idcliente, c.nombre
            FROM clientes c
            JOIN alquilados al ON c.idcliente = al.idcliente";
                MySqlCommand clientesCmd = new MySqlCommand(clientesQuery, connection);

                using (var reader = clientesCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.Clientes.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            Nombre = reader["nombre"].ToString()
                        });
                    }
                }

                // Obtener autos alquilados para el cliente en sesión (si aplica)
                if (HttpContext.Session.GetInt32("ClienteId") != null)
                {
                    int clienteId = HttpContext.Session.GetInt32("ClienteId").Value;

                    string autosQuery = @"
                SELECT a.idauto, a.marca, a.modelo, a.imagen, f.total, f.idfactura
                FROM autos a
                JOIN alquilados al ON a.idauto = al.idauto
                JOIN facturas f ON f.idfactura = al.idfactura
                WHERE al.idcliente = @ClienteId";
                    MySqlCommand autosCmd = new MySqlCommand(autosQuery, connection);
                    autosCmd.Parameters.AddWithValue("@ClienteId", clienteId);

                    using (var reader = autosCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            viewModel.AutosAlquilados.Add(new Auto
                            {
                                IdAuto = Convert.ToInt32(reader["idauto"]),
                                Marca = reader["marca"].ToString(),
                                Modelo = reader["modelo"].ToString(),
                                Imagen = reader["imagen"].ToString(),
                                CostoDia = Convert.ToDouble(reader["total"]) // Reutilizando el campo CostoDia como Total
                            });
                        }
                    }
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ObtenerAutosAlquilados(int clienteId)
        {
            var autosAlquilados = new List<Auto>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT a.idauto, a.marca, a.modelo, a.placa
                    FROM autos a
                    JOIN alquilados al ON a.idauto = al.idauto
                    WHERE al.idcliente = @IdCliente";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdCliente", clienteId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        autosAlquilados.Add(new Auto
                        {
                            IdAuto = Convert.ToInt32(reader["idauto"]),
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Placa = reader["placa"].ToString()
                        });
                    }
                }
            }

            return Json(autosAlquilados);
        }

        public IActionResult HistorialCl()
        {
            var autosEnCurso = new List<Auto>();

            // Obtener autos alquilados del usuario en sesión
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT a.idauto, a.marca, a.modelo, a.imagen, f.total, f.idfactura
                    FROM autos a
                    JOIN alquilados al ON a.idauto = al.idauto
                    JOIN facturas f ON f.idfactura = al.idfactura
                    WHERE al.idcliente = @ClienteId";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ClienteId", HttpContext.Session.GetInt32("ClienteId"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        autosEnCurso.Add(new Auto
                        {
                            IdAuto = Convert.ToInt32(reader["idauto"]),
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Imagen = reader["imagen"].ToString(),
                            CostoDia = Convert.ToDouble(reader["total"])
                        });
                    }
                }
            }

            return View(autosEnCurso);
        }

        [HttpPost]
        public IActionResult Devolver(int autoId)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Obtener datos del auto y factura para el historial
                string selectQuery = @"
                    SELECT al.idfactura, al.idcliente
                    FROM alquilados al
                    WHERE al.idauto = @IdAuto";
                MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                selectCmd.Parameters.AddWithValue("@IdAuto", autoId);

                int idFactura = 0;
                int idCliente = 0;

                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        idFactura = Convert.ToInt32(reader["idfactura"]);
                        idCliente = Convert.ToInt32(reader["idcliente"]);
                    }
                }

                // Insertar en historial
                string insertQuery = @"
                    INSERT INTO historial (id_factura, id_auto, id_cliente, estado_auto, fecha)
                    VALUES (@IdFactura, @IdAuto, @IdCliente, 'Auto devuelto', NOW())";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@IdFactura", idFactura);
                insertCmd.Parameters.AddWithValue("@IdAuto", autoId);
                insertCmd.Parameters.AddWithValue("@IdCliente", idCliente);
                insertCmd.ExecuteNonQuery();

                // Eliminar de la tabla alquilados
                string deleteQuery = "DELETE FROM alquilados WHERE idauto = @IdAuto";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@IdAuto", autoId);
                deleteCmd.ExecuteNonQuery();

                // Actualizar estado del auto
                string updateQuery = "UPDATE autos SET estado = 'Disponible' WHERE idauto = @IdAuto";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@IdAuto", autoId);
                updateCmd.ExecuteNonQuery();
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult Devolucion(int clienteId, int idAuto)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Obtener datos necesarios del auto y factura para el historial
                string selectQuery = @"
            SELECT al.idfactura
            FROM alquilados al
            WHERE al.idauto = @IdAuto AND al.idcliente = @IdCliente";
                MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                selectCmd.Parameters.AddWithValue("@IdAuto", idAuto);
                selectCmd.Parameters.AddWithValue("@IdCliente", clienteId);

                int idFactura = 0;

                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        idFactura = Convert.ToInt32(reader["idfactura"]);
                    }
                    else
                    {
                        TempData["Error"] = "No se encontró el registro correspondiente para la devolución.";
                        return RedirectToAction("Index");
                    }
                }

                // Insertar en historial
                string insertQuery = @"
            INSERT INTO historial (id_factura, id_auto, id_cliente, estado_auto, fecha)
            VALUES (@IdFactura, @IdAuto, @IdCliente, 'Auto devuelto', NOW())";
                MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                insertCmd.Parameters.AddWithValue("@IdFactura", idFactura);
                insertCmd.Parameters.AddWithValue("@IdAuto", idAuto);
                insertCmd.Parameters.AddWithValue("@IdCliente", clienteId);

                try
                {
                    insertCmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    TempData["Error"] = $"Error al guardar en el historial: {ex.Message}";
                    return RedirectToAction("Index");
                }

                // Eliminar de la tabla alquilados
                string deleteQuery = "DELETE FROM alquilados WHERE idauto = @IdAuto AND idcliente = @IdCliente";
                MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                deleteCmd.Parameters.AddWithValue("@IdAuto", idAuto);
                deleteCmd.Parameters.AddWithValue("@IdCliente", clienteId);
                deleteCmd.ExecuteNonQuery();

                // Actualizar estado del auto
                string updateQuery = "UPDATE autos SET estado = 'Disponible' WHERE idauto = @IdAuto";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@IdAuto", idAuto);
                updateCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "El auto ha sido devuelto con éxito.";
            return RedirectToAction("Index");
        }

        public IActionResult IndexAdmin()
        {
            var viewModel = new DevolucionViewModel();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Llenar la lista de clientes
                string clientesQuery = @"
                    SELECT DISTINCT c.idcliente, c.nombre
                    FROM clientes c
                    JOIN alquilados a ON c.idcliente = a.idcliente";

                MySqlCommand clientesCmd = new MySqlCommand(clientesQuery, connection);

                using (var reader = clientesCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.Clientes.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            Nombre = reader["nombre"].ToString()
                        });
                    }
                }

                // Llenar la lista del historial
                string historialQuery = @"
                    SELECT 
                        h.id_factura,
                        CONCAT(a.marca, ' ', a.modelo) AS auto,
                        c.nombre AS nombre_cliente,
                        h.estado_auto,
                        h.fecha
                    FROM historial h
                    JOIN autos a ON h.id_auto = a.idauto
                    JOIN clientes c ON h.id_cliente = c.idcliente";

                MySqlCommand historialCmd = new MySqlCommand(historialQuery, connection);

                using (var reader = historialCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        viewModel.Historial.Add(new HistorialViewModel
                        {
                            IdFactura = Convert.ToInt32(reader["id_factura"]),
                            Auto = reader["auto"].ToString(),
                            NombreCliente = reader["nombre_cliente"].ToString(),
                            Estado = reader["estado_auto"].ToString(),
                            dFecha = Convert.ToDateTime(reader["fecha"])
                        });
                    }
                }
            }

            return View(viewModel);
        }
    }
}
