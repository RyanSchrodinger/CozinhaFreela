using CozinhaFreela.Domain.Usuarios;
using CozinhaFreela.Infrastructure.Data;
using CozinhaFreela.Web.ViewModels.Funcionarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CozinhaFreela.Web.Controllers
{
    [Authorize(Roles = RolesSistema.Chefe)]
    public class FuncionariosController : Controller
    {
        private readonly ApplicationDbContext
            _context;

        public FuncionariosController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Pendentes()
        {
            var dados =
                await _context.Funcionarios
                    .AsNoTracking()
                    .Where(
                        funcionario =>
                            funcionario.Usuario
                                .StatusCadastro ==
                            StatusCadastro.Pendente
                    )
                    .OrderBy(
                        funcionario =>
                            funcionario.Usuario
                                .DataCadastro
                    )
                    .Select(
                        funcionario => new
                        {
                            funcionario.UsuarioId,

                            funcionario.Usuario
                                .NomeCompleto,

                            Email =
                                funcionario.Usuario.Email
                                ?? string.Empty,

                            funcionario.Cpf,
                            funcionario.Telefone,
                            funcionario.Cidade,
                            funcionario.Estado,

                            funcionario.Usuario
                                .DataCadastro,

                            EmailConfirmado =
                                funcionario.Usuario
                                    .EmailConfirmed
                        }
                    )
                    .ToListAsync();

            var funcionarios =
                dados.Select(
                    funcionario =>
                        new FuncionarioPendenteViewModel
                        {
                            UsuarioId =
                                funcionario.UsuarioId,

                            NomeCompleto =
                                funcionario.NomeCompleto,

                            Email =
                                funcionario.Email,

                            CpfMascarado =
                                MascararCpf(
                                    funcionario.Cpf
                                ),

                            Telefone =
                                FormatarTelefone(
                                    funcionario.Telefone
                                ),

                            Cidade =
                                funcionario.Cidade,

                            Estado =
                                funcionario.Estado,

                            DataCadastro =
                                funcionario.DataCadastro,

                            EmailConfirmado =
                                funcionario.EmailConfirmado
                        }
                )
                .ToList();

            return View(funcionarios);
        }

        private static string MascararCpf(
            string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf) ||
                cpf.Length != 11)
            {
                return "***.***.***-**";
            }

            return
                $"***.***.***-{cpf.Substring(9, 2)}";
        }

        private static string FormatarTelefone(
            string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
            {
                return string.Empty;
            }

            if (telefone.Length == 11)
            {
                return
                    $"({telefone[..2]}) " +
                    $"{telefone.Substring(2, 5)}-" +
                    $"{telefone.Substring(7, 4)}";
            }

            if (telefone.Length == 10)
            {
                return
                    $"({telefone[..2]}) " +
                    $"{telefone.Substring(2, 4)}-" +
                    $"{telefone.Substring(6, 4)}";
            }

            return telefone;
        }
    }
}