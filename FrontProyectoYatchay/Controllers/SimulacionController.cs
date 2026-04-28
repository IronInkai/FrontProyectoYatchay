using FrontProyectoYatchay.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FrontProyectoYatchay.Controllers
{
    [Authorize]
    public class SimulacionController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public SimulacionController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("YatchayApi");
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) || idUsuarioClaim == "0")
            {
                return Content("Error: El ID de usuario no existe en la sesión actual. Por favor, cierra sesión y vuelve a entrar.");
            }

            var response = await client.PostAsJsonAsync("api/Simulation/start", new { IdUsuario = int.Parse(idUsuarioClaim) });

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                var datos = json.GetProperty("datos");

                return RedirectToAction("Introduccion", new
                {
                    idSession = ObtenerInt(datos, "idSession"),
                    faseActual = ObtenerInt(datos, "faseActual")
                });
            }

            return Content("Error al iniciar simulación");    
        }

        public IActionResult Introduccion(int idSession, int faseActual)
        {
            ViewBag.IdSession = idSession;
            ViewBag.FaseActual = faseActual;
            return View();
        }

        
        public async Task<IActionResult> Jugar(int idSession, int fase)
        {
            int idABuscar = (fase <= 0) ? 1 : fase;

            var client = _clientFactory.CreateClient("YatchayApi");
            var response = await client.GetAsync($"api/Simulation/content/{idSession}/{idABuscar}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonDocument>();
                var datos = json.RootElement.GetProperty("datos");

                string opcionesRaw = datos.GetProperty("opciones").GetRawText();
                var listaOpciones = System.Text.Json.JsonSerializer.Deserialize<List<OpcionViewModel>>(opcionesRaw,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                var modelo = new SimulacionViewModel
                {
                    IdSession = idSession,
                    IdContent = ObtenerInt(datos, "idContent"),
                    Fase = ObtenerInt(datos, "fase"),
                    Titulo = datos.GetProperty("titulo").GetString() ?? "",
                    Contenido = "Selecciona la acción más adecuada para esta etapa de la simulación.",
                    ListadoOpciones = listaOpciones
                };

                return View(modelo);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EnviarDecision(int IdSession, int IdContent, int OpcionElegida, int FaseActual)
        {
            var client = _clientFactory.CreateClient("YatchayApi");

            var decision = new
            {
                idSession = IdSession,
                idContent = IdContent,
                idOption = OpcionElegida
            };

            var response = await client.PostAsJsonAsync("api/Simulation/decide", decision);

            if (response.IsSuccessStatusCode)
            {

                var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonDocument>();
                var datos = json.RootElement.GetProperty("datos");

                bool puedeSiguiente = datos.TryGetProperty("puedeSiguiente", out var prop) && prop.GetBoolean();

                if (puedeSiguiente)
                {
                    int siguienteFase = FaseActual + 1;
                    return RedirectToAction("Jugar", new { idSession = IdSession, fase = siguienteFase });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            //var errorMsg = await response.Content.ReadAsStringAsync();
           //return Content($"Error de la API: {errorMsg}");

            return RedirectToAction("Jugar", new { idSession = IdSession, fase = FaseActual });
        }

        private int ObtenerInt(System.Text.Json.JsonElement elemento, string propiedad)
        {
            if (!elemento.TryGetProperty(propiedad, out var prop)) return 0;

            if (prop.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                return prop.GetInt32();
            }
            string valorTexto = prop.GetString() ?? "0";
            return int.TryParse(valorTexto, out int resultado) ? resultado : 0;
        }

    }
}
