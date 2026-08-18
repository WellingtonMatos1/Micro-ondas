using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using micro_ondas.Models;

namespace micro_ondas.Services
{
    public class MicroondasService
    {
        private const string CaracterePadraoManual = ".";

        // Lista fixa: os programas pre-definidos nao possuem metodos de edicao ou exclusao.
        private static readonly IReadOnlyList<ProgramaAquecimento> ProgramasPreDefinidos =
            new List<ProgramaAquecimento>
            {
                new("pipoca", "Pipoca", "Pipoca de micro-ondas", 180, 7, "*",
                    "Observar o barulho de estouros do milho. Caso houver um intervalo de mais de 10 segundos entre um estouro e outro, interrompa o aquecimento."),
                new("leite", "Leite", "Leite", 300, 5, "~",
                    "Cuidado com aquecimento de liquidos. O choque termico aliado ao movimento do recipiente pode causar fervura imediata e risco de queimaduras."),
                new("carne-boi", "Carnes de boi", "Carne em pedaco ou fatias", 840, 4, "#",
                    "Interrompa o processo na metade e vire o conteudo com a parte de baixo para cima para o descongelamento uniforme."),
                new("frango", "Frango", "Frango, qualquer corte", 480, 7, "@",
                    "Interrompa o processo na metade e vire o conteudo com a parte de baixo para cima para o descongelamento uniforme."),
                new("feijao", "Feijao", "Feijao congelado", 480, 9, "$",
                    "Deixe o recipiente destampado e, em casos de plastico, cuidado ao retirar o recipiente pois ele pode perder resistencia em altas temperaturas.")
            };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly object _sync = new();
        private readonly string _arquivoProgramasCustomizados;
        private readonly List<ProgramaAquecimento> _programasCustomizados;
        private readonly Microondas _microondas = new();

        public MicroondasService(IWebHostEnvironment environment)
        {
            _arquivoProgramasCustomizados = Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "programas-customizados.json");

            _programasCustomizados = CarregarProgramasCustomizados();
        }

        public Microondas ObterEstado()
        {
            return _microondas;
        }

        public IReadOnlyList<ProgramaAquecimento> ObterProgramas()
        {
            lock (_sync)
            {
                return ProgramasPreDefinidos
                    .Concat(_programasCustomizados)
                    .ToList();
            }
        }

        public ProgramaAquecimento CadastrarPrograma(
            string? nome,
            string? alimento,
            int? tempoSegundos,
            int? potencia,
            string? caractereAquecimento,
            string? instrucoes)
        {
            var nomeTratado = NormalizarTexto(nome);
            var alimentoTratado = NormalizarTexto(alimento);
            var caractereTratado = NormalizarTexto(caractereAquecimento);
            var instrucoesTratadas = NormalizarTexto(instrucoes);

            ValidarCadastro(nomeTratado, alimentoTratado, tempoSegundos, potencia, caractereTratado);

            var programa = new ProgramaAquecimento(
                CriarId(nomeTratado),
                nomeTratado,
                alimentoTratado,
                tempoSegundos!.Value,
                potencia!.Value,
                caractereTratado,
                instrucoesTratadas,
                customizado: true);

            lock (_sync)
            {
                _programasCustomizados.Add(programa);
                SalvarProgramasCustomizados();
            }

            return programa;
        }

        public void SelecionarPrograma(string programaId)
        {
            // O controller recebe apenas o id, a regra de negocio localiza o programa valido.
            var programa = ObterProgramas()
                .FirstOrDefault(item => item.Id == programaId);

            if (programa == null)
                throw new ArgumentException("Programa de aquecimento nao encontrado.");

            _microondas.SelecionarPrograma(programa);
        }

        public void RemoverProgramaCustomizado(string programaId)
        {
            lock (_sync)
            {
                var programa = _programasCustomizados
                    .FirstOrDefault(item => item.Id == programaId);

                if (programa == null)
                    throw new ArgumentException("Somente programas customizados podem ser removidos.");

                if (_microondas.ProgramaSelecionado?.Id == programa.Id)
                    _microondas.Limpar();

                _programasCustomizados.Remove(programa);
                SalvarProgramasCustomizados();
            }
        }

