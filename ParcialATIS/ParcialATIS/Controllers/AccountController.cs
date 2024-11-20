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
        private const string AdminPassword = "adpsswrd";
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
            // vldr admin
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

            // cliente existe (sí, no) en bd
            using (var connection = _dbController.GetConnection())
            {
                connection.Open();

                string checkUserQuery = "SELECT * FROM clientes WHERE email = @Email";
                MySqlCommand checkUserCmd = new MySqlCommand(checkUserQuery, connection);
                checkUserCmd.Parameters.AddWithValue("@Email", email);

                using (var reader = checkUserCmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        ViewBag.Error = "El usuario no está registrado.";
                        return View();
                    }
                }

                connection.Close();
                connection.Open();

                // valdr las credenciales
                string credentialsQuery = "SELECT * FROM clientes WHERE email = @Email AND dui = @Password";
                MySqlCommand credentialsCmd = new MySqlCommand(credentialsQuery, connection);
                credentialsCmd.Parameters.AddWithValue("@Email", email);
                credentialsCmd.Parameters.AddWithValue("@Password", password);

                using (var reader = credentialsCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
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

        // Register (POST)
        [HttpPost]
        public IActionResult Register(string name, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View("Login");
            }

            using (var connection = _dbController.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO clientes (nombre, direccion, telefono, dui, email) VALUES (@Nombre, '', '', @Password, @Email)";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Nombre", name);
                cmd.Parameters.AddWithValue("@Password", password);
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }



    }
}
