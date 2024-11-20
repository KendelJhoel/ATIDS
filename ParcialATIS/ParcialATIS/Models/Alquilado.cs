namespace ParcialATIS.Models
{
    public class Alquilado
    {
        public int IdAlquiler { get; set; }
        public int? IdAuto { get; set; }
        public int? IdCliente { get; set; }
        public int? IdEmpleado { get; set; }
        public int? IdFactura { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? FechaDevolver { get; set; }
    }
}
