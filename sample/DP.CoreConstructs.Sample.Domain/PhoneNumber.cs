using DP.CoreConstructs.Domain;

namespace DP.CoreConstructs.Sample.Domain;

public class PhoneNumber(string val) : ValueObject
{
    public string Value { get; } = val;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
