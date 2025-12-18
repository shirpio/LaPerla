namespace PracticaProfesional2025.ViewModels
{
    public class VentaViewModel
    {
        public List<DetalleTemporal> Detalles { get; set; } = new List<DetalleTemporal>();

        public decimal Total => Detalles.Sum(d => d.Subtotal);

        public class DetalleTemporal
        {
            public int ProductoId { get; set; }
            public string NombreProducto { get; set; } = "";
            public string TipoVenta { get; set; } = "";
            public decimal Cantidad { get; set; }   // para kg: dinero ingresado
            public decimal Precio { get; set; }     // precio por unidad o por kg
            public decimal Subtotal { get; set; }   // ya calculado
            public DateTime Fecha { get; set; }

        }
    }
}

