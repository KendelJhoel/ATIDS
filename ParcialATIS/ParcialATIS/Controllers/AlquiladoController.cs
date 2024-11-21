using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;
using System.Collections.Generic;

namespace ParcialATIS.Controllers
{
    public class AlquiladoController : Controller
    {
        private readonly DbController _dbController;

        public AlquiladoController()
        {
            _dbController = new DbController();
        }

        [HttpPost]
        public IActionResult Devolucion(int clienteId, int idAuto)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Obtener datos del auto y factura para el historial
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
                insertCmd.ExecuteNonQuery();

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

        // Selección de autos alquilados por cliente
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

            return Json(autosAlquilados); // Retornamos los datos en JSON
        }

        // Proceso de devolución de auto
        [HttpPost]
        public IActionResult Devolver(int clienteId, int idAuto)
        {

            var viewModel = new DevolucionViewModel();
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Eliminar registro de alquiler
                string deleteQuery = "DELETE FROM alquilados WHERE idcliente = @IdCliente AND idauto = @IdAuto";
                MySqlCommand cmd = new MySqlCommand(deleteQuery, connection);
                cmd.Parameters.AddWithValue("@IdCliente", clienteId);
                cmd.Parameters.AddWithValue("@IdAuto", idAuto);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    TempData["Success"] = "El auto ha sido devuelto con éxito.";
                }
                else
                {
                    TempData["Error"] = "No se pudo procesar la devolución. Intenta nuevamente.";
                }
            }

            return View("Index", viewModel);
        }
    }
}
