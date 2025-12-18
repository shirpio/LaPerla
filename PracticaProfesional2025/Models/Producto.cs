using System.ComponentModel.DataAnnotations.Schema;
namespace PracticaProfesional2025.Models 
{

    public enum TipoVenta
    {
        Unidad,
        Kilogramo,
        Paquete
    }
    public class Producto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public TipoVenta Tipo { get; set; }
        public bool Activo { get; set; } = true;
    }
}

