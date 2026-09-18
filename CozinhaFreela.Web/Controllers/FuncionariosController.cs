using CozinhaFreela.Domain.Usuarios;
using CozinhaFreela.Infrastructure.Data;
using CozinhaFreela.Web.Services.Pdf;
using CozinhaFreela.Web.ViewModels.Funcionarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CozinhaFreela.Infrastructure.Email;

namespace CozinhaFreela.Web.Controllers
{
    [Authorize(Roles = RolesSistema.Chefe)]
    public class FuncionariosController : Controller
    {
        private readonly ApplicationDbContext
            _context;
        private readonly IRelatorioFuncionarioPdfService _relatorioFuncionarioPdfService;
        private readonly IRelatorioFuncionariosPdfService _relatorioFuncionariosPdfService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICodigoConfirmacaoEmailService _codigoConfirmacaoEmailService;

        public FuncionariosController(
            ApplicationDbContext context,
            IRelatorioFuncionarioPdfService relatorioFuncionarioPdfService,
            IRelatorioFuncionariosPdfService relatorioFuncionariosPdfService,
            UserManager<ApplicationUser> userManager,
            ICodigoConfirmacaoEmailService codigoConfirmacaoEmailService)
        {
            _context = context;

            _relatorioFuncionarioPdfService =
                relatorioFuncionarioPdfService;

            _relatorioFuncionariosPdfService =
                relatorioFuncionariosPdfService;

            _userManager = userManager;
            _codigoConfirmacaoEmailService =
                codigoConfirmacaoEmailService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dados =
                await _context.Funcionarios
                    .AsNoTracking()
                    .Where(
                        funcionario =>
                            funcionario.Usuario.StatusCadastro ==
                            StatusCadastro.Aprovado
                    )
                    .OrderBy(
                        funcionario =>
                            funcionario.Usuario.NomeCompleto
                    )
                    .Select(
                        funcionario => new
                        {
                            funcionario.UsuarioId,

                            funcionario.Usuario.NomeCompleto,

                            Email =
                                funcionario.Usuario.Email
                                ?? string.Empty,

                            Funcao =
                                funcionario.Funcao
                                ?? "Não definida",

                            funcionario.Telefone,
                            funcionario.Cidade,
                            funcionario.Estado,
                            funcionario.Usuario.Ativo
                        }
                    )
                    .ToListAsync();

            var funcionarios =
                dados.Select(
                    funcionario =>
                        new FuncionarioListaViewModel
                        {
                            UsuarioId =
                                funcionario.UsuarioId,

                            NomeCompleto =
                                funcionario.NomeCompleto,

                            Email =
                                funcionario.Email,

                            Funcao =
                                funcionario.Funcao,

                            Telefone =
                                FormatarTelefone(
                                    funcionario.Telefone
                                ),

                            Cidade =
                                funcionario.Cidade,

                            Estado =
                                funcionario.Estado,

                            Ativo =
                                funcionario.Ativo
                        }
                )
                .ToList();

            return View(funcionarios);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var funcionario =
                await _context.Funcionarios
                    .AsNoTracking()
                    .Include(item => item.Usuario)
                    .FirstOrDefaultAsync(
                        item =>
                            item.UsuarioId == id &&
                            item.Usuario.StatusCadastro ==
                            StatusCadastro.Aprovado
                    );

            if (funcionario is null)
            {
                return NotFound();
            }

            var model =
                new DetalhesFuncionarioViewModel
                {
                    UsuarioId =
                        funcionario.UsuarioId,

                    NomeCompleto =
                        funcionario.Usuario.NomeCompleto,

                    Email =
                        funcionario.Usuario.Email
                        ?? string.Empty,

                    EmailConfirmado =
                        funcionario.Usuario.EmailConfirmed,

                    Ativo =
                        funcionario.Usuario.Ativo,

                    Funcao =
                        funcionario.Funcao
                        ?? string.Empty,

                    Cpf =
                        funcionario.Cpf,

                    DataNascimento =
                        funcionario.DataNascimento,

                    Idade =
                        CalcularIdade(
                            funcionario.DataNascimento
                        ),

                    Telefone =
                        FormatarTelefone(
                            funcionario.Telefone
                        ),

                    Cep =
                        funcionario.Cep,

                    Rua =
                        funcionario.Rua,

                    Numero =
                        funcionario.Numero,

                    Complemento =
                        funcionario.Complemento,

                    Bairro =
                        funcionario.Bairro,

                    Cidade =
                        funcionario.Cidade,

                    Estado =
                        funcionario.Estado,

                    Nacionalidade =
                        funcionario.Nacionalidade,

                    EstadoCivil =
                        funcionario.EstadoCivil,

                    ContatoEmergenciaNome =
                        funcionario.ContatoEmergenciaNome,

                    ContatoEmergenciaTelefone =
                        FormatarTelefone(
                            funcionario.ContatoEmergenciaTelefone
                        ),

                    Observacoes =
                        funcionario.Observacoes,

                    DataCadastro =
                        funcionario.Usuario.DataCadastro
                };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> ExportarPdf(
    string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var funcionario =
                await _context.Funcionarios
                    .AsNoTracking()
                    .Include(item => item.Usuario)
                    .FirstOrDefaultAsync(
                        item =>
                            item.UsuarioId == id &&
                            item.Usuario.StatusCadastro ==
                            StatusCadastro.Aprovado
                    );

            if (funcionario is null)
            {
                return NotFound();
            }

            var model =
                new DetalhesFuncionarioViewModel
                {
                    UsuarioId =
                        funcionario.UsuarioId,

                    NomeCompleto =
                        funcionario.Usuario.NomeCompleto,

                    Email =
                        funcionario.Usuario.Email
                        ?? string.Empty,

                    EmailConfirmado =
                        funcionario.Usuario.EmailConfirmed,

                    Ativo =
                        funcionario.Usuario.Ativo,

                    Funcao =
                        funcionario.Funcao
                        ?? string.Empty,

                    Cpf =
                        funcionario.Cpf,

                    DataNascimento =
                        funcionario.DataNascimento,

                    Idade =
                        CalcularIdade(
                            funcionario.DataNascimento
                        ),

                    Telefone =
                        FormatarTelefone(
                            funcionario.Telefone
                        ),

                    Cep =
                        funcionario.Cep,

                    Rua =
                        funcionario.Rua,

                    Numero =
                        funcionario.Numero,

                    Complemento =
                        funcionario.Complemento,

                    Bairro =
                        funcionario.Bairro,

                    Cidade =
                        funcionario.Cidade,

                    Estado =
                        funcionario.Estado,

                    Nacionalidade =
                        funcionario.Nacionalidade,

                    EstadoCivil =
                        funcionario.EstadoCivil,

                    ContatoEmergenciaNome =
                        funcionario.ContatoEmergenciaNome,

                    ContatoEmergenciaTelefone =
                        FormatarTelefone(
                            funcionario.ContatoEmergenciaTelefone
                        ),

                    Observacoes =
                        funcionario.Observacoes,

                    DataCadastro =
                        funcionario.Usuario.DataCadastro
                };

            var arquivoPdf =
                _relatorioFuncionarioPdfService
                    .Gerar(model);

            var nomeArquivo =
                $"ficha-funcionario-{funcionario.UsuarioId}.pdf";

            return File(
                arquivoPdf,
                "application/pdf",
                nomeArquivo
            );
        }

        [HttpGet]
        public async Task<IActionResult> ExportarTodosPdf()
        {
            var funcionarios =
                await _context.Funcionarios
                    .AsNoTracking()
                    .Where(
                        funcionario =>
                            funcionario.Usuario.StatusCadastro ==
                            StatusCadastro.Aprovado
                    )
                    .OrderBy(
                        funcionario =>
                            funcionario.Usuario.NomeCompleto
                    )
                    .Select(
                        funcionario =>
                            new FuncionarioRelatorioGeralViewModel
                            {
                                NomeCompleto =
                                    funcionario.Usuario.NomeCompleto,

                                Funcao =
                                    funcionario.Funcao
                                    ?? "Não definida",

                                Cpf =
                                    funcionario.Cpf,

                                DataNascimento =
                                    funcionario.DataNascimento,

                                Telefone =
                                    funcionario.Telefone,

                                Cidade =
                                    funcionario.Cidade,

                                Estado =
                                    funcionario.Estado,

                                ContatoEmergenciaNome =
                                    funcionario.ContatoEmergenciaNome,

                                ContatoEmergenciaTelefone =
                                    funcionario
                                        .ContatoEmergenciaTelefone,

                                Ativo =
                                    funcionario.Usuario.Ativo
                            }
                    )
                    .ToListAsync();

            foreach (var funcionario in funcionarios)
            {
                funcionario.Telefone =
                    FormatarTelefone(
                        funcionario.Telefone
                    );

                funcionario.ContatoEmergenciaTelefone =
                    FormatarTelefone(
                        funcionario.ContatoEmergenciaTelefone
                    );
            }

            var arquivoPdf =
                _relatorioFuncionariosPdfService
                    .Gerar(funcionarios);

            var nomeArquivo =
                $"funcionarios-{DateTime.Now:yyyy-MM-dd}.pdf";

            return File(
                arquivoPdf,
                "application/pdf",
                nomeArquivo
            );
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

        [HttpGet]
        public async Task<IActionResult> Analisar(
    string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var funcionario =
                await _context.Funcionarios
                    .AsNoTracking()
                    .Include(item => item.Usuario)
                    .FirstOrDefaultAsync(
                        item =>
                            item.UsuarioId == id &&
                            item.Usuario.StatusCadastro ==
                            StatusCadastro.Pendente
                    );

            if (funcionario is null)
            {
                return NotFound();
            }

            var model =
                new AnaliseFuncionarioViewModel
                {
                    UsuarioId =
                        funcionario.UsuarioId,

                    NomeCompleto =
                        funcionario.Usuario.NomeCompleto,

                    Email =
                        funcionario.Usuario.Email
                        ?? string.Empty,

                    EmailConfirmado =
                        funcionario.Usuario.EmailConfirmed,

                    Cpf =
                        funcionario.Cpf,

                    DataNascimento =
                        funcionario.DataNascimento,

                    Idade =
                        CalcularIdade(
                            funcionario.DataNascimento
                        ),

                    Telefone =
                        FormatarTelefone(
                            funcionario.Telefone
                        ),

                    Cep =
                        funcionario.Cep,

                    Rua =
                        funcionario.Rua,

                    Numero =
                        funcionario.Numero,

                    Complemento =
                        funcionario.Complemento,

                    Bairro =
                        funcionario.Bairro,

                    Cidade =
                        funcionario.Cidade,

                    Estado =
                        funcionario.Estado,

                    Nacionalidade =
                        funcionario.Nacionalidade,

                    EstadoCivil =
                        funcionario.EstadoCivil,

                    ContatoEmergenciaNome =
                        funcionario.ContatoEmergenciaNome,

                    ContatoEmergenciaTelefone =
                        FormatarTelefone(
                            funcionario
                                .ContatoEmergenciaTelefone
                        ),

                    Observacoes =
                        funcionario.Observacoes,

                    DataCadastro =
                        funcionario.Usuario.DataCadastro,

                    Funcao =
                        funcionario.Funcao
                        ?? string.Empty
                };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprovar(
    string usuarioId,
    string? funcao)
        {
            funcao = funcao?.Trim();

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(funcao))
            {
                TempData["MensagemErro"] =
                    "Informe a função do funcionário.";

                return RedirectToAction(
                    nameof(Analisar),
                    new
                    {
                        id = usuarioId
                    }
                );
            }

            if (funcao.Length > 80)
            {
                TempData["MensagemErro"] =
                    "A função deve possuir no máximo 80 caracteres.";

                return RedirectToAction(
                    nameof(Analisar),
                    new
                    {
                        id = usuarioId
                    }
                );
            }

            var funcionario =
                await _context.Funcionarios
                    .Include(item => item.Usuario)
                    .FirstOrDefaultAsync(
                        item =>
                            item.UsuarioId == usuarioId &&
                            item.Usuario.StatusCadastro ==
                            StatusCadastro.Pendente
                    );

            if (funcionario is null)
            {
                TempData["MensagemErro"] =
                    "O cadastro não foi encontrado ou já foi analisado.";

                return RedirectToAction(
                    nameof(Pendentes)
                );
            }

            if (!funcionario.Usuario.EmailConfirmed)
            {
                TempData["MensagemErro"] =
                    "O e-mail precisa ser confirmado antes da aprovação.";

                return RedirectToAction(
                    nameof(Analisar),
                    new { id = usuarioId }
                );
            }

            funcionario.Funcao = funcao;

            funcionario.Usuario.StatusCadastro =
                StatusCadastro.Aprovado;

            funcionario.Usuario.Ativo = true;

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                $"O cadastro de {funcionario.Usuario.NomeCompleto} foi aprovado.";

            return RedirectToAction(
                nameof(Pendentes)
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recusar(
    string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest();
            }

            var funcionario =
                await _context.Funcionarios
                    .Include(item => item.Usuario)
                    .FirstOrDefaultAsync(
                        item =>
                            item.UsuarioId == usuarioId &&
                            item.Usuario.StatusCadastro ==
                            StatusCadastro.Pendente
                    );

            if (funcionario is null)
            {
                TempData["MensagemErro"] =
                    "O cadastro não foi encontrado ou já foi analisado.";

                return RedirectToAction(
                    nameof(Pendentes)
                );
            }

            funcionario.Funcao = null;

            funcionario.Usuario.StatusCadastro =
                StatusCadastro.Recusado;

            funcionario.Usuario.Ativo = false;

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] =
                $"O cadastro de {funcionario.Usuario.NomeCompleto} foi recusado.";

            return RedirectToAction(
                nameof(Pendentes)
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarCodigo(
            string usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId);

            if (usuario is null || usuario.EmailConfirmed)
            {
                TempData["MensagemErro"] =
                    "O cadastro não foi encontrado ou o e-mail já foi confirmado.";

                return RedirectToAction(nameof(Pendentes));
            }

            await _codigoConfirmacaoEmailService.GerarEEnviarAsync(
                usuario
            );

            TempData["MensagemSucesso"] =
                $"Um novo código foi enviado para {usuario.Email}.";

            return RedirectToAction(nameof(Pendentes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CorrigirEmail(
            string usuarioId,
            string novoEmail)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId);

            if (usuario is null || usuario.EmailConfirmed)
            {
                TempData["MensagemErro"] =
                    "Somente cadastros sem confirmação podem ter o e-mail corrigido aqui.";

                return RedirectToAction(nameof(Pendentes));
            }

            novoEmail = novoEmail?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(novoEmail) ||
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(novoEmail))
            {
                TempData["MensagemErro"] =
                    "Informe um e-mail válido.";

                return RedirectToAction(nameof(Pendentes));
            }

            var usuarioDoEmail =
                await _userManager.FindByEmailAsync(novoEmail);

            if (usuarioDoEmail is not null &&
                usuarioDoEmail.Id != usuario.Id)
            {
                TempData["MensagemErro"] =
                    "Este e-mail já está sendo utilizado.";

                return RedirectToAction(nameof(Pendentes));
            }

            usuario.Email = novoEmail;
            usuario.UserName = novoEmail;
            usuario.EmailConfirmed = false;
            usuario.DataExpiracaoConfirmacao =
                DateTime.UtcNow.AddHours(24);

            var resultado = await _userManager.UpdateAsync(usuario);

            if (!resultado.Succeeded)
            {
                TempData["MensagemErro"] =
                    "Não foi possível corrigir o e-mail.";

                return RedirectToAction(nameof(Pendentes));
            }

            await _codigoConfirmacaoEmailService.GerarEEnviarAsync(
                usuario
            );

            TempData["MensagemSucesso"] =
                "E-mail corrigido e novo código enviado.";

            return RedirectToAction(nameof(Pendentes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirNaoConfirmado(
            string usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId);

            if (usuario is null || usuario.EmailConfirmed)
            {
                TempData["MensagemErro"] =
                    "Somente cadastros sem e-mail confirmado podem ser excluídos por esta ação.";

                return RedirectToAction(nameof(Pendentes));
            }

            var nome = usuario.NomeCompleto;
            var resultado = await _userManager.DeleteAsync(usuario);

            TempData[resultado.Succeeded
                ? "MensagemSucesso"
                : "MensagemErro"] = resultado.Succeeded
                    ? $"O cadastro de {nome} foi excluído."
                    : "Não foi possível excluir o cadastro.";

            return RedirectToAction(nameof(Pendentes));
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

        private static int CalcularIdade(
    DateOnly dataNascimento)
        {
            var hoje =
                DateOnly.FromDateTime(
                    DateTime.Today
                );

            var idade =
                hoje.Year - dataNascimento.Year;

            if (dataNascimento >
                hoje.AddYears(-idade))
            {
                idade--;
            }

            return idade;
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
