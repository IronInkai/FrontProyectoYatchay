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

        // En AccountController.cs -> Método Login
        if (response.IsSuccessStatusCode)
        {
            // 1. Leemos el objeto completo (la caja que contiene 'datos')
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioSesion>>();

            if (apiResponse != null && apiResponse.Datos != null)
            {
                // 2. Sacamos la información de adentro de 'Datos'
                var infoUsuario = apiResponse.Datos;

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, infoUsuario.Nombre),
            new Claim(ClaimTypes.Role, infoUsuario.NombreRol),
            // Ahora infoUsuario.IdUsuario valdrá 3, no 0
            new Claim("IdUsuario", infoUsuario.IdUsuario.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Simulacion");
            }
        }

        ViewBag.Error = "Correo o contraseña incorrectos.";
        return View(model);
    }

    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
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

        var errorData = await response.Content.ReadFromJsonAsync<ApiResponse<RegistroViewModel>>();
        ViewBag.Error = errorData?.Mensaje ?? "Error al registrar usuario.";

        // En System.Text.Json, el dynamic se convierte en JsonElement
        if (errorData?.Errores != null && errorData.Errores.Any())
        {
            ViewBag.ErrorDetail = errorData.Errores;
        }

        return View(model);
    }
}