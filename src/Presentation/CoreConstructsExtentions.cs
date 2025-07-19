using System.Reflection;
using System.Text.Json.Serialization;
using DP.CoreConstructs.Presentation.JsonConverters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DP.CoreConstructs.Presentation;

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
    
    private class MvcOptionsConfigurator
        : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, new ValueObjectBinderProvider());
        }
    }

    private class JsonOptionsConfigurator(IEnumerable<ValueObjectDescriptor> descriptors, IServiceProvider sp)
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

}
