using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.GroupSumUps;

public class GroupSumUpDocument : IDocument
{
    public GroupSumUpDocument(GroupSumUpModel model)
    {
        Model = model;
    }

    public GroupSumUpModel Model { get; }

    public DocumentMetadata GetMetadata()
    {
        return DocumentMetadata.Default;
    }

    public DocumentSettings GetSettings()
    {
        return DocumentSettings.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(descriptor =>
            {
                descriptor.Margin(50);

                descriptor.Header().Element(ComposeHeader);
                descriptor.Content().Element(ComposeContent);
                descriptor.Footer().Height(50).Background(Colors.Grey.Lighten1);
            });
    }

    private void ComposeContent(IContainer obj)
    {
        obj
            .PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);
                column.Item().Element(ComposeTable);
            });
    }

    private void ComposeTable(IContainer obj)
    {
        obj.Column(column =>
        {
            column.Item().Text(text => { text.Span("Expenses").Style(TextStyle.Default.FontSize(20).SemiBold()); });
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Name");
                    header.Cell().Element(CellStyle).Text("Price");
                    header.Cell().Element(CellStyle).Text("Date created");
                    header.Cell().AlignRight().Element(CellStyle).Text("Type");

                    static IContainer CellStyle(IContainer obj)
                    {
                        return obj.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1)
                            .BorderColor(Colors.Black);
                    }
                });

                foreach (var currentExpense in Model.Expenses)
                {
                    table.Cell().Element(CellStyle).Text(currentExpense.Name);
                    table.Cell().Element(CellStyle).Text(currentExpense.Amount.ToString("0.00"));
                    table.Cell().Element(CellStyle).Text(currentExpense.Date.ToString("dd.MM.yyyy"));
                    table.Cell().AlignRight().Element(CellStyle).Text(currentExpense.Type.ToString());

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
            column.Spacing(10);
            column.Item().Text(text =>
            {
                text.Span("User balances").Style(TextStyle.Default.FontSize(20).SemiBold());
            });
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Username");
                    header.Cell().Element(CellStyle).Text("Amount due");
                    header.Cell().Element(CellStyle).AlignRight().Text("To");

                    static IContainer CellStyle(IContainer obj)
                    {
                        return obj.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1)
                            .BorderColor(Colors.Black);
                    }
                });

                var payers = Model.Payers;
                foreach (var payer in payers)
                {
                    table.Cell().Element(CellStyle).Text(payer.UserName);
                    table.Cell().Element(CellStyle).Text(payer.AmountDue?.ToString("0.00") ?? "N/A");
                    table.Cell().Element(CellStyle).AlignRight().Text(payer.ToUserName ?? "N/A");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
        });
    }

    private void ComposeHeader(IContainer obj)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
        obj.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"\"{Model.GroupName}\" group").Style(titleStyle);
                column.Item().Text(text =>
                {
                    text.Span("Generated on ").Style(TextStyle.Default.FontSize(10));
                    text.Span($"{Model.CurrentDate:dd.MM.yyyy}").Style(TextStyle.Default.FontSize(10).SemiBold());
                });
                column.Item().Text(text =>
                {
                    text.Span("Proudly generated by ").Style(TextStyle.Default.FontSize(10));
                    text.Span("MoneyMinder")
                        .Style(TextStyle.Default.FontSize(10).SemiBold().FontColor(Colors.Blue.Medium));
                });
            });
            row.ConstantItem(100).Height(50).AlignRight().Element(ComposeLogo);
        });
    }

    private void ComposeLogo(IContainer obj)
    {
        obj.Image("wwwroot/favicon.ico").FitArea();
    }
}