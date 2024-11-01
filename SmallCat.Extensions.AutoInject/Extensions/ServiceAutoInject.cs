using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallCat.Extensions.AutoInject.Attributes;
using SmallCat.Extensions.AutoInject.Helpers;

namespace SmallCat.Extensions.AutoInject;

public static class ServiceAutoInjectExtensions
{
    private static readonly string? CurrentServiceName = MethodBase.GetCurrentMethod()?.ReflectedType?.Name;

    /// <summary>
    /// 自动注册标识的服务
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public static IServiceCollection AutoInject(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider(false);
        var logger          = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(string.Empty);
        logger.LogInformation("[{ServiceName}]: 开始注入服务到IOC容器中...", CurrentServiceName);
        var assemblies = AssemblyHelper.GetProjectAssemblies();

        var allTypes = assemblies.SelectMany(assembly => assembly
                                                         .GetTypes()
                                                         .Where(
                                                             type =>
                                                                 type is { IsClass: true, IsSealed: false, IsAbstract: false, IsPublic: true } &&
                                                                 type.IsDefined(typeof(AutoInjectAttribute), false)
                                                         ))
                                 .ToList();

        var index = 1;

        var length = allTypes.Max(t => t.FullName.Length);

        foreach (var type in allTypes)
        {
            var attribute      = type.GetCustomAttribute<AutoInjectAttribute>()!;
            var interfaceTypes = attribute.Services.Count == 0 ? [] : attribute.Services;
            Func<Type, Type, IServiceCollection> injectFunc = attribute.Life switch
            {
                ServiceLifetime.Scoped    => services.AddScoped,
                ServiceLifetime.Singleton => services.AddSingleton,
                ServiceLifetime.Transient => services.AddTransient,
                _                         => throw new ArgumentOutOfRangeException(),
            };

            interfaceTypes.ForEach(implementationType =>
            {
                injectFunc.Invoke(implementationType, type);

                logger.LogInformation("[{ServiceName}]: [{Index:000} - {Life:-9}] {Type}{Service} ", CurrentServiceName, index, attribute.Life.ToString().PadRight(9), type.FullName.PadRight(length), implementationType == type ? "" : $" -> {implementationType.FullName}");

                index++;
            });
        }

        logger.LogInformation("[{ServiceName}]: 本次自动扫描并且注入[{Count}]个服务。", CurrentServiceName, index);
        return services;
    }
}