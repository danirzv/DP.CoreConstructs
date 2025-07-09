using DP.CoreConstructs.Domain;

namespace DP.CoreConstructs.Sample.Domain;

public class Page : ValueObject
{
    public Page(int size, int number)
    {
        Size = size;
        Number = number;
    }

    public int Size { get; }
    public int Number { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Size;
        yield return Number;
    }
}
