using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DP.CoreConstructs.Presentation.Swashbuckle.SchemaFilters;

public class JsonSchemaFilter : ISchemaFilter
{
    private static readonly HashSet<Type> SchemaTypes =
    [
        typeof(JsonDocument),
        typeof(JsonObject),
        typeof(JsonNode),
        typeof(JsonElement)
    ];

    private static readonly OpenApiObject JsonSchemaExample = new()
    {
        ["comment"] = new OpenApiString("This object could contain any property with any type"),
        ["foo"] = new OpenApiInteger(1),
        ["bar"] = new OpenApiObject()
    };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (SchemaTypes.Contains(context.Type))
        {
            schema.Example = JsonSchemaExample;
        }
    }
}