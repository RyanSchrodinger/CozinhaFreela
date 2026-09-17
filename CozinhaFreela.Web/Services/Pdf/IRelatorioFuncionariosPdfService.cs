using CozinhaFreela.Web.ViewModels.Funcionarios;

namespace CozinhaFreela.Web.Services.Pdf
{
    public interface IRelatorioFuncionariosPdfService
    {
        byte[] Gerar(
            IReadOnlyCollection<
                FuncionarioRelatorioGeralViewModel
            > funcionarios
        );
    }
}