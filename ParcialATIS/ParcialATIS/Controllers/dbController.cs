using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace ParcialATIS.Controllers
{
    public class DbController : Controller
    {
        private readonly string connectionString = "Server=atids.online;Database=atidsuser_rCar;User Id=atidsuser_rCaruser;Password=12345;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
