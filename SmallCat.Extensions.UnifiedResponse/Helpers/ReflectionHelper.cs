using System.Reflection;

namespace SmallCat.Extensions.UnifiedResponse.Helpers;

internal static class ReflectionHelper
{
    # region 通用反射方法
    /// <summary>
    /// 转载于  https://blog.csdn.net/WPwalter/article/details/82859267
    /// <para></para>
    /// 判断指定的类型 <paramref name="type"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
    /// </summary>
    /// <param name="type">需要测试的类型。</param>
    /// <param name="generic">泛型接口类型，传入 typeof(IXxx&lt;&gt;)</param>
    /// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
    internal static bool HasImplementedRawGeneric(this Type type, Type generic)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(generic);

        // 测试接口。
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
        if (isTheRawGenericType) return true;

        // 测试类型。
        while (type != null && type != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(type);
            if (isTheRawGenericType) return true;
            type = type.BaseType;
        }

        // 没有找到任何匹配的接口或类型。
        return false;

        // 测试某个类型是否是指定的原始接口。
        bool IsTheRawGenericType(Type test)
            => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
    }

    /// <summary>
    /// 获取 MemberInfo 的第一个属性 / 空
    /// </summary>
    /// <typeparam name="TAttribute">Type of the attribute</typeparam>
    /// <param name="memberInfo">The member that will be checked for the attribute</param>
    /// <param name="inherit">Include inherited attributes</param>
    /// <returns>Returns the attribute object if found. Returns null if not found.</returns>
    public static TAttribute? GetSingleAttributeOrNull<TAttribute>(this MemberInfo memberInfo, bool inherit = true)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(memberInfo);

        var attrs = memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).ToArray();
        if (attrs.Length > 0)
        {
            return (TAttribute)attrs[0];
        }

        return default;
    }

    #endregion
}