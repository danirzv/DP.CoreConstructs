using System.Text.Json;
using DP.CoreConstructs.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DP.CoreConstructs.Infrastructure;

public static class PropertyBuilderExtensions
{
    public static JsonSerializerOptions JsonSerializerOptions = JsonSerializerOptions.Default;

    public static PropertyBuilder<T> HasEnumerationConvertion<T>(this PropertyBuilder<T> builder) where T : Enumeration
    {
        return builder.HasConversion(
            to => to.Id,
            from => Enumeration.FromValue<T>(from));
    }

    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder)
    {
        return builder.HasConversion(
            to => JsonSerializer.Serialize(to, typeof(TProperty), JsonSerializerOptions),
            from => JsonSerializer.Deserialize<TProperty>(from, JsonSerializerOptions));
    }

    public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder, TProperty defaultValue)
    {
        return builder.HasConversion(
            to => JsonSerializer.Serialize(to, typeof(TProperty), JsonSerializerOptions),
            from => string.IsNullOrEmpty(from)
                ? defaultValue
                : JsonSerializer.Deserialize<TProperty>(from, JsonSerializerOptions) ?? defaultValue);
    }
}