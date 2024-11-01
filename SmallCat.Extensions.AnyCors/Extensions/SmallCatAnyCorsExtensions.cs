using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SmallCat.Extensions.AnyCors;

public static class SmallCatAnyCorsExtensions
{
    private const string PolicyName = "smallcat-anycors";

    public static IServiceCollection AddSmallCatAnyCors(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddCors(options =>
        {
            options.AddPolicy(name: PolicyName,
                policy =>
                {
                    policy.SetIsOriginAllowed(_ => true);
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                });
        });
        return serviceCollection;
    }

    public static void UseSmallCatAnyCors(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseCors(PolicyName);
    }
}