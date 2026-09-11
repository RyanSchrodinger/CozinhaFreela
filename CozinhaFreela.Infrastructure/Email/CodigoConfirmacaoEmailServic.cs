using System.Net;
using System.Security.Cryptography;
using CozinhaFreela.Domain.Usuarios;
using CozinhaFreela.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CozinhaFreela.Infrastructure.Email
{
    public class CodigoConfirmacaoEmailService
        : ICodigoConfirmacaoEmailService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public CodigoConfirmacaoEmailService(
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task GerarEEnviarAsync(
            ApplicationUser usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Email))
            {
                throw new InvalidOperationException(
                    "O usuário não possui um e-mail válido."
                );
            }

            var codigosAnteriores =
                await _context.CodigosConfirmacaoEmail
                    .Where(codigo =>
                        codigo.UsuarioId == usuario.Id &&
                        codigo.DataConfirmacao == null)
                    .ToListAsync();

            _context.CodigosConfirmacaoEmail.RemoveRange(
                codigosAnteriores
            );

            var codigo =
                RandomNumberGenerator
                    .GetInt32(0, 1_000_000)
                    .ToString("D6");

            var registro = new CodigoConfirmacaoEmail
            {
                UsuarioId = usuario.Id,
                DataCriacao = DateTime.UtcNow,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10)
            };

            var hasher =
                new PasswordHasher<CodigoConfirmacaoEmail>();

            registro.CodigoHash =
                hasher.HashPassword(registro, codigo);

            _context.CodigosConfirmacaoEmail.Add(registro);

            await _context.SaveChangesAsync();

            var nomeSeguro =
                WebUtility.HtmlEncode(usuario.NomeCompleto);

            var conteudoHtml = $"""
                <div style="font-family: Arial, sans-serif;
                            max-width: 560px;
                            margin: 0 auto;
                            color: #222;">
                    <h2>Confirmação de e-mail</h2>

                    <p>Olá, {nomeSeguro}.</p>

                    <p>
                        Use o código abaixo para confirmar seu
                        cadastro no CozinhaFreela:
                    </p>

                    <div style="font-size: 32px;
                                font-weight: bold;
                                letter-spacing: 8px;
                                margin: 24px 0;">
                        {codigo}
                    </div>

                    <p>
                        O código é válido por 10 minutos.
                    </p>

                    <p>
                        Se você não solicitou esse cadastro,
                        ignore esta mensagem.
                    </p>
                </div>
                """;

            await _emailService.EnviarAsync(
                usuario.Email,
                "Código de confirmação — CozinhaFreela",
                conteudoHtml
            );
        }
    }
}