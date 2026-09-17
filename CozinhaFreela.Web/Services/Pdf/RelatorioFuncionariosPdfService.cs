using CozinhaFreela.Web.ViewModels.Funcionarios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CozinhaFreela.Web.Services.Pdf
{
    public class RelatorioFuncionariosPdfService
        : IRelatorioFuncionariosPdfService
    {
        private const string VerdeProfundo = "#16251B";
        private const string Terracota = "#A86443";
        private const string Dourado = "#C59A5D";
        private const string Creme = "#F7F2E8";
        private const string CremeEscuro = "#EDE5D8";
        private const string Texto = "#282721";
        private const string TextoSuave = "#68665D";

        public byte[] Gerar(
            IReadOnlyCollection<
                FuncionarioRelatorioGeralViewModel
            > funcionarios)
        {
            ArgumentNullException.ThrowIfNull(funcionarios);

            return Document
                .Create(
                    documento =>
                    {
                        documento.Page(
                            pagina =>
                            {
                                pagina.Size(
                                    PageSizes.A4.Landscape()
                                );

                                pagina.Margin(28);
                                pagina.PageColor(Colors.White);

                                pagina.DefaultTextStyle(
                                    estilo =>
                                        estilo
                                            .FontSize(8)
                                            .FontColor(Texto)
                                );

                                pagina.Header()
                                    .Element(
                                        container =>
                                            CriarCabecalho(
                                                container,
                                                funcionarios.Count
                                            )
                                    );

                                pagina.Content()
                                    .PaddingVertical(18)
                                    .Element(
                                        container =>
                                            CriarTabela(
                                                container,
                                                funcionarios
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
            int totalFuncionarios)
        {
            container
                .BorderBottom(2)
                .BorderColor(Dourado)
                .PaddingBottom(13)
                .Row(
                    linha =>
                    {
                        linha.RelativeItem()
                            .Column(
                                coluna =>
                                {
                                    coluna.Item()
                                        .Text("COZINHAFREELA")
                                        .FontSize(15)
                                        .Bold()
                                        .FontColor(VerdeProfundo);

                                    coluna.Item()
                                        .PaddingTop(3)
                                        .Text("Relatório geral de funcionários")
                                        .FontSize(9)
                                        .FontColor(TextoSuave);
                                }
                            );

                        linha.AutoItem()
                            .AlignRight()
                            .Column(
                                coluna =>
                                {
                                    coluna.Item()
                                        .AlignRight()
                                        .Text(
                                            $"{totalFuncionarios} funcionário(s)"
                                        )
                                        .FontSize(10)
                                        .SemiBold()
                                        .FontColor(Terracota);

                                    coluna.Item()
                                        .AlignRight()
                                        .PaddingTop(3)
                                        .Text(
                                            $"Emitido em {DateTime.Now:dd/MM/yyyy}"
                                        )
                                        .FontSize(8)
                                        .FontColor(TextoSuave);
                                }
                            );
                    }
                );
        }

        private static void CriarTabela(
            IContainer container,
            IReadOnlyCollection<
                FuncionarioRelatorioGeralViewModel
            > funcionarios)
        {
            if (funcionarios.Count == 0)
            {
                container
                    .PaddingTop(50)
                    .AlignCenter()
                    .Text("Nenhum funcionário aprovado encontrado.")
                    .FontSize(11)
                    .FontColor(TextoSuave);

                return;
            }

            container.Table(
                tabela =>
                {
                    tabela.ColumnsDefinition(
                        colunas =>
                        {
                            colunas.RelativeColumn(2.1f);
                            colunas.RelativeColumn(1.2f);
                            colunas.RelativeColumn(1.15f);
                            colunas.RelativeColumn(1.05f);
                            colunas.RelativeColumn(1.25f);
                            colunas.RelativeColumn(1.25f);
                            colunas.RelativeColumn(1.8f);
                            colunas.RelativeColumn(0.8f);
                        }
                    );

                    tabela.Header(
                        cabecalho =>
                        {
                            CabecalhoCelula(cabecalho.Cell(), "Nome");
                            CabecalhoCelula(cabecalho.Cell(), "Função");
                            CabecalhoCelula(cabecalho.Cell(), "CPF");
                            CabecalhoCelula(cabecalho.Cell(), "Nascimento");
                            CabecalhoCelula(cabecalho.Cell(), "Telefone");
                            CabecalhoCelula(cabecalho.Cell(), "Cidade/UF");
                            CabecalhoCelula(cabecalho.Cell(), "Emergência");
                            CabecalhoCelula(cabecalho.Cell(), "Situação");
                        }
                    );

                    var indice = 0;

                    foreach (var funcionario in funcionarios)
                    {
                        var linhaAlternada = indice % 2 != 0;

                        CorpoCelula(
                            tabela.Cell(),
                            funcionario.NomeCompleto,
                            linhaAlternada
                        );

                        CorpoCelula(
                            tabela.Cell(),
                            funcionario.Funcao,
                            linhaAlternada
                        );

                        CorpoCelula(
                            tabela.Cell(),
                            FormatarCpf(funcionario.Cpf),
                            linhaAlternada
                        );

                        CorpoCelula(
                            tabela.Cell(),
                            funcionario.DataNascimento
                                .ToString("dd/MM/yyyy"),
                            linhaAlternada
                        );

                        CorpoCelula(
                            tabela.Cell(),
                            funcionario.Telefone,
                            linhaAlternada
                        );

                        CorpoCelula(
                            tabela.Cell(),
                            $"{funcionario.Cidade}/{funcionario.Estado}",
                            linhaAlternada
                        );

                        CorpoCelula(
                            tabela.Cell(),
                            $"{funcionario.ContatoEmergenciaNome}\n" +
                            funcionario.ContatoEmergenciaTelefone,
                            linhaAlternada
                        );

                        CorpoCelula(
                            tabela.Cell(),
                            funcionario.Ativo
                                ? "Ativo"
                                : "Inativo",
                            linhaAlternada
                        );

                        indice++;
                    }
                }
            );
        }

        private static void CabecalhoCelula(
            IContainer container,
            string texto)
        {
            container
                .Background(VerdeProfundo)
                .BorderRight(1)
                .BorderColor(Dourado)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .AlignMiddle()
                .Text(texto)
                .FontSize(7)
                .Bold()
                .FontColor(Colors.White);
        }

        private static void CorpoCelula(
            IContainer container,
            string? texto,
            bool linhaAlternada)
        {
            container
                .Background(
                    linhaAlternada
                        ? CremeEscuro
                        : Creme
                )
                .BorderBottom(1)
                .BorderRight(1)
                .BorderColor("#DDD3C3")
                .PaddingVertical(7)
                .PaddingHorizontal(6)
                .AlignMiddle()
                .Text(
                    string.IsNullOrWhiteSpace(texto)
                        ? "—"
                        : texto
                )
                .FontSize(7)
                .FontColor(Texto);
        }

        private static void CriarRodape(
            IContainer container)
        {
            container
                .BorderTop(1)
                .BorderColor("#E4DCCF")
                .PaddingTop(8)
                .Row(
                    linha =>
                    {
                        linha.RelativeItem()
                            .Text(
                                "Documento interno — contém dados pessoais"
                            )
                            .FontSize(7)
                            .FontColor(TextoSuave);

                        linha.AutoItem()
                            .DefaultTextStyle(
                                estilo =>
                                    estilo
                                        .FontSize(7)
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
    }
}
