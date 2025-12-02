using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionUsuarios.Models
{
    public class Permiso
    {
        [Key]
        public int PermisoId { get; set; }

        [Required]
        public int ModuloId { get; set; }

        [Required]
        [StringLength(100)]
        public string NombrePermiso { get; set; }

        [Required]
        [StringLength(50)]
        public string CodigoPermiso { get; set; }

        [StringLength(255)]
        public string? Descripcion { get; set; }

        // Relación con Módulo
        [ForeignKey("ModuloId")]
        public virtual Modulo Modulo { get; set; }

        // Relación con RolPermisos
        public virtual ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}