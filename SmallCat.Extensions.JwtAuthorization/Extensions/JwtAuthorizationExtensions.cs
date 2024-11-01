using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmallCat.Extensions.JwtAuthorization.Handler;
using SmallCat.Extensions.JwtAuthorization.Model;

namespace SmallCat.Extensions;

public static class JwtAuthorizationExtensions
{
    public static IServiceCollection AddJwt<TAuthorizationHandler>(this IServiceCollection services)
        where TAuthorizationHandler : class, IAuthorizationHandler
    {
        var serviceProvider = services.BuildServiceProvider(false);
        var configuration   = serviceProvider.GetRequiredService<IConfiguration>();

        // init 
        var jwtSetting = new JwtSetting();
        configuration.Bind("JwtSetting", jwtSetting);
        // 当密钥过短的时候。
        if (jwtSetting.IssuerSigningKey.Length < 64)
        {
            throw new ArgumentException("JWT settings must contain at least 64 bytes.");
        }
        if (!typeof(TAuthorizationHandler).IsSubclassOf(typeof(SmallCatAuthorizationHandler)))
        {
            throw new ArgumentException("TAuthorizationHandler must be a SmallCatAuthorizationHandler.");
        }

        // Option
        services.Configure<JwtSetting>(configuration.GetSection("JwtSetting"));

        // Register
        services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, TAuthorizationHandler>();

        var authenticationBuilder = services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        });

        authenticationBuilder.AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken            = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                // 是否验证发布者
                // 发布者名称
                ValidateIssuer           = jwtSetting.ValidateIssuer,
                ValidIssuer              = jwtSetting.ValidIssuer,
                ValidAudience            = jwtSetting.ValidAudience,
                ValidateAudience         = jwtSetting.ValidateAudience,
                ValidateIssuerSigningKey = jwtSetting.ValidateIssuerSigningKey,
                IssuerSigningKey         = new SymmetricSecurityKey(jwtSetting.IssuerSigningKeyByteArray),
                ValidateLifetime         = jwtSetting.ValidateLifetime,
                RequireExpirationTime    = jwtSetting.RequireExpirationTime,
                ClockSkew                = TimeSpan.FromTicks(jwtSetting.ClockSkew),
            };


            x.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    var access_token = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(access_token))
                    {
                        context.Token = access_token.ToString();
                    }

                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization();
        return services;
    }
}