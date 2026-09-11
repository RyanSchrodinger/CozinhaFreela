using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CozinhaFreela.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly ConfiguracaoEmail _configuracao;

        public SmtpEmailService(
            IOptions<ConfiguracaoEmail> configuracao)
        {
            _configuracao = configuracao.Value;
        }

        public async Task EnviarAsync(
            string destinatario,
            string assunto,
            string conteudoHtml)
        {
            ValidarConfiguracao();

            var mensagem = new MimeMessage();

            mensagem.From.Add(
                new MailboxAddress(
                    _configuracao.NomeRemetente,
                    _configuracao.EmailRemetente
                )
            );

            mensagem.To.Add(
                MailboxAddress.Parse(destinatario)
            );

            mensagem.Subject = assunto;

            mensagem.Body = new BodyBuilder
            {
                HtmlBody = conteudoHtml
            }.ToMessageBody();

            using var cliente = new SmtpClient();

            await cliente.ConnectAsync(
                _configuracao.Servidor,
                _configuracao.Porta,
                SecureSocketOptions.StartTls
            );

            await cliente.AuthenticateAsync(
                _configuracao.Usuario,
                _configuracao.Senha
            );

            await cliente.SendAsync(mensagem);
            await cliente.DisconnectAsync(true);
        }

        private void ValidarConfiguracao()
        {
            if (string.IsNullOrWhiteSpace(
                    _configuracao.Servidor) ||
                _configuracao.Porta <= 0 ||
                string.IsNullOrWhiteSpace(
                    _configuracao.Usuario) ||
                string.IsNullOrWhiteSpace(
                    _configuracao.Senha) ||
                string.IsNullOrWhiteSpace(
                    _configuracao.EmailRemetente))
            {
                throw new InvalidOperationException(
                    "As configurações de e-mail estão incompletas."
                );
            }
        }
    }
}