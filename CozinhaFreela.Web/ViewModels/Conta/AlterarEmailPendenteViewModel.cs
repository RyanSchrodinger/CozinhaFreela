using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Web.ViewModels.Conta
{
    public class AlterarEmailPendenteViewModel
    {
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o novo e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string NovoEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme o novo e-mail.")]
        [Compare(nameof(NovoEmail), ErrorMessage = "Os e-mails não são iguais.")]
        public string ConfirmacaoNovoEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;
    }
}
