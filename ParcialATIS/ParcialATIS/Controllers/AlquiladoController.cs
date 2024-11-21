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

        public IActionResult Index()
        {
            var alquileres = new List<Alquilado>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"
                    SELECT a.idalquiler, a.idauto, a.idcliente, a.idempleado, a.idfactura, a.fecha, a.fecha_devolver, c.nombre, au.marca, au.modelo
                    FROM alquilados a
                    JOIN clientes c ON a.idcliente = c.idcliente
                    JOIN autos au ON a.idauto = au.idauto
                    WHERE au.estado = 'No disponible'";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        alquileres.Add(new Alquilado
                        {
                            IdAlquiler = Convert.ToInt32(reader["idalquiler"]),
                            IdAuto = Convert.ToInt32(reader["idauto"]),
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            IdEmpleado = Convert.ToInt32(reader["idempleado"]),
                            IdFactura = Convert.ToInt32(reader["idfactura"]),
                            Fecha = Convert.ToDateTime(reader["fecha"]),
                            FechaDevolver = Convert.ToDateTime(reader["fecha_devolver"])
                        });
                    }
                }
            }

            return View(alquileres);
        }

        // Registrar devolución (POST)
        [HttpPost]
        public IActionResult Devolver(int idAlquiler)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Actualizar estado del auto
                string updateAutoQuery = @"
                    UPDATE autos
                    SET estado = 'Disponible'
                    WHERE idauto = (SELECT idauto FROM alquilados WHERE idalquiler = @IdAlquiler)";
                MySqlCommand updateAutoCmd = new MySqlCommand(updateAutoQuery, connection);
                updateAutoCmd.Parameters.AddWithValue("@IdAlquiler", idAlquiler);
                updateAutoCmd.ExecuteNonQuery();

                // Eliminar el registro de la tabla alquilados
                string deleteAlquilerQuery = "DELETE FROM alquilados WHERE idalquiler = @IdAlquiler";
                MySqlCommand deleteAlquilerCmd = new MySqlCommand(deleteAlquilerQuery, connection);
                deleteAlquilerCmd.Parameters.AddWithValue("@IdAlquiler", idAlquiler);
                deleteAlquilerCmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }


        //[HttpPost]
        //public IActionResult RentarAuto(int idAuto, int idCliente, int idEmpleado, int idFactura, DateTime fecha, DateTime fechaDevolver)
        //{
        //    using (var connection = _dbController.GetConnection())
        //    {
        //        connection.Open();
        //        string query = @"
        //    INSERT INTO alquilados (idauto, idcliente, idempleado, idfactura, fecha, fecha_devolver)
        //    VALUES (@IdAuto, @IdCliente, @IdEmpleado, @IdFactura, @Fecha, @FechaDevolver)";
        //        MySqlCommand cmd = new MySqlCommand(query, connection);
        //        cmd.Parameters.AddWithValue("@IdAuto", idAuto);
        //        cmd.Parameters.AddWithValue("@IdCliente", idCliente);
        //        cmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
        //        cmd.Parameters.AddWithValue("@IdFactura", idFactura);
        //        cmd.Parameters.AddWithValue("@Fecha", fecha);
        //        cmd.Parameters.AddWithValue("@FechaDevolver", fechaDevolver);

        //        cmd.ExecuteNonQuery();

        //        // Actualizar estado del auto
        //        string updateAutoQuery = "UPDATE autos SET estado = 'No disponible' WHERE idauto = @IdAuto";
        //        MySqlCommand updateAutoCmd = new MySqlCommand(updateAutoQuery, connection);
        //        updateAutoCmd.Parameters.AddWithValue("@IdAuto", idAuto);
        //        updateAutoCmd.ExecuteNonQuery();
        //    }

        //    return RedirectToAction("Index", "Alquilados");
        //}
         

    }
}
