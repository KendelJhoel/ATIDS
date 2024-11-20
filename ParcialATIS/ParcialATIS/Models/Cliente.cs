using System.ComponentModel.DataAnnotations;

namespace ParcialATIS.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(20, ErrorMessage = "El nombre no puede exceder los 20 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(20, ErrorMessage = "La dirección no puede exceder los 20 caracteres.")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\d{4}-\d{4}$", ErrorMessage = "El formato del Número es inválido (Ejemplo: ####-####).")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(200, ErrorMessage = "La contraseña no puede exceder los 20 caracteres.")]
        public string DUI { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido. ~~~@~~~")]
        public string Email { get; set; }
    }
}
