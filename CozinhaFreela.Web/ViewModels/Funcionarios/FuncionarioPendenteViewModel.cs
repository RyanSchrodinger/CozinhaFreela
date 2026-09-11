namespace CozinhaFreela.Web.ViewModels.Funcionarios
{
    public class FuncionarioPendenteViewModel
    {
        public string UsuarioId { get; set; }
            = string.Empty;

        public string NomeCompleto { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string CpfMascarado { get; set; }
            = string.Empty;

        public string Telefone { get; set; }
            = string.Empty;

        public string Cidade { get; set; }
            = string.Empty;

        public string Estado { get; set; }
            = string.Empty;

        public DateTime DataCadastro { get; set; }

        public bool EmailConfirmado { get; set; }
    }
}