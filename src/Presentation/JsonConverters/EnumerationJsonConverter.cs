using System.Text.Json;
using System.Text.Json.Serialization;
using DP.CoreConstructs.Domain;

namespace DP.CoreConstructs.Presentation.JsonConverters;

public class EnumerationJsonConverter<T> : JsonConverter<T> where T : Enumeration
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Enumeration.FromValue<T>(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Id);
    }
}
