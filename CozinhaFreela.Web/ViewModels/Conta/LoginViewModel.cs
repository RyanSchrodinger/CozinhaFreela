using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Web.ViewModels.Conta
{
    public class LoginViewModel
    {
        [Required(
            ErrorMessage = "Informe seu e-mail."
        )]
        [EmailAddress(
            ErrorMessage = "Informe um e-mail válido."
        )]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage = "Informe sua senha."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; }
            = string.Empty;

        [Display(Name = "Manter conectado")]
        public bool LembrarMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}