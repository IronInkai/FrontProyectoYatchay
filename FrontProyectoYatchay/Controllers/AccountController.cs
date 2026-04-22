using FrontProyectoYatchay.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _clientFactory;
    private object errorData;

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
        var response = await client.PostAsJsonAsync("api/Auth/login", model);

        if (response.IsSuccessStatusCode)
        {
            // Leemos la respuesta exitosa usando tu LoginResponseDto
            var infoUsuario = await response.Content.ReadFromJsonAsync<UsuarioSesion>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, infoUsuario.Nombre),
                new Claim(ClaimTypes.Email, model.Correo),
                new Claim(ClaimTypes.Role, infoUsuario.NombreRol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Redirigir según el rol
            if (infoUsuario.NombreRol.ToLower() == "analista") 
            {
                return RedirectToAction("Dashboard", "admin");
            }

            // Aquí puedes leer el rol que devuelve tu SP para decidir a dónde mandarlo
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Correo o contraseña incorrectos.";
        return View(model);
    }

    //GET: Account/Registro
    [HttpGet]
    public IActionResult Registro() => View();

    //POST: Account/Registro
    [HttpPost]
    public async Task<IActionResult> Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _clientFactory.CreateClient("YatchayApi");

        var response = await client.PostAsJsonAsync("api/Auth/register", model);

        if (response.IsSuccessStatusCode)
        {
            TempData["MensajeExito"] = "¡Registro exitoso!";
            return RedirectToAction("Login");
        }

        var errorData = await response.Content.ReadFromJsonAsync<ApiResponse>();
        ViewBag.Error = errorData?.Mensaje ?? "Error al registrar usuario.";

        // En System.Text.Json, el dynamic se convierte en JsonElement
        if (errorData?.Errores != null && errorData.Errores.Any())
        {
            ViewBag.ErrorDetail = errorData.Errores;
        }

        return View(model);
    }
}