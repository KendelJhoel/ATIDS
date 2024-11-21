namespace ParcialATIS.Models
{
    public class HistorialViewModel
    {
        public int IdFactura { get; set; }
        public string Auto { get; set; }
        public string Estado { get; set; }
        public string Fecha { get; set; }

        public DateTime dFecha { get; set; }
        public string NombreCliente { get; set; }
    }
}
