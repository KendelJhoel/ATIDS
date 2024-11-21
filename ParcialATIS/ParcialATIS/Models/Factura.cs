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
        public double? IVA { get; set; }
        public double? Total { get; set; }

        // Propiedades para el recibo (no se guardan en la BD)
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Placa { get; set; }
        public string Tipo { get; set; }
        public double CostoDia { get; set; }
        public string Imagen { get; set; }
        public double CantidadDias { get; set; }

        public string Cliente { get; set; }

        public string Correo { get; set; }

    }
}
