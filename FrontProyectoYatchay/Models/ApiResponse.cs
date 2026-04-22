namespace FrontProyectoYatchay.Models
{
    public class ApiResponse
    {
        public int Exito { get; set; }
        public string Mensaje { get; set; }
        public List<string> Errores { get; set; }
    }
}
