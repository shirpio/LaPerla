using System.ComponentModel.DataAnnotations.Schema;
namespace PracticaProfesional2025.Models
{


    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public ICollection<DetalleVenta>? Detalles { get; set; }
    }
}
