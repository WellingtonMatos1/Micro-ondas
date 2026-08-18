using System;
using System.Text;

namespace micro_ondas.Models
{
    public class Microondas
    {
        private const int TempoMinimoManual = 1;
        private const int TempoMaximoManual = 120;
        private const int PotenciaPadrao = 10;

        // Setters privados evitam que a interface altere o estado interno diretamente.
        public int TempoRestante { get; private set; }
        public int TempoTotal { get; private set; }
        public int Potencia { get; private set; } = PotenciaPadrao;
        public EstadoMicroondas Estado { get; private set; } = EstadoMicroondas.Parado;
        public string StringAquecimento { get; private set; } = string.Empty;
        public ProgramaAquecimento? ProgramaSelecionado { get; private set; }
        public bool UsandoProgramaPreDefinido => ProgramaSelecionado != null;

        public void Iniciar(int? tempo, int? potencia)
        {
            // No modo manual, pressionar iniciar durante o aquecimento acrescenta 30 segundos.
            if (Estado == EstadoMicroondas.Aquecendo)
            {
                if (UsandoProgramaPreDefinido)
                    throw new InvalidOperationException("Nao e permitido acrescentar tempo em programas selecionados.");

                AcrescentarTempoManual();
                return;
            }

            // Se estiver pausado, o mesmo botão retoma o aquecimento do ponto em que parou.
            if (Estado == EstadoMicroondas.Pausado)
            {
                Estado = EstadoMicroondas.Aquecendo;
                return;
            }

            // Programa selecionado ignora entradas manuais e usa tempo/potência cadastrados.
            if (ProgramaSelecionado != null)
            {
                IniciarProgramaSelecionado();
                return;
            }

            int tempoDefinido = tempo ?? 30;
            int potenciaDefinida = potencia ?? PotenciaPadrao;

            ValidarTempoManual(tempoDefinido);
            ValidarPotencia(potenciaDefinida);

            TempoTotal = tempoDefinido;
            TempoRestante = tempoDefinido;
            Potencia = potenciaDefinida;
            StringAquecimento = string.Empty;
            Estado = EstadoMicroondas.Aquecendo;
        }

        public void SelecionarPrograma(ProgramaAquecimento programa)
        {
            // Troca de programa não deve acontecer no meio de um aquecimento ou pausa.
            if (Estado == EstadoMicroondas.Aquecendo || Estado == EstadoMicroondas.Pausado)
                throw new InvalidOperationException("Pause ou cancele o aquecimento antes de selecionar outro programa.");

            // Ao selecionar, a tela recebe tempo/potência automáticamente e bloqueia edição.
            ProgramaSelecionado = programa;
            TempoTotal = programa.TempoSegundos;
            TempoRestante = programa.TempoSegundos;
            Potencia = programa.Potencia;
            StringAquecimento = string.Empty;
            Estado = EstadoMicroondas.Parado;
        }

        public void ProcessarSegundo()
        {
            if (Estado != EstadoMicroondas.Aquecendo)
                return;

            if (TempoRestante <= 0)
                return;

            // Cada segundo adiciona a string conforme a potência configurada.
            TempoRestante--;
            StringAquecimento += GerarStringAquecimentoDoSegundo() + " ";

            if (TempoRestante == 0)
            {
                Estado = EstadoMicroondas.Concluido;
                StringAquecimento += "Aquecimento concluido";
            }
        }

        public void PausarOuCancelar()
        {
            if (Estado == EstadoMicroondas.Aquecendo)
            {
                Estado = EstadoMicroondas.Pausado;
                return;
            }

            if (Estado == EstadoMicroondas.Pausado)
            {
                Cancelar();
                return;
            }

            Limpar();
        }

        public void Cancelar()
        {
            TempoRestante = 0;
            TempoTotal = 0;
            Potencia = PotenciaPadrao;
            Estado = EstadoMicroondas.Parado;
            StringAquecimento = string.Empty;
            ProgramaSelecionado = null;
        }

        public void Limpar()
        {
            Cancelar();
        }

        public string TempoFormatado()
        {
            int minutos = TempoRestante / 60;
            int segundos = TempoRestante % 60;

            return $"{minutos}:{segundos:D2}";
        }

        private void AcrescentarTempoManual()
        {
            TempoRestante += 30;

            if (TempoRestante > TempoMaximoManual)
                TempoRestante = TempoMaximoManual;
        }

        private void IniciarProgramaSelecionado()
        {
            if (ProgramaSelecionado == null)
                return;

            TempoTotal = ProgramaSelecionado.TempoSegundos;
            TempoRestante = ProgramaSelecionado.TempoSegundos;
            Potencia = ProgramaSelecionado.Potencia;
            StringAquecimento = string.Empty;
            Estado = EstadoMicroondas.Aquecendo;
        }

        private string GerarStringAquecimentoDoSegundo()
        {
            // Manual usa ".", enquanto programas usam seus caracteres exclusivos.
            var texto = ProgramaSelecionado?.StringAquecimento ?? ".";
            var builder = new StringBuilder();

            for (int i = 0; i < Potencia; i++)
            {
                builder.Append(texto);
            }

            return builder.ToString();
        }

        private static void ValidarTempoManual(int tempo)
        {
            if (tempo < TempoMinimoManual || tempo > TempoMaximoManual)
                throw new ArgumentException("Informe um tempo valido entre 1 segundo e 2 minutos.");
        }

        private static void ValidarPotencia(int potencia)
        {
            if (potencia < 1 || potencia > 10)
                throw new ArgumentException("Informe uma potencia valida entre 1 e 10.");
        }
    }
}
