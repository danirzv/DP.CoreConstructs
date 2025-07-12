using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DP.CoreConstructs.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Namotion.Reflection;

namespace DP.CoreConstructs.Presentation.JsonConverters;

public class ValueObjectJsonConverter<T>(IServiceProvider serviceProvider)
    : JsonConverter<T> where T : ValueObject
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var descriptor = serviceProvider.GetRequiredService<ValueObjectDescriptor<T>>();

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
                    var t when t == typeof(short) => jsonProperty.GetInt16(),
                    var t when t == typeof(long) => jsonProperty.GetInt64(),
                    var t when t == typeof(decimal) => jsonProperty.GetDecimal(),
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

        var descriptor = serviceProvider.GetRequiredService<ValueObjectDescriptor<T>>();

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

public abstract class ValueObjectDescriptor;

public abstract class ValueObjectDescriptor<T> : ValueObjectDescriptor where T : ValueObject
{
    public abstract IReadOnlyDictionary<string, Type> PresentationMap { get; }
}

public static class CoreConstructsExtentions
{
    public static IServiceCollection ScanValueObjectsOfAssembly<T>(this IServiceCollection services)
    {
        var valueObjectDescriptors = typeof(T).Assembly.DefinedTypes
            .Where(type => type.BaseType is {IsGenericType: true} && type.BaseType.GetGenericTypeDefinition() == typeof(ValueObjectDescriptor<>));

        services.AddSingleton<BaBinder>();

        services.RegisterValueObjectDescriptors(valueObjectDescriptors);

        services.AddSingleton<IConfigureOptions<JsonOptions>, JsonOptionsConfigurator>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, MvcOptionsConfigurator>();

        return services;
    }

    private static IServiceCollection RegisterValueObjectDescriptors(this IServiceCollection services, IEnumerable<TypeInfo> descriptors)
    {
        foreach (var descriptor in descriptors)
        {
            var valueObjectType = descriptor.BaseType!.GenericTypeArguments[0];
            var standsFor = typeof(ValueObjectDescriptor<>).MakeGenericType(valueObjectType);
            services.TryAdd(new ServiceDescriptor(standsFor, descriptor, ServiceLifetime.Singleton));

            services.Add(new ServiceDescriptor(typeof(ValueObjectDescriptor), descriptor, ServiceLifetime.Singleton));
        }

        return services;
    }
}

public class MvcOptionsConfigurator
    : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new ValueObjectBinderProvider());
    }
}

public class JsonOptionsConfigurator(IEnumerable<ValueObjectDescriptor> descriptors, IServiceProvider sp)
    : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        foreach (var descriptor in descriptors)
        {
            var valueObjectType = descriptor.GetType().BaseType!.GenericTypeArguments[0];
            var specificJsonConverterType = typeof(ValueObjectJsonConverter<>).MakeGenericType(valueObjectType);

            var converterInstance = Activator.CreateInstance(specificJsonConverterType, sp) as JsonConverter;

            options.JsonSerializerOptions.Converters.Add(converterInstance);
        }
    }
}

public class ValueObjectBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (!context.Metadata.ModelType.InheritsFromTypeName(typeof(ValueObject).FullName, TypeNameStyle.FullName))
        {
            return null;
        }

        var type = typeof(ValueObjectBinder<>).MakeGenericType(context.Metadata.ModelType);

        var binder = Activator.CreateInstance(type) as IModelBinder;

        return binder;
    }
}

public class ValueObjectBinder<TValueObject> : IModelBinder where TValueObject : ValueObject
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var baBinder = bindingContext.HttpContext.RequestServices.GetService<BaBinder>()!;
        var descriptor = bindingContext.HttpContext.RequestServices.GetService<ValueObjectDescriptor<TValueObject>>()!;

        baBinder.GetModel<TValueObject>(bindingContext, descriptor.PresentationMap);
        return Task.CompletedTask;
    }
}

public class BaBinder
{
    private readonly IServiceProvider _serviceProvider;

    public BaBinder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void GetModel<T>(ModelBindingContext bindingContext, IReadOnlyDictionary<string, Type> parameters)
    {
        var values = new List<object>(parameters.Count);
        foreach (var parameter in parameters)
        {
            var value = TryGet(bindingContext, parameter.Key)!.FirstOrDefault();

            if (parameter.Value == typeof(string))
            {
                values.Add(value);
            }
            else if (parameter.Value == typeof(int))
            {
                int.TryParse(value, out var prop);
                values.Add(prop);
            }
            else if (parameter.Value == typeof(Guid))
            {
                Guid.TryParse(value, out var prop);
                values.Add(prop);
            }
        }

        var factory = ActivatorUtilities.CreateFactory(typeof(T), parameters.Select(s => s.Value).ToArray());

        var model = factory.Invoke(_serviceProvider, values.ToArray());

        bindingContext.Result = ModelBindingResult.Success(model);

        static string?[]? TryGet(ModelBindingContext bindingContext, string key)
        {
            string[] result = [];
            if (String.IsNullOrEmpty(key))
                return result;

            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + key);

            if (valueResult == ValueProviderResult.None)
            {
                return result;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

            result = valueResult.Values;

            return result;
        }
    }
}