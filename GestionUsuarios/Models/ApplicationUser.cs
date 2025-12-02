using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestionUsuarios.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; }

        [StringLength(20)]
        public string? NumeroDocumento { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public DateTime? UltimoAcceso { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(255)]
        public string? Observaciones { get; set; }
    }
}