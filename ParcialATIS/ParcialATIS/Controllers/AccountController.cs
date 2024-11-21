using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParcialATIS.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ParcialATIS.Controllers
{
    public class AccountController : Controller
    {
        private const string AdminEmail = "admin@admin";
        private const string AdminPassword = "adps";
        private readonly DbController _dbController;

        public AccountController()
        {
            _dbController = new DbController();
        }

        // Vista de Login (GET)
        public IActionResult Login()
        {
            return View();
        }

        // Login (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Validar admin
            if (email == AdminEmail && password == AdminPassword)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Administrador"),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                return RedirectToAction("Index", "AdminView");
            }

            // Validar cliente en la base de datos
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                string credentialsQuery = "SELECT * FROM clientes WHERE email = @Email AND dui = @Password";
                MySqlCommand credentialsCmd = new MySqlCommand(credentialsQuery, connection);
                credentialsCmd.Parameters.AddWithValue("@Email", email);
                credentialsCmd.Parameters.AddWithValue("@Password", password);

                using (var reader = credentialsCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Guardar ClienteId en la sesión
                        int clienteId = Convert.ToInt32(reader["idcliente"]);
                        HttpContext.Session.SetInt32("ClienteId", clienteId);

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, reader["nombre"].ToString()),
                            new Claim(ClaimTypes.Email, email),
                            new Claim(ClaimTypes.Role, "Cliente")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties();

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties
                        );

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.Error = "Credenciales incorrectas.";
            return View();
        }

        [HttpPost]
        public IActionResult Register(string name, string direccion, string telefono, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View("Login");
            }

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                // La contraseña se almacena en el campo `dui`
                string query = "INSERT INTO clientes (nombre, direccion, telefono, dui, email) VALUES (@Nombre, @Direccion, @Telefono, @Password, @Email)";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Nombre", name);
                cmd.Parameters.AddWithValue("@Direccion", direccion);
                cmd.Parameters.AddWithValue("@Telefono", telefono);
                cmd.Parameters.AddWithValue("@Password", password); // Se almacena en el campo `dui`
                cmd.Parameters.AddWithValue("@Email", email);

                cmd.ExecuteNonQuery();
            }

            ViewBag.Success = "Usuario registrado con éxito.";
            return View("Login");
        }

        // Logout (POST)
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear(); // Limpiar la sesión
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
