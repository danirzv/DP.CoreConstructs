using DP.CoreConstructs.Domain;
using NJsonSchema;
using NJsonSchema.Generation;

namespace DP.CoreConstructs.Presentation.NSwag.SchemaProcessors;

public class StringValueObjectSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType.BaseType != null &&
            context.ContextualType.BaseType == typeof(ValueObject) &&
            context.ContextualType.Properties.Length == 1 &&
            context.ContextualType.Properties[0].PropertyType == typeof(string))
        {
            context.Schema.Type = JsonObjectType.String;
            context.Schema.AnyOf.Clear();
            context.Schema.AllOf.Clear();
        }
    }
}
