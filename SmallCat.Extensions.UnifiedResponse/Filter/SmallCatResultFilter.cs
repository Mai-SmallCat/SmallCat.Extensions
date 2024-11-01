using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SmallCat.Extensions.UnifiedResponse.Models;
using SmallCat.Extensions.UnifiedResponse.Helpers;


namespace SmallCat.Extensions.UnifiedResponse.Filter;

public class SmallCatResultFilter : IResultFilter, IAsyncResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
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
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();
    }
}