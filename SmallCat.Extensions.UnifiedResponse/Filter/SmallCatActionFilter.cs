using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmallCat.Extensions.UnifiedResponse.Models;
using SmallCat.Extensions.UnifiedResponse.Helpers;

namespace SmallCat.Extensions.UnifiedResponse.Filter;

/// <summary>
/// 统一响应
/// </summary>
public class SmallCatActionFilter : IActionFilter, IAsyncActionFilter
{
    public virtual void OnActionExecuting(ActionExecutingContext context)
    {
        context.DataValidation();
    }

    public virtual void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
    }
}