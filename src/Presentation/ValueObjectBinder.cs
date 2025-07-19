using DP.CoreConstructs.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Namotion.Reflection;

namespace DP.CoreConstructs.Presentation;

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
