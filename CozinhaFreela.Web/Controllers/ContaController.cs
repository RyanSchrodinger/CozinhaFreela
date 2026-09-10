using System.Text.RegularExpressions;
using CozinhaFreela.Domain.Usuarios;
using CozinhaFreela.Infrastructure.Data;
using CozinhaFreela.Web.ViewModels.Conta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CozinhaFreela.Web.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ContaController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Cadastro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(
            CadastroViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            model.Cpf = SomenteNumeros(model.Cpf);
            model.Cep = SomenteNumeros(model.Cep);
            model.Telefone = SomenteNumeros(model.Telefone);
            model.ContatoEmergenciaTelefone =
                SomenteNumeros(model.ContatoEmergenciaTelefone);

            if (model.Cpf.Length != 11)
            {
                ModelState.AddModelError(
                    nameof(model.Cpf),
                    "O CPF deve possuir 11 números."
                );
            }

            if (model.Cep.Length != 8)
            {
                ModelState.AddModelError(
                    nameof(model.Cep),
                    "O CEP deve possuir 8 números."
                );
            }

            var cpfExistente =
                await _context.Funcionarios
                    .AsNoTracking()
                    .AnyAsync(
                        funcionario =>
                            funcionario.Cpf == model.Cpf
                    );

            if (cpfExistente)
            {
                ModelState.AddModelError(
                    nameof(model.Cpf),
                    "Já existe um cadastro com este CPF."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await using var transacao =
                await _context.Database.BeginTransactionAsync();

            var usuario = new ApplicationUser
            {
                NomeCompleto = model.NomeCompleto.Trim(),
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                StatusCadastro = StatusCadastro.Pendente,
                Ativo = false
            };

            var resultadoUsuario =
                await _userManager.CreateAsync(
                    usuario,
                    model.Senha
                );

            if (!resultadoUsuario.Succeeded)
            {
                foreach (var erro in resultadoUsuario.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        erro.Description
                    );
                }

                return View(model);
            }

            var funcionario = new Funcionario
            {
                UsuarioId = usuario.Id,
                Cpf = model.Cpf,
                DataNascimento = model.DataNascimento,
                Telefone = model.Telefone,
                Cep = model.Cep,
                Rua = model.Rua.Trim(),
                Numero = model.Numero.Trim(),
                Complemento = model.Complemento?.Trim(),
                Bairro = model.Bairro.Trim(),
                Cidade = model.Cidade.Trim(),
                Estado = model.Estado.Trim().ToUpperInvariant(),
                Nacionalidade = model.Nacionalidade.Trim(),
                EstadoCivil = model.EstadoCivil.Trim(),
                ContatoEmergenciaNome =
                    model.ContatoEmergenciaNome.Trim(),
                ContatoEmergenciaTelefone =
                    model.ContatoEmergenciaTelefone,
                Observacoes = model.Observacoes?.Trim()
            };

            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();

            var resultadoRole =
                await _userManager.AddToRoleAsync(
                    usuario,
                    RolesSistema.Funcionario
                );

            if (!resultadoRole.Succeeded)
            {
                throw new InvalidOperationException(
                    "Não foi possível definir o perfil do funcionário."
                );
            }

            await transacao.CommitAsync();

            return RedirectToAction(
                nameof(ConfirmacaoPendente)
            );
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ConfirmacaoPendente()
        {
            return View();
        }

        private static string SomenteNumeros(
            string? valor)
        {
            return Regex.Replace(
                valor ?? string.Empty,
                @"\D",
                string.Empty
            );
        }
    }
}