namespace ParcialATIS.Models
{
    public class DevolucionViewModel
    {
        public int ClienteId { get; set; }
        public List<HistorialViewModel> Historial { get; set; } = new List<HistorialViewModel>();

        public List<Cliente> Clientes { get; set; } = new List<Cliente>(); // Lista de clientes disponibles
        public List<Auto> AutosAlquilados { get; set; } = new List<Auto>(); // Lista de autos alquilados

        // Constructor opcional para inicializar el ViewModel
        public DevolucionViewModel() { }

        public DevolucionViewModel(List<Cliente> clientes, List<Auto> autosAlquilados)
        {
            Clientes = clientes ?? new List<Cliente>();
            AutosAlquilados = autosAlquilados ?? new List<Auto>();
        }
    }
}
