using System.Text.Json;
using System.Text.Json.Serialization;
using DP.CoreConstructs.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace DP.CoreConstructs.Presentation.JsonConverters;

public class ValueObjectJsonConverter<T>(IServiceProvider serviceProvider, ValueObjectDescriptor<T> descriptor)
    : JsonConverter<T> where T : ValueObject
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject but found {reader.TokenType}.");
        }

        var arguments = new object[descriptor.PresentationMap.Count];
        var argumentIndex = 0;

        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var jsonObject = jsonDoc.RootElement;

            foreach (var key in descriptor.PresentationMap)
            {
                var propertyName = key.Key;
                var propertyType = key.Value;

                if (!jsonObject.TryGetProperty(propertyName, out var jsonProperty))
                {
                    throw new JsonException($"Missing property '{propertyName}' for type '{typeof(T).Name}'.");
                }

                object? value = propertyType switch
                {
                    var t when t == typeof(string) => jsonProperty.GetString(),
                    var t when t == typeof(int) => jsonProperty.GetInt32(),
                    var t when t == typeof(Guid) => jsonProperty.GetGuid(),
                    var t when t == typeof(bool) => jsonProperty.GetBoolean(),
                    _ => JsonSerializer.Deserialize(jsonProperty.GetRawText(), propertyType, options)
                };

                arguments[argumentIndex++] = value!;
            }
        }

        var factory = ActivatorUtilities.CreateFactory(typeof(T), descriptor.PresentationMap.Values.ToArray());
        var model = factory.Invoke(serviceProvider, arguments);

        return model as T ?? throw new JsonException($"Unable to create instance of type {typeof(T).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var property in descriptor.PresentationMap)
        {
            var propertyName = property.Key;
            var propertyInfo = typeof(T).GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new JsonException($"Property '{propertyName}' not found on type '{typeof(T).Name}'.");
            }

            var propertyValue = propertyInfo.GetValue(value);
            writer.WritePropertyName(propertyName);
            JsonSerializer.Serialize(writer, propertyValue, propertyInfo.PropertyType, options);
        }

        writer.WriteEndObject();
    }
}

public class ValueObjectDescriptor<T> where T : ValueObject
{
    public IReadOnlyDictionary<string, Type> PresentationMap { get; }
}