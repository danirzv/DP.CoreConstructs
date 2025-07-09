using DP.CoreConstructs.Presentation.JsonConverters;
using DP.CoreConstructs.Sample.Domain;

namespace DP.CoreConstructs.Sample.Presentation.Descriptors;

public class PageDescriptor : ValueObjectDescriptor<Page>
{
    public override IReadOnlyDictionary<string, Type> PresentationMap => new Dictionary<string, Type>
    {
        ["Size"] = typeof(int),
        ["Number"] = typeof(int),
    };
}
