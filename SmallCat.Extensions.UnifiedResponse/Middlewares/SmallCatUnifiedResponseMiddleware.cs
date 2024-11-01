using Microsoft.AspNetCore.Http;
using SmallCat.Extensions.UnifiedResponse.Models;

namespace SmallCat.Extensions.UnifiedResponse.Middlewares;

public class SmallCatUnifiedResponseMiddleware
{
    private readonly RequestDelegate _next;

    public SmallCatUnifiedResponseMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            var msg        = string.Empty;
            if (statusCode < 400)
            {
                msg = string.Empty;
            }
            else
            {
                msg = statusCode switch
                {
                    401 => "未授权访问",
                    403 => "未授权访问",
                    404 => "未找到该服务",
                    405 => "请求方法错误",
                    500 => "程序内部错误",
                    502 => "请求方式错误",
                    _   => "其他未知错误，请联系管理员",
                };
            }

            if (!string.IsNullOrEmpty(msg))
            {
                await HandleExceptionAsync(context, statusCode, msg);
            }
        }
        catch (Exception ex)
        {
            if (ex is SmallCatException smallCatException)
            {
                await HandleExceptionAsync(context, smallCatException.StatusCode, smallCatException.Message);
            }

            var statusCode = context.Response.StatusCode;
            await HandleExceptionAsync(context, statusCode, ex.Message);
        }
    }

    //异常错误信息捕获，将错误信息用Json方式返回
    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string msg)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.WriteAsJsonAsync(new UnifiedResult<string> { StatusCode = statusCode, Error = msg });
        }

        return Task.CompletedTask;
    }
}
