using System.Text.Json.Serialization;

namespace FrontProyectoYatchay.Models
{
    // Para mostrar la pregunta actual
    public class SimulacionViewModel
    {
        public int IdSession { get; set; } 
        public int IdContent { get; set; }
        public int Fase { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty; 
        public List<OpcionViewModel> ListadoOpciones { get; set; } = new();
    }

    public class OpcionViewModel
    {
        [JsonPropertyName("id_option")]
        public int Id { get; set; }

        [JsonPropertyName("texto")]
        public string Texto { get; set; } = string.Empty;
    }
}
