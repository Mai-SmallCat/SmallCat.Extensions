using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SmallCat.Extensions.AutoInject.Attributes;

namespace SmallCat.Extensions.AutoInject.Helpers;

public static class InjectHelper
{
    public static List<(Type ImplementationType, Type? Interface, ServiceLifetime Lifetime)> GetAllNeedInjectTypes(
        Assembly assembly)
    {
        var result = new List<(Type ImplementationType, Type? Interface, ServiceLifetime Lifetime)>();

        var types = assembly.GetTypes().Where(
            type => type is
                {
                    IsClass   : true,
                    IsPublic  : true,
                    IsSealed  : false,
                    IsAbstract: false,
                } &&
                type.IsDefined(typeof(AutoInjectAttribute), false)
        ).ToList();

        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<AutoInjectAttribute>()!;

            if (attribute.Services.Count != 0)
            {
                attribute.Services.ForEach(implementationType =>
                {
                    result.Add(
                        new ValueTuple<Type, Type, ServiceLifetime>(type, implementationType, attribute.Life));
                });
            }
            else
            {
                result.Add(new ValueTuple<Type, Type?, ServiceLifetime>(type, null, attribute.Life));
            }
        }

        return result;
    }
}