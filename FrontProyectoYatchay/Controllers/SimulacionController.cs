using FrontProyectoYatchay.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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



        // Acción para iniciar o retomar la simulación
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("YatchayApi");

            // 1. Extraer el ID del Claim
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;

            if (string.IsNullOrEmpty(idUsuarioClaim) || idUsuarioClaim == "0")
            {
                // Si llegamos aquí, el Login no guardó el ID correctamente
                return Content("Error: El ID de usuario no existe en la sesión actual. Por favor, cierra sesión y vuelve a entrar.");
            }

            // 2. Enviar a la API
            var response = await client.PostAsJsonAsync("api/Simulation/start", new { IdUsuario = int.Parse(idUsuarioClaim) });

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                var datos = json.GetProperty("datos");

                return RedirectToAction("Introduccion", new
                {
                    idSession = datos.GetProperty("idSession").GetInt32(),
                    faseActual = datos.GetProperty("faseActual").GetInt32()
                });
            }
            else
            {
                // Para ver si la API da error 400, veremos el porqué aquí
                var errorBody = await response.Content.ReadAsStringAsync();
                return Content($"La API devolvió error {response.StatusCode}: {errorBody}");
            }
        }

        // Acción para la frase introductoria
        public IActionResult Introduccion(int idSession, int faseActual)
        {
            ViewBag.IdSession = idSession;
            ViewBag.FaseActual = faseActual;
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Jugar(int idSession, int fase)
        {
            var client = _clientFactory.CreateClient("YatchayApi");

            var response = await client.GetAsync($"api/Simulation/content/{idSession}/{fase}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonDocument>();
                var datos = json.RootElement.GetProperty("datos");

                // 1. Extraemos el string crudo de las opciones
                string opcionesRaw = datos.GetProperty("opciones").GetString() ?? "[]";

                var listaOpciones = System.Text.Json.JsonSerializer.Deserialize<List<OpcionViewModel>>(opcionesRaw,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();                 

                var modelo = new SimulacionViewModel
                {
                    IdSession = idSession,
                    IdContent = datos.GetProperty("idContent").GetInt32(),
                    Fase = datos.GetProperty("fase").GetInt32(),
                    Titulo = datos.GetProperty("titulo").GetString() ?? "",
                    Contenido = datos.GetProperty("tipo").GetString() ?? "", // O usa la propiedad que prefieras
                    ListadoOpciones = listaOpciones 
                };

                return View(modelo);
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EnviarDecision(DecisionRequest request)
        {
            var client = _clientFactory.CreateClient("YatchayApi");
            var response = await client.PostAsJsonAsync("api/Simulation/decide", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();

                var feedback = new FeedbackViewModel
                {
                    Titulo = result.datos.titulo,
                    Texto = result.datos.texto,
                    Resultado = result.datos.resultado,
                    PuntajeObtenido = result.datos.puntajeObtenido,
                    PuedeSiguiente = result.datos.puedeSiguiente
                };

                return View("EnviarDecision", feedback);
            }

            return BadRequest("Error al procesar la decisión.");
        }
    }
}
