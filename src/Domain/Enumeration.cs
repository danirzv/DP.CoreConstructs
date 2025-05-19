using System.Reflection;

namespace DP.CoreConstructs.Domain;

/// <summary>
/// Base-class of any Enumeration
/// based on microsoft suggested enumeration classes
/// https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types#implement-an-enumeration-base-class
/// </summary>
public abstract class Enumeration : IComparable
{
    public string Name { get; private set; }

    public int Id { get; private set; }

    protected Enumeration(int id, string name) => (Id, Name) = (id, name);

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T).GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);
        return absoluteDifference;
    }

    public static T FromValue<T>(int value) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(item => item.Id == value);

        if (matchingItem == null)
        {
            throw new ArgumentOutOfRangeException($"'{value}' is not valid in {typeof(T)}");
        }

        return matchingItem;
    }

    public static T FromName<T>(string name) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(item => item.Name == name);

        if (matchingItem == null)
        {
            throw new ArgumentOutOfRangeException($"'{name}' is not valid in {typeof(T)}");
        }

        return matchingItem;
    }

    public int CompareTo(Enumeration other) => Id.CompareTo(other.Id);

    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            throw new Exception("object in compare with Enumeration could not be null");
        }

        return CompareTo((Enumeration) obj);
    }
}
