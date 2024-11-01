using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallCat.Extensions.UnifiedResponse.Filter;
using SmallCat.Extensions.UnifiedResponse.Middlewares;

namespace SmallCat.Extensions.UnifiedResponse;

public static class SmallCatUnifiedResponseExtensions
{
    public static IServiceCollection AddSmallCatUnifiedResponse(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider(false);
        var logger          = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(string.Empty);

        logger.LogInformation("添加过滤器：{SmallCatActionFilter},序号：{order}",                 nameof(SmallCatActionFilter), 1);
        logger.LogInformation("添加过滤器：{SmallCatResultFilter},序号：{order}",                 nameof(SmallCatResultFilter), 2);
        logger.LogInformation("请不要忘记在{useAuthorication}前引入{code}，否则全局异常处理和自定义Jwt认证会失效。", "app.UseAuthorization()",     "app.UseSmallCatUnifiedResponse()");
        // 全局服务注入 
        services.Configure<MvcOptions>(option =>
        {
            option.Filters.Add<SmallCatActionFilter>(1);
            option.Filters.Add<SmallCatResultFilter>(2);
        });

        return services;
    }

    public static IApplicationBuilder UseSmallCatUnifiedResponse(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SmallCatUnifiedResponseMiddleware>();
    }
}