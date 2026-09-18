using System.Text.RegularExpressions;
using CozinhaFreela.Domain.Usuarios;
using CozinhaFreela.Infrastructure.Data;
using CozinhaFreela.Infrastructure.Email;
using CozinhaFreela.Web.ViewModels.Conta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CozinhaFreela.Web.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<ApplicationUser>
            _userManager;

        private readonly SignInManager<ApplicationUser>
            _signInManager;

        private readonly ApplicationDbContext
            _context;

        private readonly ICodigoConfirmacaoEmailService
            _codigoConfirmacaoEmailService;

        private readonly ILogger<ContaController>
            _logger;

        public ContaController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ICodigoConfirmacaoEmailService
                codigoConfirmacaoEmailService,
            ILogger<ContaController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;

            _codigoConfirmacaoEmailService =
                codigoConfirmacaoEmailService;

            _logger = logger;
        }

        /*
            CADASTRO
        */

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Cadastro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
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
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            model.Cpf =
                SomenteNumeros(model.Cpf);

            model.Cep =
                SomenteNumeros(model.Cep);

            model.Telefone =
                SomenteNumeros(model.Telefone);

            model.ContatoEmergenciaTelefone =
                SomenteNumeros(
                    model.ContatoEmergenciaTelefone
                );

            DateOnly dataNascimento;

            if (!DateOnly.TryParseExact(
                    model.DataNascimento?.Trim(),
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dataNascimento))
            {
                ModelState.AddModelError(
                    nameof(model.DataNascimento),
                    "Informe a data no formato dia/mês/ano."
                );
            }

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
                DateOnly.FromDateTime(
                    DateTime.Today
                );

            if (dataNascimento == default ||
                dataNascimento >= hoje)
            {
                ModelState.AddModelError(
                    nameof(model.DataNascimento),
                    "Informe uma data de nascimento válida."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
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

                return View(model);
            }

            var estrategia =
                _context.Database
                    .CreateExecutionStrategy();

            (
                IdentityResult Resultado,
                ApplicationUser? Usuario
            ) resultadoTransacao;

            try
            {
                resultadoTransacao =
                    await estrategia.ExecuteAsync(
                        async () =>
                        {
                            await using var transacao =
                                await _context.Database
                                    .BeginTransactionAsync();

                            var usuario =
                                new ApplicationUser
                                {
                                    NomeCompleto =
                                        model.NomeCompleto.Trim(),

                                    UserName =
                                        model.Email.Trim(),

                                    Email =
                                        model.Email.Trim(),

                                    EmailConfirmed = false,

                                    DataExpiracaoConfirmacao =
                                        DateTime.UtcNow.AddHours(24),

                                    StatusCadastro =
                                        StatusCadastro.Pendente,

                                    Ativo = false
                                };

                            var resultadoUsuario =
                                await _userManager.CreateAsync(
                                    usuario,
                                    model.Senha
                                );

                            if (!resultadoUsuario.Succeeded)
                            {
                                return (
                                    Resultado:
                                        resultadoUsuario,

                                    Usuario:
                                        (ApplicationUser?)null
                                );
                            }

                            var funcionario =
                                new Funcionario
                                {
                                    UsuarioId = usuario.Id,
                                    Cpf = model.Cpf,

                                    DataNascimento =
                                        dataNascimento,

                                    Telefone =
                                        model.Telefone,

                                    Cep = model.Cep,

                                    Rua =
                                        model.Rua.Trim(),

                                    Numero =
                                        model.Numero.Trim(),

                                    Complemento =
                                        model.Complemento?.Trim(),

                                    Bairro =
                                        model.Bairro.Trim(),

                                    Cidade =
                                        model.Cidade.Trim(),

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
                                        model
                                            .ContatoEmergenciaNome
                                            .Trim(),

                                    ContatoEmergenciaTelefone =
                                        model
                                            .ContatoEmergenciaTelefone,

                                    Observacoes =
                                        model.Observacoes?.Trim()
                                };

                            _context.Funcionarios.Add(
                                funcionario
                            );

                            await _context.SaveChangesAsync();

                            var resultadoRole =
                                await _userManager.AddToRoleAsync(
                                    usuario,
                                    RolesSistema.Funcionario
                                );

                            if (!resultadoRole.Succeeded)
                            {
                                return (
                                    Resultado:
                                        resultadoRole,

                                    Usuario:
                                        (ApplicationUser?)null
                                );
                            }

                            await transacao.CommitAsync();

                            return (
                                Resultado:
                                    IdentityResult.Success,

                                Usuario:
                                    (ApplicationUser?)usuario
                            );
                        }
                    );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Ocorreu um erro durante o cadastro de um funcionário."
                );

                ModelState.AddModelError(
                    string.Empty,
                    "Não foi possível concluir o cadastro agora. Aguarde alguns instantes e tente novamente."
                );

                return View(model);
            }

            if (!resultadoTransacao.Resultado.Succeeded)
            {
                AdicionarErros(
                    resultadoTransacao.Resultado
                );

                return View(model);
            }

            var usuarioCriado =
                resultadoTransacao.Usuario;

            if (usuarioCriado is null)
            {
                _logger.LogError(
                    "O cadastro foi concluído sem retornar o usuário criado."
                );

                ModelState.AddModelError(
                    string.Empty,
                    "Não foi possível concluir o cadastro."
                );

                return View(model);
            }

            try
            {
                await _codigoConfirmacaoEmailService
                    .GerarEEnviarAsync(
                        usuarioCriado
                    );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Não foi possível enviar o código de confirmação para o usuário {UsuarioId}.",
                    usuarioCriado.Id
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
                    usuarioId =
                        usuarioCriado.Id
                }
            );
        }

        /*
            CONFIRMAÇÃO DO E-MAIL
        */

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(
            string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(
                    usuarioId))
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            var usuario =
                await _userManager.FindByIdAsync(
                    usuarioId
                );

            if (usuario is null)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            if (!usuario.EmailConfirmed &&
                usuario.DataExpiracaoConfirmacao.HasValue &&
                usuario.DataExpiracaoConfirmacao <= DateTime.UtcNow)
            {
                await _userManager.DeleteAsync(usuario);

                TempData["CadastroExpirado"] =
                    "O prazo de 24 horas terminou. Faça um novo cadastro para continuar.";

                return RedirectToAction(nameof(Cadastro));
            }

            if (usuario.EmailConfirmed)
            {
                return RedirectToAction(
                    nameof(ConfirmacaoConcluida)
                );
            }

            ViewBag.EmailMascarado =
                MascararEmail(usuario.Email);

            var model =
                new ConfirmarEmailViewModel
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
            if (string.IsNullOrWhiteSpace(
                    model.UsuarioId))
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            var usuario =
                await _userManager.FindByIdAsync(
                    model.UsuarioId
                );

            if (usuario is null)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
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
                        nameof(
                            ConfirmacaoConcluida
                        )
                    );

                case ResultadoConfirmacaoEmail
                    .CodigoInvalido:

                    ModelState.AddModelError(
                        nameof(model.Codigo),
                        "O código informado está incorreto."
                    );
                    break;

                case ResultadoConfirmacaoEmail
                    .CodigoExpirado:

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

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarCodigo(
            ReenviarCodigoViewModel model)
        {
            var usuario = await _userManager.FindByIdAsync(
                model.UsuarioId
            );

            if (usuario is null || usuario.EmailConfirmed)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!await _userManager.CheckPasswordAsync(
                    usuario,
                    model.Senha))
            {
                TempData["ErroConfirmacao"] =
                    "A senha informada está incorreta.";

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id }
                );
            }

            var ultimoEnvio = await _context.CodigosConfirmacaoEmail
                .Where(codigo => codigo.UsuarioId == usuario.Id)
                .OrderByDescending(codigo => codigo.DataCriacao)
                .Select(codigo => (DateTime?)codigo.DataCriacao)
                .FirstOrDefaultAsync();

            if (ultimoEnvio.HasValue &&
                ultimoEnvio.Value.AddMinutes(1) > DateTime.UtcNow)
            {
                TempData["ErroConfirmacao"] =
                    "Aguarde um minuto antes de solicitar outro código.";

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id }
                );
            }

            await _codigoConfirmacaoEmailService.GerarEEnviarAsync(
                usuario
            );

            TempData["SucessoConfirmacao"] =
                "Um novo código foi enviado para seu e-mail.";

            return RedirectToAction(
                nameof(ConfirmarEmail),
                new { usuarioId = usuario.Id }
            );
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarEmailPendente(
            AlterarEmailPendenteViewModel model)
        {
            var usuario = await _userManager.FindByIdAsync(
                model.UsuarioId
            );

            if (usuario is null || usuario.EmailConfirmed)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid ||
                !await _userManager.CheckPasswordAsync(
                    usuario,
                    model.Senha))
            {
                TempData["ErroConfirmacao"] =
                    "Confira os dados e informe a senha correta.";

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id }
                );
            }

            var novoEmail = model.NovoEmail.Trim();

            var usuarioDoEmail =
                await _userManager.FindByEmailAsync(novoEmail);

            if (usuarioDoEmail is not null &&
                usuarioDoEmail.Id != usuario.Id)
            {
                TempData["ErroConfirmacao"] =
                    "Este e-mail já está sendo utilizado.";

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id }
                );
            }

            usuario.Email = novoEmail;
            usuario.UserName = novoEmail;
            usuario.EmailConfirmed = false;
            usuario.DataExpiracaoConfirmacao =
                DateTime.UtcNow.AddHours(24);

            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
            {
                TempData["ErroConfirmacao"] =
                    "Não foi possível alterar o e-mail.";

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id }
                );
            }

            await _codigoConfirmacaoEmailService.GerarEEnviarAsync(
                usuario
            );

            TempData["SucessoConfirmacao"] =
                "E-mail alterado. Enviamos um novo código.";

            return RedirectToAction(
                nameof(ConfirmarEmail),
                new { usuarioId = usuario.Id }
            );
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarCadastro(
            CancelarCadastroViewModel model)
        {
            var usuario = await _userManager.FindByIdAsync(
                model.UsuarioId
            );

            if (usuario is null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (usuario.EmailConfirmed ||
                !await _userManager.CheckPasswordAsync(
                    usuario,
                    model.Senha))
            {
                TempData["ErroConfirmacao"] =
                    "Não foi possível cancelar. Verifique sua senha.";

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id }
                );
            }

            var resultado = await _userManager.DeleteAsync(usuario);

            if (!resultado.Succeeded)
            {
                TempData["ErroConfirmacao"] =
                    "Não foi possível cancelar o cadastro.";

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id }
                );
            }

            TempData["CadastroCancelado"] =
                "Seu cadastro foi cancelado e os dados foram excluídos.";

            return RedirectToAction("Index", "Home");
        }

        /*
            MEU PERFIL
        */

        [Authorize(Roles = RolesSistema.Funcionario)]
        [HttpGet]
        public async Task<IActionResult> MeuPerfil()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario is null)
            {
                return Challenge();
            }

            var funcionario = await _context.Funcionarios
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.UsuarioId == usuario.Id);

            if (funcionario is null)
            {
                return NotFound();
            }

            var model = new MeuPerfilViewModel
            {
                NomeCompleto = usuario.NomeCompleto,
                Email = usuario.Email ?? string.Empty,
                Cpf = FormatarCpf(funcionario.Cpf),
                DataNascimento = funcionario.DataNascimento
                    .ToString("dd/MM/yyyy"),
                Telefone = FormatarTelefone(funcionario.Telefone),
                Cep = FormatarCep(funcionario.Cep),
                Rua = funcionario.Rua,
                Numero = funcionario.Numero,
                Complemento = funcionario.Complemento,
                Bairro = funcionario.Bairro,
                Cidade = funcionario.Cidade,
                Estado = funcionario.Estado,
                Nacionalidade = funcionario.Nacionalidade,
                EstadoCivil = funcionario.EstadoCivil,
                ContatoEmergenciaNome =
                    funcionario.ContatoEmergenciaNome,
                ContatoEmergenciaTelefone = FormatarTelefone(
                    funcionario.ContatoEmergenciaTelefone),
                Observacoes = funcionario.Observacoes
            };

            return View(model);
        }

        [Authorize(Roles = RolesSistema.Funcionario)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MeuPerfil(
            MeuPerfilViewModel model)
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (usuario is null)
            {
                return Challenge();
            }

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(item =>
                    item.UsuarioId == usuario.Id);

            if (funcionario is null)
            {
                return NotFound();
            }

            model.Cpf = SomenteNumeros(model.Cpf);
            model.Telefone = SomenteNumeros(model.Telefone);
            model.Cep = SomenteNumeros(model.Cep);
            model.ContatoEmergenciaTelefone = SomenteNumeros(
                model.ContatoEmergenciaTelefone);

            if (model.Cpf.Length != 11)
            {
                ModelState.AddModelError(
                    nameof(model.Cpf),
                    "O CPF deve possuir 11 números.");
            }

            if (model.Cep.Length != 8)
            {
                ModelState.AddModelError(
                    nameof(model.Cep),
                    "O CEP deve possuir 8 números.");
            }

            if (!DateOnly.TryParseExact(
                    model.DataNascimento?.Trim(),
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dataNascimento) ||
                dataNascimento >= DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    nameof(model.DataNascimento),
                    "Informe uma data de nascimento válida no formato dia/mês/ano.");
            }

            var cpfEmUso = await _context.Funcionarios
                .AsNoTracking()
                .AnyAsync(item =>
                    item.Cpf == model.Cpf &&
                    item.UsuarioId != usuario.Id);

            if (cpfEmUso)
            {
                ModelState.AddModelError(
                    nameof(model.Cpf),
                    "Este CPF já pertence a outro cadastro.");
            }

            var novoEmail = (model.Email ?? string.Empty).Trim();
            var emailAlterado = !string.Equals(
                usuario.Email,
                novoEmail,
                StringComparison.OrdinalIgnoreCase);
            var cpfAlterado = funcionario.Cpf != model.Cpf;

            if ((emailAlterado || cpfAlterado) &&
                string.IsNullOrWhiteSpace(model.SenhaAtual))
            {
                ModelState.AddModelError(
                    nameof(model.SenhaAtual),
                    "Informe sua senha para alterar o CPF ou o e-mail.");
            }
            else if ((emailAlterado || cpfAlterado) &&
                !await _userManager.CheckPasswordAsync(
                    usuario,
                    model.SenhaAtual!))
            {
                ModelState.AddModelError(
                    nameof(model.SenhaAtual),
                    "A senha informada está incorreta.");
            }

            if (emailAlterado)
            {
                var usuarioDoEmail =
                    await _userManager.FindByEmailAsync(novoEmail);

                if (usuarioDoEmail is not null &&
                    usuarioDoEmail.Id != usuario.Id)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "Este e-mail já está sendo utilizado.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            usuario.NomeCompleto = model.NomeCompleto.Trim();
            funcionario.Cpf = model.Cpf;
            funcionario.DataNascimento = dataNascimento;
            funcionario.Telefone = model.Telefone;
            funcionario.Cep = model.Cep;
            funcionario.Rua = model.Rua.Trim();
            funcionario.Numero = model.Numero.Trim();
            funcionario.Complemento = model.Complemento?.Trim();
            funcionario.Bairro = model.Bairro.Trim();
            funcionario.Cidade = model.Cidade.Trim();
            funcionario.Estado = model.Estado.Trim().ToUpperInvariant();
            funcionario.Nacionalidade = model.Nacionalidade.Trim();
            funcionario.EstadoCivil = model.EstadoCivil.Trim();
            funcionario.ContatoEmergenciaNome =
                model.ContatoEmergenciaNome.Trim();
            funcionario.ContatoEmergenciaTelefone =
                model.ContatoEmergenciaTelefone;
            funcionario.Observacoes = model.Observacoes?.Trim();

            if (emailAlterado)
            {
                usuario.Email = novoEmail;
                usuario.UserName = novoEmail;
                usuario.EmailConfirmed = false;
                usuario.DataExpiracaoConfirmacao =
                    DateTime.UtcNow.AddHours(24);
            }

            var resultadoUsuario =
                await _userManager.UpdateAsync(usuario);

            if (!resultadoUsuario.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Não foi possível atualizar o perfil.");

                return View(model);
            }

            await _context.SaveChangesAsync();

            if (emailAlterado)
            {
                await _signInManager.SignOutAsync();

                try
                {
                    await _codigoConfirmacaoEmailService
                        .GerarEEnviarAsync(usuario);

                    TempData["SucessoConfirmacao"] =
                        "E-mail alterado. Enviamos um novo código de confirmação.";
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Não foi possível enviar o código após a alteração do e-mail do usuário {UsuarioId}.",
                        usuario.Id);

                    TempData["ErroConfirmacao"] =
                        "O e-mail foi alterado, mas o código não pôde ser enviado. Use a opção de reenviar código.";
                }

                return RedirectToAction(
                    nameof(ConfirmarEmail),
                    new { usuarioId = usuario.Id });
            }

            TempData["PerfilAtualizado"] =
                "Suas informações foram atualizadas.";

            return RedirectToAction(nameof(MeuPerfil));
        }

        /*
            LOGIN
        */

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(
            string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Email =
                model.Email.Trim();

            var usuario =
                await _userManager.FindByEmailAsync(
                    model.Email
                );

            if (usuario is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "E-mail ou senha inválidos."
                );

                return View(model);
            }

            var resultado =
                await _signInManager
                    .PasswordSignInAsync(
                        usuario,
                        model.Senha,
                        model.LembrarMe,
                        lockoutOnFailure: true
                    );

            if (resultado.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "A conta foi temporariamente bloqueada devido a várias tentativas. Tente novamente em 15 minutos."
                );

                return View(model);
            }

            if (resultado.IsNotAllowed)
            {
                if (!usuario.EmailConfirmed)
                {
                    if (await _userManager.CheckPasswordAsync(
                            usuario,
                            model.Senha))
                    {
                        return RedirectToAction(
                            nameof(ConfirmarEmail),
                            new { usuarioId = usuario.Id }
                        );
                    }

                    ModelState.AddModelError(
                        string.Empty,
                        "E-mail ou senha inválidos."
                    );
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "O acesso desta conta ainda não está permitido."
                    );
                }

                return View(model);
            }

            if (!resultado.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "E-mail ou senha inválidos."
                );

                return View(model);
            }

            if (usuario.StatusCadastro ==
                StatusCadastro.Pendente)
            {
                await _signInManager.SignOutAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Seu e-mail foi confirmado, mas seu cadastro ainda está aguardando aprovação."
                );

                return View(model);
            }

            if (usuario.StatusCadastro ==
                StatusCadastro.Recusado)
            {
                await _signInManager.SignOutAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Este cadastro não foi aprovado. Procure a responsável pela equipe."
                );

                return View(model);
            }

            if (!usuario.Ativo)
            {
                await _signInManager.SignOutAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Esta conta está desativada. Procure a responsável pela equipe."
                );

                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(
                    model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(
                    model.ReturnUrl
                );
            }

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        /*
            LOGOUT E ACESSO NEGADO
        */

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AcessoNegado()
        {
            return View();
        }

        /*
            MÉTODOS AUXILIARES
        */

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

        private static string FormatarCpf(string cpf)
        {
            var numeros = SomenteNumeros(cpf);

            return numeros.Length == 11
                ? $"{numeros[..3]}.{numeros.Substring(3, 3)}.{numeros.Substring(6, 3)}-{numeros[9..]}"
                : cpf;
        }

        private static string FormatarTelefone(string telefone)
        {
            var numeros = SomenteNumeros(telefone);

            return numeros.Length switch
            {
                11 => $"({numeros[..2]}) {numeros.Substring(2, 5)}-{numeros[7..]}",
                10 => $"({numeros[..2]}) {numeros.Substring(2, 4)}-{numeros[6..]}",
                _ => telefone
            };
        }

        private static string FormatarCep(string cep)
        {
            var numeros = SomenteNumeros(cep);

            return numeros.Length == 8
                ? $"{numeros[..5]}-{numeros[5..]}"
                : cep;
        }
    }
}
