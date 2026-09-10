using CozinhaFreela.Domain.Usuarios;
using Microsoft.AspNetCore.Identity;

namespace CozinhaFreela.Infrastructure.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            string? nomeChefe,
            string? emailChefe,
            string? senhaChefe)
        {
            string[] roles =
            {
                RolesSistema.Chefe,
                RolesSistema.Funcionario
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var resultadoRole =
                        await roleManager.CreateAsync(
                            new IdentityRole(role)
                        );

                    if (!resultadoRole.Succeeded)
                    {
                        var erros = string.Join(
                            "; ",
                            resultadoRole.Errors.Select(
                                erro => erro.Description
                            )
                        );

                        throw new InvalidOperationException(
                            $"Não foi possível criar a role {role}: {erros}"
                        );
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(nomeChefe) ||
                string.IsNullOrWhiteSpace(emailChefe) ||
                string.IsNullOrWhiteSpace(senhaChefe))
            {
                return;
            }

            var chefe =
                await userManager.FindByEmailAsync(emailChefe);

            if (chefe is null)
            {
                chefe = new ApplicationUser
                {
                    NomeCompleto = nomeChefe,
                    UserName = emailChefe,
                    Email = emailChefe,
                    EmailConfirmed = true,
                    StatusCadastro = StatusCadastro.Aprovado,
                    Ativo = true
                };

                var resultadoUsuario =
                    await userManager.CreateAsync(
                        chefe,
                        senhaChefe
                    );

                if (!resultadoUsuario.Succeeded)
                {
                    var erros = string.Join(
                        "; ",
                        resultadoUsuario.Errors.Select(
                            erro => erro.Description
                        )
                    );

                    throw new InvalidOperationException(
                        $"Não foi possível criar a chefe inicial: {erros}"
                    );
                }
            }

            if (!await userManager.IsInRoleAsync(
                    chefe,
                    RolesSistema.Chefe))
            {
                await userManager.AddToRoleAsync(
                    chefe,
                    RolesSistema.Chefe
                );
            }
        }
    }
}