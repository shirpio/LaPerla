namespace PracticaProfesional2025.ViewModels
{
    public class VentaViewModel
    {
        public List<DetalleTemporal> Detalles { get; set; } = new List<DetalleTemporal>();

        public decimal Total => Detalles.Sum(d => d.Subtotal);

        // ===== EDITAR =====
        public int? EditIndex { get; set; }

        public int? ProductoEditarId { get; set; }

        public decimal? CantidadEditar { get; set; }

        public class DetalleTemporal
        {
            public int ProductoId { get; set; }

            public string NombreProducto { get; set; } = "";

            public string TipoVenta { get; set; } = "";

            public decimal Cantidad { get; set; }

            public decimal Precio { get; set; }

            public decimal Subtotal { get; set; }

            public DateTime Fecha { get; set; }
        }
    }
}

