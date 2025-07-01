using DP.CoreConstructs.Domain;
using Microsoft.OpenApi.Models;
using NJsonSchema;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DP.CoreConstructs.Presentation.Swashbuckle.SchemaFilters;

public class StringValueObjectSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.BaseType != null &&
            context.Type.BaseType == typeof(ValueObject) &&
            context.Type.GetProperties().Length == 1 &&
            context.Type.GetProperties()[0].PropertyType == typeof(string))
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Properties?.Clear();
            schema.Reference = null;
            schema.AnyOf.Clear();
            schema.AllOf.Clear();
        }
    }
}
