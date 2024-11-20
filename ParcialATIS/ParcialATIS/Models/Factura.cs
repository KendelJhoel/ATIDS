namespace ParcialATIS.Models
{
    public class Factura
    {
        public int IdFactura { get; set; }
        public int? IdCliente { get; set; }
        public int? IdAuto { get; set; }
        public int? IdEmpleado { get; set; }
        public DateTime? Fecha { get; set; }
        public double? Subtotal { get; set; }
        public double? Iva { get; set; }
        public double? Total { get; set; }
    }
}
