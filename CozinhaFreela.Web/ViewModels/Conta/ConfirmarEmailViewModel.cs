using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Web.ViewModels.Conta
{
    public class ConfirmarEmailViewModel
    {
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o código.")]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "O código deve possuir 6 números."
        )]
        [Display(Name = "Código de confirmação")]
        public string Codigo { get; set; } = string.Empty;
    }
}