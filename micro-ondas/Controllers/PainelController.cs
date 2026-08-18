using Microsoft.AspNetCore.Mvc;
using micro_ondas.Services;

namespace micro_ondas.Controllers
{
    public class PainelController : Controller
    {
        private readonly MicroondasService _microondasService;

        public PainelController(MicroondasService microondasService)
        {
            _microondasService = microondasService;
        }

        public IActionResult Index()
        {
            // Envia os programas fixos para que a view monte os botões do painel.
            ViewBag.Programas = _microondasService.ObterProgramas();
            return View();
        }

        [HttpGet]
        public IActionResult Iniciar()
        {
            // Evita abrir JSON no navegador caso a rota de API seja acessada por GET.
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult SelecionarPrograma([FromForm] string programaId)
        {
            try
            {
                _microondasService.SelecionarPrograma(programaId);
                return Json(CriarResposta());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CadastrarPrograma(
            [FromForm] string? nome,
            [FromForm] string? alimento,
            [FromForm] int? tempoSegundos,
            [FromForm] int? potencia,
            [FromForm] string? caractereAquecimento,
            [FromForm] string? instrucoes)
        {
            try
            {
                _microondasService.CadastrarPrograma(
                    nome,
                    alimento,
                    tempoSegundos,
                    potencia,
                    caractereAquecimento,
                    instrucoes);

                return Json(new
                {
                    mensagem = "Programa cadastrado com sucesso.",
                    programas = CriarProgramasResposta()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult RemoverPrograma([FromForm] string programaId)
        {
            try
            {
                _microondasService.RemoverProgramaCustomizado(programaId);

                return Json(new
                {
                    mensagem = "Programa removido com sucesso.",
                    programas = CriarProgramasResposta(),
                    estado = CriarResposta()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
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
            var programa = microondas.ProgramaSelecionado;

            // Resposta usada pelo JavaScript para atualizar display, botões e instruções.
            return new
            {
                tempoRestante = microondas.TempoRestante,
                tempoFormatado = microondas.TempoFormatado(),
                potencia = microondas.Potencia,
                estado = microondas.Estado.ToString(),
                stringAquecimento = microondas.StringAquecimento,
                programaPreDefinido = microondas.UsandoProgramaPreDefinido,
                programaId = programa?.Id,
                programaNome = programa?.Nome,
                alimento = programa?.Alimento,
                instrucoes = programa?.Instrucoes,
                customizado = programa?.Customizado ?? false
            };
        }

        private object CriarProgramasResposta()
        {
            return _microondasService.ObterProgramas()
                .Select(programa => new
                {
                    id = programa.Id,
                    nome = programa.Nome,
                    alimento = programa.Alimento,
                    tempoSegundos = programa.TempoSegundos,
                    tempoFormatado = TimeSpan.FromSeconds(programa.TempoSegundos).ToString(@"m\:ss"),
                    potencia = programa.Potencia,
                    caractere = programa.StringAquecimento,
                    instrucoes = programa.Instrucoes,
                    customizado = programa.Customizado
                });
        }
    }
}
