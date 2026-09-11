using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Domain.Usuarios
{
    public class CodigoConfirmacaoEmail
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string CodigoHash { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }
            = DateTime.UtcNow;

        public DateTime DataExpiracao { get; set; }

        public DateTime? DataConfirmacao { get; set; }

        public int Tentativas { get; set; }

        public bool Utilizado =>
            DataConfirmacao.HasValue;

        public ApplicationUser Usuario { get; set; } = null!;
    }
}