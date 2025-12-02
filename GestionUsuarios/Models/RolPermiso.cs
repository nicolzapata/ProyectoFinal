using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionUsuarios.Models
{
    public class RolPermiso
    {
        [Key]
        public int RolPermisoId { get; set; }

        [Required]
        [StringLength(450)]
        public string? RolId { get; set; }

        [Required]
        public int PermisoId { get; set; }

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        // Relación con Permiso
        [ForeignKey("PermisoId")]
        public virtual Permiso? Permiso { get; set; }
    }
}