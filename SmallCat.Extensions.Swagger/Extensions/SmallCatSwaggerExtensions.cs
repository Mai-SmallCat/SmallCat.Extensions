using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace SmallCat.Extensions.Swagger;

/// <summary>
/// 注册动态Api
/// <para></para>
/// PandaDynamicWebApi
/// <para></para>
/// Swagger
/// </summary>
public static class SmallCatSwaggerExtensions
{
    private static readonly OpenApiInfo _openApiInfo = new() { Title = "RemMai'Blog 开放Api接口", Version = "v1" };

    public static IServiceCollection AddSmallCatSwagger(this IServiceCollection services, Action<SwaggerGenOptions>? swaggerGenOptions = null)
    {
        var serviceProvider = services.BuildServiceProvider(false);
        var configuration   = serviceProvider.GetRequiredService<IConfiguration>();
        var section         = configuration.GetSection("OpenApiInfo");
        if (section.Value != null)
        {
            configuration.GetSection("OpenApiInfo").Bind(_openApiInfo);
        }

        swaggerGenOptions ??= options =>
        {
            options.SwaggerDoc(_openApiInfo.Version, _openApiInfo);

            // TODO：一定要返回true！
            options.DocInclusionPredicate((_, _) => true);

            // TODO：不可省略，否则Swagger中文注释不显示。
            DirectoryInfo directoryInfo = new(AppContext.BaseDirectory);

            directoryInfo.GetFiles("*.xml").Select(e => e.FullName).ToList().ForEach(xmlFullName =>
            {
                // 添加控制器层注释，true表示显示控制器注释
                options.IncludeXmlComments(xmlFullName, true);
            });

            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            options.OperationFilter<SecurityRequirementsOperationFilter>(true, JwtBearerDefaults.AuthenticationScheme);

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description  = "JWT Authorization header using the Bearer scheme.z",
                Name         = "Authorization",
                In           = ParameterLocation.Header,
                BearerFormat = "JWT",
                Scheme       = JwtBearerDefaults.AuthenticationScheme,
                Type         = SecuritySchemeType.ApiKey
            });
        };

        // 注册Swagger
        services.AddSwaggerGen(swaggerGenOptions);

        return services;
    }


    public static IApplicationBuilder UseSmallCatSwaggerUi(this IApplicationBuilder app, Action<SwaggerUIOptions>? swaggerUiAction = null, Action<SwaggerOptions>? swaggerAction = null)
    {
        if (swaggerUiAction != null)
        {
            app.UseSwaggerUI(swaggerUiAction);
        }
        else
        {
            app.UseSwaggerUI(options =>
            {
                options.IndexStream = () => Assembly.GetExecutingAssembly().GetManifestResourceStream("SmallCat.Extensions.Swagger.UI.index.html");
                options.RoutePrefix = "swagger";
                options.SwaggerEndpoint($"/swagger/{_openApiInfo.Version}/swagger.json", _openApiInfo.Title);
            });
        }

        app.UseSwagger(swaggerAction);
        return app;
    }
}