using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Web.ViewModels.Conta
{
    public class ReenviarCodigoViewModel
    {
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;
    }
}
