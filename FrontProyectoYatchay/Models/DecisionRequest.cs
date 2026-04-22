namespace FrontProyectoYatchay.Models
{
    // Para enviar la decisión del usuario
    public class DecisionRequest
    {
        public int IdSession { get; set; }
        public int IdContent { get; set; }
        public int OpcionElegida { get; set; }
    }
}
