using CozinhaFreela.Domain.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CozinhaFreela.Web.Services.Cadastro
{
    public class CadastroPendenteCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CadastroPendenteCleanupService> _logger;

        public CadastroPendenteCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<CadastroPendenteCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExcluirExpiradosAsync(stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Não foi possível excluir os cadastros não confirmados expirados."
                    );
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ExcluirExpiradosAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var agora = DateTime.UtcNow;

            var usuarios = await userManager.Users
                .Where(usuario =>
                    !usuario.EmailConfirmed &&
                    usuario.DataExpiracaoConfirmacao.HasValue &&
                    usuario.DataExpiracaoConfirmacao <= agora)
                .ToListAsync(cancellationToken);

            foreach (var usuario in usuarios)
            {
                var resultado = await userManager.DeleteAsync(usuario);

                if (!resultado.Succeeded)
                {
                    _logger.LogWarning(
                        "Não foi possível excluir o cadastro expirado {UsuarioId}.",
                        usuario.Id
                    );
                }
            }
        }
    }
}
