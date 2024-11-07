using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmallCat.Extensions.DynamicWebApi.Helpers;
using SmallCat.Extensions.DynamicWebApi.Attributes;

namespace SmallCat.Extensions.DynamicWebApi;

public class DynamicWebApiConvention : IApplicationModelConvention
{
    private readonly IActionRouteFactory _actionRouteFactory;
    private readonly ISelectController   _selectController;

    public DynamicWebApiConvention(IActionRouteFactory actionRouteFactory, ISelectController selectController)
    {
        _selectController   = selectController;
        _actionRouteFactory = actionRouteFactory;
    }

    /// <summary>
    /// 只判断有DynamicWebApi标记的Controller
    /// </summary>
    /// <param name="application"></param>
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (!_selectController.IsController(controller.ControllerType.GetTypeInfo()))
            {
                continue;
            }

            var attr = controller.ControllerType.GetTypeInfo().GetSingleAttributeOrDefaultByFullSearch<DynamicWebApiAttribute>();
            controller.ControllerName = controller.ControllerName.RemovePostFix(AppConsts.ControllerPostfixes.ToArray());
            ConfigureArea(controller, attr);
            ConfigureDynamicWebApi(controller, attr);
        }
    }

    /// <summary>
    /// 配置区域
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="attr"></param>
    /// <exception cref="ArgumentException"></exception>
    private static void ConfigureArea(ControllerModel controller, DynamicWebApiAttribute? attr)
    {
        if (!controller.RouteValues.ContainsKey("area"))
        {
            if (!string.IsNullOrEmpty(attr?.Module))
            {
                controller.RouteValues["area"] = attr.Module;
            }
            else if (!string.IsNullOrEmpty(AppConsts.DefaultAreaName))
            {
                controller.RouteValues["area"] = AppConsts.DefaultAreaName;
            }
        }
    }

    /// <summary>
    /// 配置DynamicWebApi
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="controllerAttr"></param>
    private void ConfigureDynamicWebApi(ControllerModel controller, DynamicWebApiAttribute? controllerAttr)
    {
        var actions = controller.Actions.Where(CheckMapMethod).ToList();
        // 配置 Controller
        ConfigureApiExplorer(controller);
        // 区域
        var areaName = controllerAttr != null ? controllerAttr.Module : AppConsts.DefaultAreaName;

        // 给有Router属性的控制器添加HttpMethod
        var hasRouter = controller.Selectors.Any(selector => selector.AttributeRouteModel != null);

        // 配置 Action
        foreach (var action in actions)
        {
            // 配置接口的请求方式
            AddHttpMethodAttribute(action);

            // 配置接口参数的请求方式
            ConfigureParameters(action);

            if (!hasRouter)
            {
                var appServiceSelectorModel = action.Selectors[0];
                appServiceSelectorModel.AttributeRouteModel ??= CreateActionRouteModel(areaName, controller.ControllerName, action);
            }

            if (AppConsts.ConfigurationApiResult != null)
            {
                // 配置Api返回结果
                ConfigurationApiResult(action, controller.ControllerType);
            }
        }
    }

    /// <summary>
    /// 配置Swagger文档里面的是否跳过全局序列化 (需要抽象）
    /// </summary>
    /// <param name="action"></param>
    /// <param name="controllerType"></param>
    private static void ConfigurationApiResult(ActionModel action, TypeInfo controllerType)
    {
        var realType = action.ActionMethod.GetRealType();

        var packResult = AppConsts.ConfigurationApiResult?.Invoke(controllerType, action, realType) ?? false;

        if (!packResult || AppConsts.UnifiedResultType == null) return;

        var restFulResultType = AppConsts.UnifiedResultType.MakeGenericType(realType);
        action.Filters.Add(new ProducesResponseTypeAttribute(restFulResultType, StatusCodes.Status200OK));
    }


    /// <summary>
    /// 配置参数
    /// </summary>
    /// <param name="action"></param>
    private void ConfigureParameters(ActionModel action)
    {
        foreach (var para in action.Parameters)
        {
            if (para.BindingInfo != null)
            {
                continue;
            }

            if (!TypeHelper.IsPrimitiveExtendedIncludingNullable(para.ParameterInfo.ParameterType))
            {
                if (CanUseFormBodyBinding(action, para))
                {
                    para.BindingInfo =
                        BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
                }
            }
        }
    }

    /// <summary>
    /// FormBody 绑定
    /// </summary>
    /// <param name="action"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    private bool CanUseFormBodyBinding(ActionModel action, ParameterModel parameter)
    {
        return !AppConsts.FormBodyBindingIgnoredTypes.Any(
            t => t.IsAssignableFrom(parameter.ParameterInfo.ParameterType)) && action.Selectors.All(selector =>
            !selector.ActionConstraints
                     .OfType<HttpMethodActionConstraint?>()
                     .Any(httpMethodActionConstraint => httpMethodActionConstraint != null && httpMethodActionConstraint.HttpMethods.All(hm => hm.IsIn("GET", "DELETE", "TRACE", "HEAD"))));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controller"></param>
    private static void ConfigureApiExplorer(ControllerModel controller)
    {
        foreach (var item in AppConsts.ControllerPostfixes.Where(item =>
            controller.ControllerName.ToLower().EndsWith(item.ToLower())))
        {
            var length = controller.ControllerName.Length - item.Length;
            if (length != 0)
            {
                controller.ControllerName = controller.ControllerName[..length];
            }

            break;
        }

        if (controller.ApiExplorer.GroupName != null && controller.ApiExplorer.GroupName.IsNullOrEmpty())
        {
            controller.ApiExplorer.GroupName = controller.ControllerName;
        }

        if (controller.ApiExplorer.IsVisible == null)
        {
            controller.ApiExplorer.IsVisible = true;
        }
    }


    /// <summary>
    /// 添加路由方式
    /// </summary>
    /// <param name="action"></param>
    /// <exception cref="Exception"></exception>
    private void AddHttpMethodAttribute(ActionModel action)
    {
        if (!action.Selectors.IsNullOrEmpty() && !action.Selectors.All(a => a.ActionConstraints.IsNullOrEmpty()))
        {
            return;
        }

        var verb = GetHttpVerb(action);

        action.ActionName = GetRestFulActionName(action.ActionName);

        var appServiceSelectorModel = action.Selectors[0];

        if (!appServiceSelectorModel.ActionConstraints.Any())
        {
            appServiceSelectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { verb }));
            switch (verb)
            {
                case "GET":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpGetAttribute());
                    break;
                case "POST":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpPostAttribute());
                    break;
                case "PUT":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpPutAttribute());
                    break;
                case "DELETE":
                    appServiceSelectorModel.EndpointMetadata.Add(new HttpDeleteAttribute());
                    break;
                default:
                    throw new Exception($"Unsupported http verb: {verb}.");
            }
        }
    }


    /// <summary>
    /// Processing action name
    /// </summary>
    /// <param name="actionName"></param>
    /// <returns></returns>
    private static string GetRestFulActionName(string actionName)
    {
        // custom process action name
        var appConstsActionName = AppConsts.GetRestFulActionName?.Invoke(actionName);
        if (appConstsActionName != null)
        {
            return appConstsActionName;
        }

        // Remove Postfix
        actionName = actionName.RemovePostFix(AppConsts.ActionPostfixes.ToArray());

        // Remove Prefix
        var verbKey = actionName.GetPascalOrCamelCaseFirstWord().ToLower();
        if (AppConsts.HttpVerbs.ContainsKey(verbKey))
        {
            if (actionName.Length == verbKey.Length)
            {
                return "";
            }
            else
            {
                return actionName.Substring(verbKey.Length);
            }
        }
        else
        {
            return actionName;
        }
    }

    private void NormalizeSelectorRoutes(string areaName, string controllerName, ActionModel action)
    {
        action.ActionName = GetRestFulActionName(action.ActionName);

        Console.WriteLine(action.ActionName);
        foreach (var selector in action.Selectors)
        {
            selector.AttributeRouteModel = selector.AttributeRouteModel == null
                ? CreateActionRouteModel(areaName, controllerName, action)
                : AttributeRouteModel.CombineAttributeRouteModel(null, selector.AttributeRouteModel);
        }
    }

    private static string GetHttpVerb(ActionModel action)
    {
        var getValueSuccess = AppConsts.AssemblyDynamicWebApiOptions
                                       .TryGetValue(action.Controller.ControllerType.Assembly,
                                           out AssemblyDynamicWebApiOptions assemblyDynamicWebApiOptions);
        if (getValueSuccess && !string.IsNullOrWhiteSpace(assemblyDynamicWebApiOptions?.HttpVerb))
        {
            return assemblyDynamicWebApiOptions.HttpVerb;
        }


        var verbKey = action.ActionName.GetPascalOrCamelCaseFirstWord().ToLower();

        var verb = AppConsts.HttpVerbs.ContainsKey(verbKey)
            ? AppConsts.HttpVerbs[verbKey]
            : AppConsts.DefaultHttpVerb;
        return verb;
    }

    /// <summary>
    /// 检查Action是否是Api接口
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private bool CheckMapMethod(ActionModel action)
    {
        var nonAction = action.ActionMethod.GetSingleAttributeOrDefault<NonActionAttribute>();
        var isVoid    = action.ActionMethod.ReturnType == typeof(void) || action.ActionMethod.ReturnType == typeof(Task);
        var isMap     = nonAction                      == null && !isVoid;
        action.ApiExplorer.IsVisible = isMap;
        return isMap;
    }

    /// <summary>
    /// 创建路由路径
    /// </summary>
    /// <param name="areaName"></param>
    /// <param name="controllerName"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    private AttributeRouteModel CreateActionRouteModel(string areaName, string controllerName, ActionModel action)
    {
        action.ActionName = GetRestFulActionName(action.ActionName);

        var route = _actionRouteFactory.CreateActionRouteModel(areaName, controllerName, action);

        return new AttributeRouteModel(new RouteAttribute(route));
    }
}