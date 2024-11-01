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
        if (context.Canceled) return;

        var data = context.Result switch
        {
            ContentResult contentResult => new KeyValuePair<bool, object>(true,  contentResult.Content),
            JsonResult jsonResult       => new KeyValuePair<bool, object>(true,  jsonResult.Value),
            ObjectResult objectResult   => new KeyValuePair<bool, object>(true,  objectResult.Value),
            _                           => new KeyValuePair<bool, object>(false, null),
        };
        if (data.Key)
        {
            if (!context.SkipUnifiedResponse())
            {
                context.Result = new JsonResult(new UnifiedResult<object>
                {
                    Success    = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message    = null,
                    Data       = data.Value
                });
            }
        }

        context.Canceled = true;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
    }
}