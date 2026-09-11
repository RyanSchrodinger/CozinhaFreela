using System.ComponentModel.DataAnnotations;

namespace CozinhaFreela.Domain.Usuarios
{
    public class Funcionario
    {
        [Key]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        public string Cpf { get; set; } = string.Empty;

        public DateOnly DataNascimento { get; set; }

        [Required]
        [MaxLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        [MaxLength(8)]
        public string Cep { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Rua { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Complemento { get; set; }

        [Required]
        [MaxLength(100)]
        public string Bairro { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Cidade { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        public string Estado { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Nacionalidade { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string EstadoCivil { get; set; } = string.Empty;

        [MaxLength(80)]
        public string? Funcao { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string ContatoEmergenciaNome { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ContatoEmergenciaTelefone { get; set; }
            = string.Empty;

        [MaxLength(500)]
        public string? Observacoes { get; set; }

        public ApplicationUser Usuario { get; set; } = null!;
    }
}