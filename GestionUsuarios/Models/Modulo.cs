using System.ComponentModel.DataAnnotations;

namespace GestionUsuarios.Models
{
    public class Modulo
    {
        [Key]
        public int ModuloId { get; set; }

        [Required]
        [StringLength(100)]
        public string NombreModulo { get; set; }

        [StringLength(255)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        public int? Orden { get; set; }

        public bool Activo { get; set; } = true;

        // Relación con Permisos
        public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
    }
}