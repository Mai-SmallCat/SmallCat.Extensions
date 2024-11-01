using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace SmallCat.Extensions.DynamicWebApi;

public class ActionDescriptorChangeProvider : IActionDescriptorChangeProvider
{
    public static ActionDescriptorChangeProvider Instance = new();
    public CancellationTokenSource TokenSource { get; set; }

    public IChangeToken GetChangeToken()
    {
        TokenSource = new CancellationTokenSource();
        return new CancellationChangeToken(TokenSource.Token);
    }
}