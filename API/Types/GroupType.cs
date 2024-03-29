using DAL.Models.Groups;

namespace API.Types;

public class GroupType : ObjectType<Group>
{
    protected override void Configure(IObjectTypeDescriptor<Group> descriptor)
    {
        descriptor.Field(g => g.Id).IsProjected();
        descriptor.Field(g => g.ImageExtension).IsProjected();
    }
}