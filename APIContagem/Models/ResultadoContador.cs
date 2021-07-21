namespace APIContagem.Models
{
    public class ResultadoContador
    {
        public int ValorAtual { get; set; } 
        public string Producer { get; set; } 
        public string Kernel { get; set; } 
        public string TargetFramework { get; set; } 
        public string Mensagem { get; set; }
    }
}