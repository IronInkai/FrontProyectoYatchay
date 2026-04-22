namespace FrontProyectoYatchay.Models
{
    // Para recibir el feedback después de elegir
    public class FeedbackViewModel
    {
        public string Titulo { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public int PuntajeObtenido { get; set; }
        public bool PuedeSiguiente { get; set; }
    }
}
