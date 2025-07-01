using DP.CoreConstructs.Domain;
using NJsonSchema;
using NJsonSchema.Generation;

namespace DP.CoreConstructs.Presentation.NSwag.SchemaProcessors;

public class EnumerationSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType.BaseType != null &&
            context.ContextualType.BaseType == typeof(Enumeration))
        {
            context.Schema.Type = JsonObjectType.Integer;
            context.Schema.AnyOf.Clear();
            context.Schema.AllOf.Clear();
        }
    }
}
