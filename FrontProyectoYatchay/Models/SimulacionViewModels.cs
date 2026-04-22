namespace FrontProyectoYatchay.Models
{
    // Para mostrar la pregunta actual
    public class SimulacionViewModels
    {
        public int IdContent { get; set; }
        public int Fase { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Opciones { get; set; } = string.Empty;
    }
}