        public void Iniciar(int? tempo, int? potencia)
        {
            _microondas.Iniciar(tempo, potencia);
        }

        public void ProcessarSegundo()
        {
            _microondas.ProcessarSegundo();
        }

        public void PausarOuCancelar()
        {
            _microondas.PausarOuCancelar();
        }

        public void Limpar()
        {
            _microondas.Limpar();
        }

        private void ValidarCadastro(
            string nome,
            string alimento,
            int? tempoSegundos,
            int? potencia,
            string caractere)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Informe o nome do programa.");

            if (string.IsNullOrWhiteSpace(alimento))
                throw new ArgumentException("Informe o alimento do programa.");

            if (tempoSegundos == null || tempoSegundos <= 0)
                throw new ArgumentException("Informe um tempo maior que zero.");

            if (potencia == null || potencia < 1 || potencia > 10)
                throw new ArgumentException("Informe uma potencia valida entre 1 e 10.");

            if (caractere.Length != 1)
                throw new ArgumentException("Informe apenas um caractere de aquecimento.");

            if (caractere == CaracterePadraoManual)
                throw new ArgumentException("O caractere de aquecimento nao pode ser o ponto padrao.");

            var caractereJaUsado = ObterProgramas()
                .Any(programa => programa.StringAquecimento == caractere);

            if (caractereJaUsado)
                throw new ArgumentException("Este caractere de aquecimento ja esta sendo usado por outro programa.");
        }

        private List<ProgramaAquecimento> CarregarProgramasCustomizados()
        {
            if (!File.Exists(_arquivoProgramasCustomizados))
                return new List<ProgramaAquecimento>();

            var json = File.ReadAllText(_arquivoProgramasCustomizados);
            var programas = JsonSerializer.Deserialize<List<ProgramaAquecimento>>(json, JsonOptions)
                ?? new List<ProgramaAquecimento>();

            return programas
                .Where(ProgramaCustomizadoValido)
                .Select(programa => new ProgramaAquecimento(
                    programa.Id,
                    programa.Nome,
                    programa.Alimento,
                    programa.TempoSegundos,
                    programa.Potencia,
                    programa.StringAquecimento,
                    programa.Instrucoes,
                    customizado: true))
                .ToList();
        }

        private void SalvarProgramasCustomizados()
        {
            var pasta = Path.GetDirectoryName(_arquivoProgramasCustomizados);

            if (!string.IsNullOrWhiteSpace(pasta))
                Directory.CreateDirectory(pasta);

            var json = JsonSerializer.Serialize(_programasCustomizados, JsonOptions);
            File.WriteAllText(_arquivoProgramasCustomizados, json);
        }

        private bool ProgramaCustomizadoValido(ProgramaAquecimento programa)
        {
            if (string.IsNullOrWhiteSpace(programa.Id))
                return false;

            if (string.IsNullOrWhiteSpace(programa.Nome))
                return false;

            if (string.IsNullOrWhiteSpace(programa.Alimento))
                return false;

            if (programa.TempoSegundos <= 0)
                return false;

            if (programa.Potencia < 1 || programa.Potencia > 10)
                return false;

            if (string.IsNullOrWhiteSpace(programa.StringAquecimento))
                return false;

            if (programa.StringAquecimento.Length != 1)
                return false;

            return programa.StringAquecimento != CaracterePadraoManual;
        }

        private static string NormalizarTexto(string? texto)
        {
            return texto?.Trim() ?? string.Empty;
        }

        private static string CriarId(string nome)
        {
            var baseId = new string(nome
                .ToLowerInvariant()
                .Select(caractere => char.IsLetterOrDigit(caractere) ? caractere : '-')
                .ToArray())
                .Trim('-');

            if (string.IsNullOrWhiteSpace(baseId))
                baseId = "programa";

            var sufixo = Guid.NewGuid().ToString("N")[..8];
            return $"{baseId}-{sufixo}";
        }
    }
}
