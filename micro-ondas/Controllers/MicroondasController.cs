using Microsoft.AspNetCore.Mvc;
using micro_ondas.Services;

namespace micro_ondas.Controllers
{
    public class MicroondasController : Controller
    {
        private readonly MicroondasService _microondasService;

        public MicroondasController(MicroondasService microondasService)
        {
            _microondasService = microondasService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Iniciar([FromForm] int? tempo, [FromForm] int? potencia)
        {
            try
            {
                _microondasService.Iniciar(tempo, potencia);
                return Json(CriarResposta());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ProcessarSegundo()
        {
            _microondasService.ProcessarSegundo();
            return Json(CriarResposta());
        }

        [HttpPost]
        public IActionResult PausarOuCancelar()
        {
            _microondasService.PausarOuCancelar();
            return Json(CriarResposta());
        }

        [HttpPost]
        public IActionResult Limpar()
        {
            _microondasService.Limpar();
            return Json(CriarResposta());
        }

        private object CriarResposta()
        {
            var microondas = _microondasService.ObterEstado();

            return new
            {
                tempoRestante = microondas.TempoRestante,
                tempoFormatado = microondas.TempoFormatado(),
                potencia = microondas.Potencia,
                estado = microondas.Estado.ToString(),
                stringAquecimento = microondas.StringAquecimento
            };
        }
    }

    public class IniciarMicroondasRequest
    {
        public int? Tempo { get; set; }
        public int? Potencia { get; set; }
    }
}