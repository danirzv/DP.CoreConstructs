using System.Text.Json;
using System.Text.Json.Nodes;
using NJsonSchema.Generation;

namespace DP.CoreConstructs.Presentation.NSwag.SchemaProcessors;

public class JsonSchemaProcessor : ISchemaProcessor
{
    private static readonly HashSet<Type> SchemaTypes =
    [
        typeof(JsonDocument),
        typeof(JsonObject),
        typeof(JsonNode),
        typeof(JsonElement)
    ];

    private static readonly object JsonSchemaExample = new
    {
        comment = "This object could contain any property with any type",
        foo = 1,
        bar = new { }
    };

    public void Process(SchemaProcessorContext context)
    {
        if (SchemaTypes.Contains(context.ContextualType))
        {
            context.Schema.Example = JsonSchemaExample;
        }
    }
}