using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using SmallCat.Extensions.Serialize.Converts;

namespace SmallCat.Extensions.Serialize;

public static class SmallCatJsonOptionsExtensions
{
    public static IServiceCollection AddSmallCatSerializeOption(this IServiceCollection serviceCollection, Action<JsonOptions>? configureOptions)
    {
        if (configureOptions != null)
        {
            serviceCollection.Configure(configureOptions);
            return serviceCollection;
        }

        serviceCollection.Configure<JsonOptions>(options =>
        {
            // 属性名称不敏感。
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.AllowTrailingCommas         = true;
            // 允许注释
            options.JsonSerializerOptions.ReadCommentHandling  = JsonCommentHandling.Skip;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            // Converters
            options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
        });

        return serviceCollection;
    }
}