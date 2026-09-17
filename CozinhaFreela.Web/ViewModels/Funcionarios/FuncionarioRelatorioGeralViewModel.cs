namespace CozinhaFreela.Web.ViewModels.Funcionarios
{
    public class FuncionarioRelatorioGeralViewModel
    {
        public string NomeCompleto { get; set; }
            = string.Empty;

        public string Funcao { get; set; }
            = string.Empty;

        public string Cpf { get; set; }
            = string.Empty;

        public DateOnly DataNascimento { get; set; }

        public string Telefone { get; set; }
            = string.Empty;

        public string Cidade { get; set; }
            = string.Empty;

        public string Estado { get; set; }
            = string.Empty;

        public string ContatoEmergenciaNome { get; set; }
            = string.Empty;

        public string ContatoEmergenciaTelefone { get; set; }
            = string.Empty;

        public bool Ativo { get; set; }
    }
}