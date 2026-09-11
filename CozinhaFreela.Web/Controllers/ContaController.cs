using System.Text.RegularExpressions;
using CozinhaFreela.Domain.Usuarios;
using CozinhaFreela.Infrastructure.Data;
using CozinhaFreela.Infrastructure.Email;
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

        private readonly ICodigoConfirmacaoEmailService
            _codigoConfirmacaoEmailService;

        private readonly ILogger<ContaController> _logger;

        public ContaController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ICodigoConfirmacaoEmailService
                codigoConfirmacaoEmailService,
            ILogger<ContaController> logger)
        {
            _userManager = userManager;
            _context = context;
            _codigoConfirmacaoEmailService =
                codigoConfirmacaoEmailService;
            _logger = logger;
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
                SomenteNumeros(
                    model.ContatoEmergenciaTelefone
                );

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

            var hoje =
                DateOnly.FromDateTime(DateTime.Today);

            if (model.DataNascimento == default ||
                model.DataNascimento >= hoje)
            {
                ModelState.AddModelError(
                    nameof(model.DataNascimento),
                    "Informe uma data de nascimento válida."
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
                await _context.Database
                    .BeginTransactionAsync();

            var usuario = new ApplicationUser
            {
                NomeCompleto = model.NomeCompleto.Trim(),
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = false,
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
                AdicionarErros(resultadoUsuario);

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
                Complemento =
                    model.Complemento?.Trim(),
                Bairro = model.Bairro.Trim(),
                Cidade = model.Cidade.Trim(),
                Estado =
                    model.Estado
                        .Trim()
                        .ToUpperInvariant(),
                Nacionalidade =
                    model.Nacionalidade.Trim(),
                EstadoCivil =
                    model.EstadoCivil.Trim(),
                Funcao = null,
                ContatoEmergenciaNome =
                    model.ContatoEmergenciaNome.Trim(),
                ContatoEmergenciaTelefone =
                    model.ContatoEmergenciaTelefone,
                Observacoes =
                    model.Observacoes?.Trim()
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
                AdicionarErros(resultadoRole);

                return View(model);
            }

            await transacao.CommitAsync();

            try
            {
                await _codigoConfirmacaoEmailService
                    .GerarEEnviarAsync(usuario);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Não foi possível enviar o código de confirmação para o usuário {UsuarioId}.",
                    usuario.Id
                );

                TempData["AvisoEnvioEmail"] =
                    "O cadastro foi salvo, mas não foi possível enviar o código. Você poderá solicitar um novo código.";

                return RedirectToAction(
                    nameof(ConfirmacaoPendente)
                );
            }

            return RedirectToAction(
                nameof(ConfirmarEmail),
                new
                {
                    usuarioId = usuario.Id
                }
            );
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(
            string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return RedirectToAction("Index", "Home");
            }

            var usuario =
                await _userManager.FindByIdAsync(usuarioId);

            if (usuario is null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (usuario.EmailConfirmed)
            {
                return RedirectToAction(
                    nameof(ConfirmacaoConcluida)
                );
            }

            ViewBag.EmailMascarado =
                MascararEmail(usuario.Email);

            var model = new ConfirmarEmailViewModel
            {
                UsuarioId = usuario.Id
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEmail(
            ConfirmarEmailViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UsuarioId))
            {
                return RedirectToAction("Index", "Home");
            }

            var usuario =
                await _userManager.FindByIdAsync(
                    model.UsuarioId
                );

            if (usuario is null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (usuario.EmailConfirmed)
            {
                return RedirectToAction(
                    nameof(ConfirmacaoConcluida)
                );
            }

            ViewBag.EmailMascarado =
                MascararEmail(usuario.Email);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado =
                await _codigoConfirmacaoEmailService
                    .ConfirmarAsync(
                        model.UsuarioId,
                        model.Codigo
                    );

            switch (resultado)
            {
                case ResultadoConfirmacaoEmail.Sucesso:
                    return RedirectToAction(
                        nameof(ConfirmacaoConcluida)
                    );

                case ResultadoConfirmacaoEmail.CodigoInvalido:
                    ModelState.AddModelError(
                        nameof(model.Codigo),
                        "O código informado está incorreto."
                    );
                    break;

                case ResultadoConfirmacaoEmail.CodigoExpirado:
                    ModelState.AddModelError(
                        nameof(model.Codigo),
                        "Este código expirou."
                    );
                    break;

                case ResultadoConfirmacaoEmail
                    .LimiteTentativasExcedido:

                    ModelState.AddModelError(
                        nameof(model.Codigo),
                        "O limite de tentativas foi atingido."
                    );
                    break;

                default:
                    ModelState.AddModelError(
                        nameof(model.Codigo),
                        "Não existe um código válido para esta conta."
                    );
                    break;
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ConfirmacaoConcluida()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ConfirmacaoPendente()
        {
            return View();
        }

        private void AdicionarErros(
            IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    erro.Description
                );
            }
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

        private static string MascararEmail(
            string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return string.Empty;
            }

            var partes = email.Split('@');

            if (partes.Length != 2 ||
                string.IsNullOrWhiteSpace(partes[0]))
            {
                return email;
            }

            var nome = partes[0];

            var inicio =
                nome.Length <= 2
                    ? nome[..1]
                    : nome[..2];

            return $"{inicio}***@{partes[1]}";
        }
    }
}