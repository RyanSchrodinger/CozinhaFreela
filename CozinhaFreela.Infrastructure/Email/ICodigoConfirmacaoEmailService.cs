using CozinhaFreela.Domain.Usuarios;

namespace CozinhaFreela.Infrastructure.Email
{
    public interface ICodigoConfirmacaoEmailService
    {
        Task GerarEEnviarAsync(ApplicationUser usuario);
    }
}