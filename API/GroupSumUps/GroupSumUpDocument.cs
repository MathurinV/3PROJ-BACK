using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.GroupSumUps;

public class GroupSumUpDocument : IDocument
{
    public GroupSumUpModel Model { get; }

    public GroupSumUpDocument(GroupSumUpModel model)
    {
        Model = model;
    }
    
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(descriptor =>
            {
                descriptor.Margin(50);

                descriptor.Header().Height(100).Background(Colors.Grey.Lighten1);
                descriptor.Content().Background(Colors.Grey.Lighten3);
                descriptor.Footer().Height(50).Background(Colors.Grey.Lighten1);
            });
    }
}