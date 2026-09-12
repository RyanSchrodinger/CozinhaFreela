using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Web.ViewModels.Funcionarios
{
    public class AnaliseFuncionarioViewModel
    {
        public string UsuarioId { get; set; }
            = string.Empty;

        public string NomeCompleto { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public bool EmailConfirmado { get; set; }

        public string Cpf { get; set; }
            = string.Empty;

        public DateOnly DataNascimento { get; set; }

        public int Idade { get; set; }

        public string Telefone { get; set; }
            = string.Empty;

        public string Cep { get; set; }
            = string.Empty;

        public string Rua { get; set; }
            = string.Empty;

        public string Numero { get; set; }
            = string.Empty;

        public string? Complemento { get; set; }

        public string Bairro { get; set; }
            = string.Empty;

        public string Cidade { get; set; }
            = string.Empty;

        public string Estado { get; set; }
            = string.Empty;

        public string Nacionalidade { get; set; }
            = string.Empty;

        public string EstadoCivil { get; set; }
            = string.Empty;

        public string ContatoEmergenciaNome { get; set; }
            = string.Empty;

        public string ContatoEmergenciaTelefone { get; set; }
            = string.Empty;

        public string? Observacoes { get; set; }

        public DateTime DataCadastro { get; set; }

        [Required(
            ErrorMessage =
                "Informe a função do funcionário."
        )]
        [MaxLength(
            80,
            ErrorMessage =
                "A função deve possuir no máximo 80 caracteres."
        )]
        [Display(Name = "Função")]
        public string Funcao { get; set; }
            = string.Empty;
    }
}