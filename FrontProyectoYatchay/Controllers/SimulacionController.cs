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

        // Acción para la frase introductoria
        public IActionResult Introduccion(int idSession, int faseActual)
        {
            ViewBag.IdSession = idSession;
            ViewBag.FaseActual = faseActual;
            return View();
        }

        // Acción para iniciar o retomar la simulación
        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("YatchayApi");

            int idUsuario = int.Parse(User.FindFirst("IdUsuario").Value);
            var response = await client.PostAsJsonAsync("api/Simulation/start", new { IdUsuario = idUsuario });

            var sesion = await response.Content.ReadFromJsonAsync<dynamic>();
            int idSession = sesion.datos.idSession;
            int faseActual = sesion.datos.faseActual;

            return RedirectToAction("Jugar", new { idSession, fase = faseActual });
        }

        public async Task<IActionResult> Jugar(int idSession, int fase)
        {
            var client = _clientFactory.CreateClient("YatchayApi");
            var response = await client.GetAsync($"api/Simulation/content/{idSession}/{fase}");

            if (!response.IsSuccessStatusCode) return RedirectToAction("Index", "Home");

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            var model = new SimulacionViewModels
            {
                IdContent = result.datos.idContent,
                Fase = result.datos.fase,
                Titulo = result.datos.titulo,
                Opciones = result.datos.opciones
            };

            ViewBag.IdSession = idSession;
            return View(model);
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

                return View("Feedback", feedback);
            }

            return BadRequest("Error al procesar la decisión.");
        }
    }
}
