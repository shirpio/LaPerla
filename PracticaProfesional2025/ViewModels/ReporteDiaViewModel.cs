namespace PracticaProfesional2025.ViewModels
{
    public class ReporteDiaViewModel
    {
        public DateTime Fecha { get; set; }

        public decimal? TotalDia { get; set; }

        // =========================
        // DASHBOARD
        // =========================

        public decimal TotalGeneral { get; set; }

        public int CantidadVentas { get; set; }

        public string ProductoUnidadTop { get; set; } = "";

        public decimal CantidadUnidadTop { get; set; }

        public string ProductoKgTop { get; set; } = "";

        public decimal TotalKgTop { get; set; }
    }
}
