using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmallCat.Extensions.MiniProfiler;

/// <summary>
/// 注册动态Api
/// <para></para>
/// PandaDynamicWebApi
/// <para></para>
/// Swagger
/// </summary>
public static class SmallCatMiniProfilerExtensions
{
    private static bool _enable = true;

    public static IServiceCollection AddSmallCatMiniProfiler(this IServiceCollection services, string routeBasePath = "/profiler")
    {
        var serviceProvider = services.BuildServiceProvider(false);
        var environment     = serviceProvider.GetRequiredService<IWebHostEnvironment>();
        var configuration   = serviceProvider.GetRequiredService<IConfiguration>();
        var isEnable        = configuration.GetSection("MiniProfiler").Value;
        var isDebug         = environment.IsDevelopment();

        _enable = isEnable == null || configuration.GetValue<bool>("MiniProfiler");

        if (_enable)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath            = routeBasePath;
                options.EnableMvcFilterProfiling = false;
                options.EnableMvcViewProfiling   = false;
                options.EnableDebugMode          = isDebug;
            });
        }

        return services;
    }

    public static IApplicationBuilder UseSmallCatMiniProfiler(this IApplicationBuilder app)
    {
        if (_enable) app.UseMiniProfiler();
        return app;
    }
}