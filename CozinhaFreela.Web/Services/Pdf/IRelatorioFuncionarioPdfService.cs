using CozinhaFreela.Web.ViewModels.Funcionarios;

namespace CozinhaFreela.Web.Services.Pdf
{
    public interface IRelatorioFuncionarioPdfService
    {
        byte[] Gerar(
            DetalhesFuncionarioViewModel funcionario);
    }
}
