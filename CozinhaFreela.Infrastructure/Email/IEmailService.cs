namespace CozinhaFreela.Infrastructure.Email
{
    public interface IEmailService
    {
        Task EnviarAsync(
            string destinatario,
            string assunto,
            string conteudoHtml);
    }
}