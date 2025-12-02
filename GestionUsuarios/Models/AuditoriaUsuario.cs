using System.ComponentModel.DataAnnotations;

namespace GestionUsuarios.Models
{
    public class AuditoriaUsuario
    {
        [Key]
        public int AuditoriaId { get; set; }

        [Required]
        [StringLength(450)]
        public string UsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Accion { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public DateTime FechaAccion { get; set; } = DateTime.Now;

        [StringLength(45)]
        public string? DireccionIP { get; set; }
    }
}