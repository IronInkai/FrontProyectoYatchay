namespace FrontProyectoYatchay.Models
{
    public class ApiResponse<T>
    {
        public int Exito { get; set; }
        public string Mensaje { get; set; }
        public T Datos { get; set; }
        public List<string> Errores { get; set; }
    }
}
