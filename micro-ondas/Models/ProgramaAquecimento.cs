namespace micro_ondas.Models
{
    // Representa um programa de aquecimento, fixo ou cadastrado pelo usuário.
    public sealed class ProgramaAquecimento
    {
        public ProgramaAquecimento(
            string id,
            string nome,
            string alimento,
            int tempoSegundos,
            int potencia,
            string stringAquecimento,
            string instrucoes,
            bool customizado = false)
        {
            Id = id;
            Nome = nome;
            Alimento = alimento;
            TempoSegundos = tempoSegundos;
            Potencia = potencia;
            StringAquecimento = stringAquecimento;
            Instrucoes = instrucoes;
            Customizado = customizado;
        }

        public string Id { get; }
        public string Nome { get; }
        public string Alimento { get; }
        public int TempoSegundos { get; }
        public int Potencia { get; }
        public string StringAquecimento { get; }
        public string Instrucoes { get; }
        public bool Customizado { get; }
    }
}
