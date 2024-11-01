using Microsoft.AspNetCore.Authorization;

namespace SmallCat.Extensions.JwtAuthorization.Handler;

/// <summary>
/// 自定义授权
/// </summary>
public abstract class SmallCatAuthorizationHandler : IAuthorizationHandler
{
    /// <summary>
    /// 验证授权是否成功
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual Task<bool> CheckAuthorization(AuthorizationHandlerContext context)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// 实现授权接口
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var noAuthRequirements = context.PendingRequirements;
        try
        {
            var authorization = await CheckAuthorization(context);
            if (authorization)
            {
                context.Succeed(noAuthRequirements.FirstOrDefault());
            }
            else
            {
                context.Fail();
            }
        }
        catch (Exception)
        {
            context.Fail();
        }
    }
}