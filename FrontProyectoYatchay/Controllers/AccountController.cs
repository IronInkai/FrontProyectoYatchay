using FrontProyectoYatchay.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    public AccountController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    // Muestra la página de Login
    [HttpGet]
    public IActionResult Login() => View();

    // Recibe los datos del formulario
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var client = _clientFactory.CreateClient("YatchayApi");

        // Enviamos el objeto al endpoint de tu API
        var response = await client.PostAsJsonAsync("api/Usuarios/login", model);

        if (response.IsSuccessStatusCode)
        {
            // Leemos la respuesta exitosa usando tu LoginResponseDto
            var infoUsuario = await response.Content.ReadFromJsonAsync<UsuarioSesion>();

            // Redirigir según el rol
            if (infoUsuario.NombreRol.ToLower() == "analista") 
            {
                return RedirectToAction("Dashboard", "admin");
            }

            // Aquí puedes leer el rol que devuelve tu SP para decidir a dónde mandarlo
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Credenciales incorrectas o error en el servidor.";
        return View(model);
    }

    //GET: Account/Registro
    [HttpGet]
    public IActionResult Registro() => View();

    //POST: Account/Registro
    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    { 
        if(!ModelState.IsValid) return View(model);

        var client = _clientFactory.CreateClient("YatchayApi");

        //Enviamos los datos a la ruta /api/usuarios/registro
        var response = await client.PostAsJsonAsync("api/Usuarios/registro", model);

        if (response.IsSuccessStatusCode)
        {
            TempData["MensajeExito"] = "¡Registro exitoso!";
            return RedirectToAction("Login");
        }

        //Si la API devuelve un error (ej: correo duplicado), lo capturamos
        var errorContent = await response.Content.ReadFromJsonAsync<dynamic>();
        ViewBag.Error = errorContent?.mensaje ?? "Error al registrar usuario.";

        return View(model);
    }
}