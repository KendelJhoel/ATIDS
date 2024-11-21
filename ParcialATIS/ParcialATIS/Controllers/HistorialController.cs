using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;
using System.Collections.Generic;

namespace ParcialATIS.Controllers
{
    public class HistorialController : Controller
    {
        private readonly DbController _dbController;

        public HistorialController()
        {
            _dbController = new DbController();
        }

        [HttpGet]
        public IActionResult ObtenerHistorial()
        {
            var historial = new List<HistorialViewModel>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Consulta para obtener el historial
                string query = @"
                    SELECT h.id_factura, a.marca, a.modelo, h.estado_auto, h.fecha
                    FROM historial h
                    JOIN autos a ON h.id_auto = a.idauto
                    WHERE h.id_cliente = @IdCliente";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdCliente", HttpContext.Session.GetInt32("ClienteId"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historial.Add(new HistorialViewModel
                        {
                            IdFactura = Convert.ToInt32(reader["id_factura"]),
                            Auto = $"{reader["marca"]} {reader["modelo"]}", // Concatenar Marca y Modelo
                            Estado = reader["estado_auto"].ToString(),
                            Fecha = Convert.ToDateTime(reader["fecha"]).ToString("yyyy-MM-dd") // Formatear la fecha
                        });
                    }
                }
            }

            return Json(historial); // Retornar historial en formato JSON
        }

        [HttpGet]
        public JsonResult ObtenerHistorialAdmin()
        {
            var historial = new List<HistorialViewModel>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Consulta para obtener el historial general
                string query = @"
            SELECT 
                h.id_factura,
                CONCAT(a.marca, ' ', a.modelo) AS auto,
                c.nombre AS nombre_cliente,
                h.estado_auto,
                h.fecha
            FROM historial h
            JOIN autos a ON h.id_auto = a.idauto
            JOIN clientes c ON h.id_cliente = c.idcliente";

                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historial.Add(new HistorialViewModel
                        {
                            IdFactura = Convert.ToInt32(reader["id_factura"]),
                            Auto = reader["auto"].ToString(),
                            NombreCliente = reader["nombre_cliente"].ToString(),
                            Estado = reader["estado_auto"].ToString(),
                            Fecha = Convert.ToDateTime(reader["fecha"]).ToString("yyyy-MM-dd HH:mm:ss") // Formato de fecha completo
                        });
                    }
                }
            }

            return Json(historial); // Retornar datos del historial como JSON
        }

    }
}
