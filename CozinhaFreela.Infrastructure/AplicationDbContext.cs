using CozinhaFreela.Domain.Usuarios;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CozinhaFreela.Infrastructure.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Funcionario> Funcionarios =>
            Set<Funcionario>();

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(
                entidade =>
                {
                    entidade
                        .Property(usuario => usuario.NomeCompleto)
                        .HasMaxLength(120)
                        .IsRequired();

                    entidade
                        .Property(usuario => usuario.StatusCadastro)
                        .HasConversion<string>()
                        .HasMaxLength(20);

                    entidade
                        .Property(usuario => usuario.Ativo)
                        .HasDefaultValue(false);
                });

            builder.Entity<Funcionario>(
                entidade =>
                {
                    entidade.HasKey(
                        funcionario => funcionario.UsuarioId
                    );

                    entidade
                        .HasOne(funcionario => funcionario.Usuario)
                        .WithOne(usuario => usuario.Funcionario)
                        .HasForeignKey<Funcionario>(
                            funcionario => funcionario.UsuarioId
                        )
                        .OnDelete(DeleteBehavior.Cascade);

                    entidade
                        .HasIndex(funcionario => funcionario.Cpf)
                        .IsUnique();

                    entidade
                        .Property(funcionario => funcionario.Cpf)
                        .HasMaxLength(11)
                        .IsRequired();

                    entidade
                        .Property(funcionario => funcionario.Cep)
                        .HasMaxLength(8)
                        .IsRequired();

                    entidade
                        .Property(funcionario => funcionario.Estado)
                        .HasMaxLength(2)
                        .IsRequired();
                });
        }
    }
}