using DP.CoreConstructs.Presentation;
using DP.CoreConstructs.Sample.Domain;

namespace DP.CoreConstructs.Sample.Presentation.Descriptors;

public class PhoneNumberDescriptor : ValueObjectDescriptor<PhoneNumber>
{
    public override IReadOnlyDictionary<string, Type> PresentationMap => new Dictionary<string, Type>
    {
        ["Value"] = typeof(string),
    };
}
