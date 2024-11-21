using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;

namespace ParcialATIS.Controllers
{
    public class AlquiladoController : Controller
    {

        private readonly DbController _dbController;

        public AlquiladoController()
        {
            _dbController = new DbController();
        }


        public IActionResult Devolucion()
        {
            var clientesConAutosAlquilados = new List<Cliente>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT DISTINCT c.idcliente, c.nombre
                    FROM clientes c
                    JOIN alquilados a ON c.idcliente = a.idcliente";


                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientesConAutosAlquilados.Add(new Cliente
                        {
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            Nombre = reader["nombre"].ToString()
                        });
                    }
                }
            }

            // Asegúrate de pasar los clientes correctamente al ViewModel
            var viewModel = new DevolucionViewModel
            {
                Clientes = clientesConAutosAlquilados,
                AutosAlquilados = new List<Auto>() // Esto puede actualizarse luego con AJAX
            };

            return View(viewModel); // Pasamos el ViewModel con las listas llenas
        }


        public IActionResult SeleccionarCliente(int clienteId)
        {
            var autosAlquilados = new List<Auto>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"SELECT a.idauto, a.marca, a.modelo, a.placa, a.tipo, a.estado
                         FROM autos a
                         JOIN alquilados al ON a.idauto = al.idauto
                         WHERE al.idcliente = @IdCliente AND al.estado = 'Alquilado'"; // Solo los autos alquilados

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
                            Placa = reader["placa"].ToString(),
                            Tipo = reader["tipo"].ToString(),
                            Estado = reader["estado"].ToString()
                        });
                    }
                }
            }

            ViewBag.ClienteId = clienteId; // Guardamos el clienteId para la acción de devolución
            return View(autosAlquilados); // Pasamos los autos alquilados a la vista
        }

        public IActionResult ObtenerAutosAlquilados(int clienteId)
        {
            var autosAlquilados = new List<Auto>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"SELECT a.idauto, a.marca, a.modelo, a.placa
                         FROM autos a
                         JOIN alquilados al ON a.idauto = al.idauto
                         WHERE al.idcliente = @IdCliente AND al.estado = 'Alquilado'"; // Solo los autos alquilados

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

            return Json(autosAlquilados); // Devolvemos los datos en formato JSON para ser procesados en la vista
        }

    }
}
