using Microsoft.Extensions.DependencyInjection;

namespace SmallCat.Extensions.AutoInject.Attributes;

/// <summary>
/// AutoInject Service
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoInjectAttribute : Attribute
{
    /// <summary>
    /// 生命周期
    /// </summary>
    public ServiceLifetime Life { get; }

    /// <summary>
    /// 实现接口
    /// </summary>
    public List<Type> Services { get; } = new();

    /// <summary>
    /// AutoInject , Custom definition 
    /// </summary>
    /// <param name="life"></param>
    /// <param name="implementationInterface"></param>
    public AutoInjectAttribute(ServiceLifetime life = ServiceLifetime.Scoped, params Type[] implementationInterface)
    {
        Life = life;
        Services.AddRange(implementationInterface);
    }

    public AutoInjectAttribute(ServiceLifetime serviceLifetime, Type implementationInterface)
    {
        Life = serviceLifetime;
        Services.Add(implementationInterface);
    }

    /// <summary>
    ///  AutoInject, default Scoped
    /// </summary>
    /// <param name="implementationInterfaces"></param>
    public AutoInjectAttribute(params Type[] implementationInterfaces)
    {
        Life = ServiceLifetime.Scoped;
        Services.AddRange(implementationInterfaces);
    }
}