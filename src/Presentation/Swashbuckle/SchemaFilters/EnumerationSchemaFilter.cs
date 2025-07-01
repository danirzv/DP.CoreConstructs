using DP.CoreConstructs.Domain;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DP.CoreConstructs.Presentation.Swashbuckle.SchemaFilters;

public class EnumerationSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != null &&
            context.Type.BaseType == typeof(Enumeration))
        {
            schema.Type = "integer";
            schema.Properties?.Clear();
            schema.Reference = null;
            schema.AllOf?.Clear();
            schema.AnyOf?.Clear();        }
    }
}
