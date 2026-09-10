using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Web.ViewModels.Conta
{
    public class CadastroViewModel
    {
        [Required(ErrorMessage = "Informe o nome completo.")]
        [StringLength(120)]
        [Display(Name = "Nome completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o CPF.")]
        [Display(Name = "CPF")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a data de nascimento.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de nascimento")]
        public DateOnly DataNascimento { get; set; }

        [Required(ErrorMessage = "Informe o telefone.")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o CEP.")]
        [Display(Name = "CEP")]
        public string Cep { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a rua.")]
        [StringLength(150)]
        public string Rua { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o número.")]
        [StringLength(10)]
        [Display(Name = "Número")]
        public string Numero { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Complemento { get; set; }

        [Required(ErrorMessage = "Informe o bairro.")]
        [StringLength(100)]
        public string Bairro { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a cidade.")]
        [StringLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o estado.")]
        [StringLength(2)]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a nacionalidade.")]
        [StringLength(60)]
        public string Nacionalidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o estado civil.")]
        [StringLength(30)]
        [Display(Name = "Estado civil")]
        public string EstadoCivil { get; set; } = string.Empty;

     

        [Required(ErrorMessage = "Informe o contato de emergência.")]
        [StringLength(120)]
        [Display(Name = "Nome do contato de emergência")]
        public string ContatoEmergenciaNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o telefone de emergência.")]
        [Display(Name = "Telefone do contato de emergência")]
        public string ContatoEmergenciaTelefone { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Required(ErrorMessage = "Informe uma senha.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "A senha deve possuir pelo menos 8 caracteres."
        )]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Senha),
            ErrorMessage = "As senhas não coincidem."
        )]
        [Display(Name = "Confirmar senha")]
        public string ConfirmacaoSenha { get; set; } = string.Empty;
    }
}