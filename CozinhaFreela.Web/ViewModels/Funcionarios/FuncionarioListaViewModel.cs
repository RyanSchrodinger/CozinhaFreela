namespace CozinhaFreela.Web.ViewModels.Funcionarios
{
    public class FuncionarioListaViewModel
    {
        public string UsuarioId { get; set; }
            = string.Empty;

        public string NomeCompleto { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Funcao { get; set; }
            = string.Empty;

        public string Telefone { get; set; }
            = string.Empty;

        public string Cidade { get; set; }
            = string.Empty;

        public string Estado { get; set; }
            = string.Empty;

        public bool Ativo { get; set; }

        public DateTime DataCadastro { get; set; }
    }
}