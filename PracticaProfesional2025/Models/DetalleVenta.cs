using System.ComponentModel.DataAnnotations.Schema;
namespace PracticaProfesional2025.Models

{
    public class DetalleVenta
    {
        public int Id { get; set; }

        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Column(TypeName = "decimal(18,2)")]

        public decimal Cantidad { get; set; } // Unidad o kg o paquete
        [Column(TypeName = "decimal(18,2)")]

        public decimal DineroIngresado { get; set; } // Solo se usa para kg
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Subtotal { get; set; } // Para la venta
    }

}
