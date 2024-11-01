using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;


namespace SmallCat.Extensions.DynamicWebApi;

public static class AppConsts
{
    public static string                                             DefaultHttpVerb              { get; set; }
    public static string                                             DefaultAreaName              { get; set; }
    public static string                                             DefaultApiPreFix             { get; set; }
    public static List<string>                                       ControllerPostfixes          { get; set; }
    public static List<string>                                       ActionPostfixes              { get; set; }
    public static List<Type>                                         FormBodyBindingIgnoredTypes  { get; set; }
    public static Dictionary<string, string>                         HttpVerbs                    { get; set; }
    public static Func<string, string>                               GetRestFulActionName         { get; set; }
    public static Dictionary<Assembly, AssemblyDynamicWebApiOptions> AssemblyDynamicWebApiOptions { get; set; }
    public static Func<TypeInfo, ActionModel, Type, bool>?           ConfigurationApiResult       { get; set; } = null;
    public static Type?                                              UnifiedResultType            { get; set; } = null;

    static AppConsts()
    {
        HttpVerbs = new Dictionary<string, string>
        {
            ["add"]         = "POST",
            ["create"]      = "POST",
            ["post"]        = "POST",
            ["addOrUpdate"] = "POST",
            ["get"]         = "GET",
            ["find"]        = "GET",
            ["fetch"]       = "GET",
            ["query"]       = "GET",
            ["update"]      = "PUT",
            ["put"]         = "PUT",
            ["delete"]      = "DELETE",
            ["remove"]      = "DELETE",
        };
    }
}