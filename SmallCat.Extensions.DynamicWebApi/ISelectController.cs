using System.Reflection;
using SmallCat.Extensions.DynamicWebApi.Helpers;
using SmallCat.Extensions.DynamicWebApi.Attributes;

namespace SmallCat.Extensions.DynamicWebApi;

public interface ISelectController
{
    bool IsController(TypeInfo type);
}

internal class DefaultSelectController : ISelectController
{
    public bool IsController(TypeInfo type) => type is { IsPublic: true, IsAbstract: false, IsGenericType: false } && type.GetSingleAttributeOrDefaultByFullSearch<DynamicWebApiAttribute>() != null;
}