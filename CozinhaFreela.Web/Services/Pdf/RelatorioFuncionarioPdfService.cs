using CozinhaFreela.Web.ViewModels.Funcionarios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CozinhaFreela.Web.Services.Pdf
{
    public class RelatorioFuncionarioPdfService
        : IRelatorioFuncionarioPdfService
    {
        private const string VerdeProfundo = "#16251B";
        private const string Terracota = "#A86443";
        private const string Dourado = "#C59A5D";
        private const string Creme = "#F7F2E8";
        private const string Texto = "#282721";
        private const string TextoSuave = "#68665D";

        public byte[] Gerar(
            DetalhesFuncionarioViewModel funcionario)
        {
            ArgumentNullException.ThrowIfNull(funcionario);

            return Document
                .Create(
                    documento =>
                    {
                        documento.Page(
                            pagina =>
                            {
                                pagina.Size(PageSizes.A4);
                                pagina.Margin(35);
                                pagina.PageColor(Colors.White);

                                pagina.DefaultTextStyle(
                                    estilo =>
                                        estilo
                                            .FontSize(10)
                                            .FontColor(Texto)
                                );

                                pagina.Header()
                                    .Element(
                                        container =>
                                            CriarCabecalho(
                                                container,
                                                funcionario
                                            )
                                    );

                                pagina.Content()
                                    .PaddingVertical(24)
                                    .Element(
                                        container =>
                                            CriarConteudo(
                                                container,
                                                funcionario
                                            )
                                    );

                                pagina.Footer()
                                    .Element(CriarRodape);
                            }
                        );
                    }
                )
                .GeneratePdf();
        }

        private static void CriarCabecalho(
            IContainer container,
            DetalhesFuncionarioViewModel funcionario)
        {
            container
                .BorderBottom(2)
                .BorderColor(Dourado)
                .PaddingBottom(16)
                .Row(
                    linha =>
                    {
                        linha.RelativeItem()
                            .Column(
                                coluna =>
                                {
                                    coluna.Item()
                                        .Text("COZINHAFREELA")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(VerdeProfundo);

                                    coluna.Item()
                                        .PaddingTop(3)
                                        .Text("Ficha cadastral do funcionário")
                                        .FontSize(9)
                                        .FontColor(TextoSuave);
                                }
                            );

                        linha.ConstantItem(190)
                            .AlignRight()
                            .Column(
                                coluna =>
                                {
                                    coluna.Item()
                                        .AlignRight()
                                        .Text(funcionario.NomeCompleto)
                                        .FontSize(11)
                                        .SemiBold();

                                    coluna.Item()
                                        .AlignRight()
                                        .PaddingTop(3)
                                        .Text(funcionario.Funcao)
                                        .FontSize(9)
                                        .FontColor(Terracota);
                                }
                            );
                    }
                );
        }

        private static void CriarConteudo(
            IContainer container,
            DetalhesFuncionarioViewModel funcionario)
        {
            container.Column(
                coluna =>
                {
                    coluna.Spacing(18);

                    coluna.Item()
                        .Element(
                            item => CriarSecao(
                                item,
                                "Dados profissionais",
                                new[]
                                {
                                    ("Nome completo", funcionario.NomeCompleto),
                                    ("Função", funcionario.Funcao),
                                    ("E-mail", funcionario.Email),
                                    ("Telefone", funcionario.Telefone),
                                    (
                                        "E-mail confirmado",
                                        funcionario.EmailConfirmado
                                            ? "Sim"
                                            : "Não"
                                    ),
                                    (
                                        "Situação",
                                        funcionario.Ativo
                                            ? "Ativo"
                                            : "Inativo"
                                    )
                                }
                            )
                        );

                    coluna.Item()
                        .Element(
                            item => CriarSecao(
                                item,
                                "Dados pessoais",
                                new[]
                                {
                                    ("CPF", FormatarCpf(funcionario.Cpf)),
                                    (
                                        "Data de nascimento",
                                        funcionario.DataNascimento
                                            .ToString("dd/MM/yyyy")
                                    ),
                                    ("Idade", $"{funcionario.Idade} anos"),
                                    ("Estado civil", funcionario.EstadoCivil),
                                    ("Nacionalidade", funcionario.Nacionalidade)
                                }
                            )
                        );

                    coluna.Item()
                        .Element(
                            item => CriarSecao(
                                item,
                                "Endereço",
                                new[]
                                {
                                    ("CEP", FormatarCep(funcionario.Cep)),
                                    ("Rua", funcionario.Rua),
                                    ("Número", funcionario.Numero),
                                    (
                                        "Complemento",
                                        ValorOuNaoInformado(
                                            funcionario.Complemento
                                        )
                                    ),
                                    ("Bairro", funcionario.Bairro),
                                    ("Cidade", funcionario.Cidade),
                                    ("Estado", funcionario.Estado)
                                }
                            )
                        );

                    coluna.Item()
                        .Element(
                            item => CriarSecao(
                                item,
                                "Contato de emergência",
                                new[]
                                {
                                    (
                                        "Nome",
                                        funcionario.ContatoEmergenciaNome
                                    ),
                                    (
                                        "Telefone",
                                        funcionario.ContatoEmergenciaTelefone
                                    )
                                }
                            )
                        );

                    coluna.Item()
                        .Element(
                            item => CriarSecao(
                                item,
                                "Observações",
                                new[]
                                {
                                    (
                                        "Informações adicionais",
                                        ValorOuNaoInformado(
                                            funcionario.Observacoes
                                        )
                                    )
                                }
                            )
                        );
                }
            );
        }

        private static void CriarSecao(
            IContainer container,
            string titulo,
            IEnumerable<(string Rotulo, string Valor)> campos)
        {
            container
                .Border(1)
                .BorderColor("#E4DCCF")
                .Column(
                    coluna =>
                    {
                        coluna.Item()
                            .Background(VerdeProfundo)
                            .PaddingVertical(8)
                            .PaddingHorizontal(12)
                            .Text(titulo)
                            .FontSize(11)
                            .SemiBold()
                            .FontColor(Colors.White);

                        coluna.Item()
                            .Background(Creme)
                            .Padding(12)
                            .Table(
                                tabela =>
                                {
                                    tabela.ColumnsDefinition(
                                        colunas =>
                                        {
                                            colunas.RelativeColumn();
                                            colunas.RelativeColumn();
                                        }
                                    );

                                    foreach (var campo in campos)
                                    {
                                        tabela.Cell()
                                            .Padding(6)
                                            .Column(
                                                celula =>
                                                {
                                                    celula.Item()
                                                        .Text(campo.Rotulo)
                                                        .FontSize(7)
                                                        .Bold()
                                                        .FontColor(Terracota);

                                                    celula.Item()
                                                        .PaddingTop(3)
                                                        .Text(campo.Valor)
                                                        .FontSize(9)
                                                        .FontColor(Texto);
                                                }
                                            );
                                    }
                                }
                            );
                    }
                );
        }

        private static void CriarRodape(
            IContainer container)
        {
            container
                .BorderTop(1)
                .BorderColor("#E4DCCF")
                .PaddingTop(10)
                .Row(
                    linha =>
                    {
                        linha.RelativeItem()
                            .Text(
                                $"Documento gerado em " +
                                $"{DateTime.Now:dd/MM/yyyy 'às' HH:mm}"
                            )
                            .FontSize(8)
                            .FontColor(TextoSuave);

                        linha.AutoItem()
                            .DefaultTextStyle(
                                estilo =>
                                    estilo
                                        .FontSize(8)
                                        .FontColor(TextoSuave)
                            )
                            .Text(
                                texto =>
                                {
                                    texto.Span("Página ");
                                    texto.CurrentPageNumber();
                                    texto.Span(" de ");
                                    texto.TotalPages();
                                }
                            );
                    }
                );
        }

        private static string FormatarCpf(
            string cpf)
        {
            return cpf.Length == 11
                ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}"
                : cpf;
        }

        private static string FormatarCep(
            string cep)
        {
            return cep.Length == 8
                ? $"{cep[..5]}-{cep[5..]}"
                : cep;
        }

        private static string ValorOuNaoInformado(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? "Não informado"
                : valor;
        }
    }
}
