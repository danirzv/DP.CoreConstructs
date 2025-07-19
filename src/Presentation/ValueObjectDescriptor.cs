using DP.CoreConstructs.Domain;

namespace DP.CoreConstructs.Presentation;

public abstract class ValueObjectDescriptor;

public abstract class ValueObjectDescriptor<T> : ValueObjectDescriptor where T : ValueObject
{
    public abstract IReadOnlyDictionary<string, Type> PresentationMap { get; }
}