using CozinhaFreela.Domain.Usuarios;
using CozinhaFreela.Infrastructure.Data;
using CozinhaFreela.Infrastructure.Email;
using CozinhaFreela.Web.Services.Pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection"
    )
    ?? throw new InvalidOperationException(
        "A conexão DefaultConnection não foi encontrada."
    );

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseSqlServer(
            connectionString,
            sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay:
                        TimeSpan.FromSeconds(10),
                    errorNumbersToAdd:
                        new[] { 40613 }
                );
            }
        );
    }
);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.User.RequireUniqueEmail = true;

            options.SignIn.RequireConfirmedEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.MaxFailedAccessAttempts = 5;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);
        }
    )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath = "/Conta/Login";

        options.AccessDeniedPath =
            "/Conta/AcessoNegado";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;
    }
);

builder.Services.Configure<ConfiguracaoEmail>(
    builder.Configuration.GetSection("Email")
);

builder.Services.AddTransient<
    IEmailService,
    SmtpEmailService>();

builder.Services.AddScoped<
    ICodigoConfirmacaoEmailService,
    CodigoConfirmacaoEmailService>();

builder.Services.AddScoped<
    IRelatorioFuncionarioPdfService,
    RelatorioFuncionarioPdfService>();

builder.Services.AddScoped<
    IRelatorioFuncionariosPdfService,
    RelatorioFuncionariosPdfService>();

QuestPDF.Settings.License =
    LicenseType.Community;

var app = builder.Build();

var loggerInicializacao = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("Inicializacao");

try
{
    using var scope = app.Services.CreateScope();

    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    await IdentitySeeder.SeedAsync(
        roleManager,
        userManager,
        builder.Configuration["ChefeInicial:Nome"],
        builder.Configuration["ChefeInicial:Email"],
        builder.Configuration["ChefeInicial:Senha"]
    );
}
catch (Exception exception)
{
    loggerInicializacao.LogError(
        exception,
        "Não foi possível executar o IdentitySeeder durante a inicialização."
    );
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern:
            "{controller=Home}/{action=Index}/{id?}"
    )
    .WithStaticAssets();

app.Run();  