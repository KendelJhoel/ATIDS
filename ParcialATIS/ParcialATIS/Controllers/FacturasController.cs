using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;
using System;
using SkiaSharp;

namespace ParcialATIS.Controllers
{
    public class FacturasController : Controller
    {
        private readonly DbController _dbController;

        public FacturasController()
        {
            _dbController = new DbController();
        }

        [HttpPost]
        public IActionResult Guardar(int cantidadDias, int idAuto)
        {
            var clienteId = HttpContext.Session.GetInt32("ClienteId");
            if (clienteId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var factura = new Factura();
            Cliente cliente;

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Obtener datos del cliente
                string queryCliente = "SELECT * FROM clientes WHERE idcliente = @IdCliente";
                MySqlCommand cmdCliente = new MySqlCommand(queryCliente, connection);
                cmdCliente.Parameters.AddWithValue("@IdCliente", clienteId);

                using (var reader = cmdCliente.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            IdCliente = Convert.ToInt32(reader["idcliente"]),
                            Nombre = reader["nombre"].ToString(),
                            Email = reader["email"].ToString()
                        };
                    }
                    else
                    {
                        return RedirectToAction("Login", "Account");
                    }
                }

                connection.Close();
                connection.Open();

                // Obtener datos del auto
                string queryAuto = "SELECT * FROM autos WHERE idauto = @IdAuto";
                MySqlCommand cmdAuto = new MySqlCommand(queryAuto, connection);
                cmdAuto.Parameters.AddWithValue("@IdAuto", idAuto);

                using (var reader = cmdAuto.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        factura.CostoDia = Convert.ToDouble(reader["costodia"]);
                        factura.Marca = reader["marca"].ToString();
                        factura.Modelo = reader["modelo"].ToString();
                        factura.Placa = reader["placa"].ToString();
                        factura.Tipo = reader["tipo"].ToString();
                    }
                }

                factura.Subtotal = cantidadDias * factura.CostoDia;
                factura.IVA = factura.Subtotal * 0.13;
                factura.Total = factura.Subtotal + factura.IVA;
                factura.CantidadDias = cantidadDias;

                connection.Close();
                connection.Open();

                // Insertar factura en la base de datos
                string query = @"INSERT INTO facturas (idcliente, idauto, fecha, subtotal, iva, total) 
                         VALUES (@IdCliente, @IdAuto, @Fecha, @Subtotal, @IVA, @Total)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdCliente", clienteId);
                cmd.Parameters.AddWithValue("@IdAuto", idAuto);
                cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@Subtotal", factura.Subtotal);
                cmd.Parameters.AddWithValue("@IVA", factura.IVA);
                cmd.Parameters.AddWithValue("@Total", factura.Total);

                cmd.ExecuteNonQuery();
                factura.IdFactura = (int)cmd.LastInsertedId;

