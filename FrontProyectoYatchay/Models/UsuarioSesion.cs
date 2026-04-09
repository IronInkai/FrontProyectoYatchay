namespace FrontProyectoYatchay.Models
{
    public class UsuarioSesion
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}
