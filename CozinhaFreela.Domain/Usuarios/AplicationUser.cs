using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Domain.Usuarios
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(120)]
        public string NomeCompleto { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; }
            = DateTime.UtcNow;

        public DateTime? DataExpiracaoConfirmacao { get; set; }
            = DateTime.UtcNow.AddHours(24);

        public StatusCadastro StatusCadastro { get; set; }
            = StatusCadastro.Pendente;

        public bool Ativo { get; set; } = false;

        public Funcionario? Funcionario { get; set; }
    }
}
