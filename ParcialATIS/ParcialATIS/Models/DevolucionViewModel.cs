namespace ParcialATIS.Models
{
    public class DevolucionViewModel
    {

        public int ClienteId { get; set; }
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();  // Inicializa como una lista vacía
        public List<Auto> AutosAlquilados { get; set; } = new List<Auto>(); // Lo mismo para los autos

    }
}