                // Actualizar estado del vehículo a "No disponible"
                string updateAutoQuery = "UPDATE autos SET estado = 'No disponible' WHERE idauto = @IdAuto";
                MySqlCommand updateAutoCmd = new MySqlCommand(updateAutoQuery, connection);
                updateAutoCmd.Parameters.AddWithValue("@IdAuto", idAuto);
                updateAutoCmd.ExecuteNonQuery();
            }

            // Generar recibo y permitir al usuario guardarlo
            var recibo = GenerarRecibo(factura, cliente);

            if (recibo == null)
            {
                TempData["Error"] = "Ocurrió un error al generar el recibo. Intenta nuevamente.";
                return RedirectToAction("Factura", new { idAuto });
            }

            // Guardar archivo y manejar el diálogo
            Response.Headers.Add("Content-Disposition", $"attachment; filename={cliente.Nombre}_Factura_{factura.IdFactura}.png");
            return File(recibo.ToArray(), "image/png");
        }


        private MemoryStream GenerarRecibo(Factura factura, Cliente cliente)
        {
            int width = 800;
            int height = 600;

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);

            var paintTitle = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 36,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            var paintBody = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 24,
                IsAntialias = true
            };

            var paintGray = new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 24,
                IsAntialias = true
            };

            // Dibujar encabezado
            canvas.DrawText("Recibo de Facturación", 50, 50, paintTitle);

            // Dibujar información del cliente
            canvas.DrawText($"Cliente: {cliente.Nombre}", 50, 120, paintBody);
            canvas.DrawText($"Correo: {cliente.Email}", 50, 160, paintBody);

            // Dibujar información del auto
            canvas.DrawText($"Auto: {factura.Marca} {factura.Modelo}", 50, 220, paintBody);
            canvas.DrawText($"Placa: {factura.Placa}", 50, 260, paintBody);
            canvas.DrawText($"Tipo: {factura.Tipo}", 50, 300, paintBody);
            canvas.DrawText($"Costo por Día: ${factura.CostoDia:F2}", 50, 340, paintBody);

            // Dibujar detalles de la factura
            canvas.DrawText($"Días Rentados: {factura.CantidadDias}", 50, 400, paintBody);
            canvas.DrawText($"Subtotal: ${factura.Subtotal:F2}", 50, 440, paintBody);
            canvas.DrawText($"IVA (13%): ${factura.IVA:F2}", 50, 480, paintBody);
            canvas.DrawText($"Total: ${factura.Total:F2}", 50, 520, paintBody);

            // Dibujar fecha
            canvas.DrawText($"Fecha: {DateTime.Now:dd/MM/yyyy}", 50, 580, paintGray);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var memoryStream = new MemoryStream();
            data.SaveTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        public IActionResult Factura(int idAuto)
        {
            // Obtener el cliente actual desde la sesión
            var clienteId = HttpContext.Session.GetInt32("ClienteId");
            if (clienteId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirigir al login si no hay sesión activa
            }

            // Obtener datos del cliente
            var cliente = new Cliente();
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string queryCliente = "SELECT * FROM clientes WHERE idcliente = @IdCliente";
                MySqlCommand cmdCliente = new MySqlCommand(queryCliente, connection);
                cmdCliente.Parameters.AddWithValue("@IdCliente", clienteId);

                using (var reader = cmdCliente.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            IdCliente = clienteId.Value,
                            Nombre = reader["nombre"].ToString(),
                            Email = reader["email"].ToString()
                        };
                    }
                }
            }

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            // Obtener datos del auto
            var auto = new Auto();
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string queryAuto = "SELECT * FROM autos WHERE idauto = @IdAuto";
                MySqlCommand cmdAuto = new MySqlCommand(queryAuto, connection);
                cmdAuto.Parameters.AddWithValue("@IdAuto", idAuto);

                using (var reader = cmdAuto.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        auto = new Auto
                        {
                            IdAuto = idAuto,
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Placa = reader["placa"].ToString(),
                            Tipo = reader["tipo"].ToString(),
                            CostoDia = Convert.ToDouble(reader["costodia"]),
                            Imagen = reader["imagen"].ToString()
                        };
                    }
                }
            }

            if (auto == null)
            {
                return NotFound("El auto no existe.");
            }

            var factura = new Factura
            {
                IdAuto = auto.IdAuto,
                Imagen = auto.Imagen,
                Marca = auto.Marca,
                Modelo = auto.Modelo,
                Placa = auto.Placa,
                Tipo = auto.Tipo,
                CostoDia = auto.CostoDia
            };

            // Pasar datos adicionales a la vista
            ViewBag.ClienteNombre = cliente.Nombre;
            ViewBag.ClienteEmail = cliente.Email;

            return View(factura);
        }

        
        // Listar todas las facturas (GET)
        public IActionResult Index()
        {
            var facturas = new List<Factura>();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"SELECT f.idfactura, f.fecha, f.subtotal, f.iva, f.total, 
                                c.nombre AS Cliente, a.marca, a.modelo, a.placa 
                                FROM facturas f
                                JOIN clientes c ON f.idcliente = c.idcliente
                                JOIN autos a ON f.idauto = a.idauto";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        facturas.Add(new Factura
                        {
                            IdFactura = Convert.ToInt32(reader["idfactura"]),
                            Fecha = Convert.ToDateTime(reader["fecha"]),
                            Subtotal = Convert.ToDouble(reader["subtotal"]),
                            IVA = Convert.ToDouble(reader["iva"]),
                            Total = Convert.ToDouble(reader["total"]),
                            Cliente = reader["Cliente"].ToString(),
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Placa = reader["placa"].ToString()
                        });
                    }
                }
            }

            return View(facturas);
        }

        // Detalle de una factura específica (GET)
        public IActionResult Detalle(int id)
        {
            var factura = new Factura();

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = @"SELECT f.idfactura, f.fecha, f.subtotal, f.iva, f.total, 
                                c.nombre AS Cliente, c.email, a.marca, a.modelo, a.placa, a.tipo
                                FROM facturas f
                                JOIN clientes c ON f.idcliente = c.idcliente
                                JOIN autos a ON f.idauto = a.idauto
                                WHERE f.idfactura = @IdFactura";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdFactura", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        factura = new Factura
                        {
                            IdFactura = Convert.ToInt32(reader["idfactura"]),
                            Fecha = Convert.ToDateTime(reader["fecha"]),
                            Subtotal = Convert.ToDouble(reader["subtotal"]),
                            IVA = Convert.ToDouble(reader["iva"]),
                            Total = Convert.ToDouble(reader["total"]),
                            Cliente = reader["Cliente"].ToString(),
                            Correo = reader["email"].ToString(),
                            Marca = reader["marca"].ToString(),
                            Modelo = reader["modelo"].ToString(),
                            Placa = reader["placa"].ToString(),
                            Tipo = reader["tipo"].ToString()
                        };
                    }
                }
            }

            return View(factura);
        }

        // Eliminar una factura (POST)
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                // Obtener el id del auto asociado
                string queryAutoId = "SELECT idauto FROM facturas WHERE idfactura = @IdFactura";
                MySqlCommand cmdAutoId = new MySqlCommand(queryAutoId, connection);
                cmdAutoId.Parameters.AddWithValue("@IdFactura", id);
                int idAuto = Convert.ToInt32(cmdAutoId.ExecuteScalar());

                // Eliminar la factura
                string query = "DELETE FROM facturas WHERE idfactura = @IdFactura";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdFactura", id);
                cmd.ExecuteNonQuery();

                // Actualizar estado del vehículo a "Disponible"
                string updateAutoQuery = "UPDATE autos SET estado = 'Disponible' WHERE idauto = @IdAuto";
                MySqlCommand updateAutoCmd = new MySqlCommand(updateAutoQuery, connection);
                updateAutoCmd.Parameters.AddWithValue("@IdAuto", idAuto);
                updateAutoCmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Factura eliminada correctamente.";
            return RedirectToAction("Index");
        }

    }
}
