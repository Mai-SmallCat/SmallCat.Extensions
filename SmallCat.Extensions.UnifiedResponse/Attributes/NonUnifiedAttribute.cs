﻿namespace SmallCat.Extensions.UnifiedResponse.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NonUnifiedAttribute : System.Attribute
{
    /// <summary>
    /// 该属性是否生效
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 不统一响应结果
    /// </summary>
    /// <param name="enable">是否生效，如果为True，则不序列化该响应，Method属性权重大于Class属性</param>
    public NonUnifiedAttribute(bool enable = true) => Enable = enable;
}