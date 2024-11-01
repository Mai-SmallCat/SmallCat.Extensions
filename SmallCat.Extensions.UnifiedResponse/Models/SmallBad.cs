namespace SmallCat.Extensions;

/// <summary>
/// CatQwQ （囧）
/// </summary>
public static class CatQwQ
{
    /// <summary>
    /// 全局异常处理
    /// </summary>
    /// <returns></returns>
    public static SmallCatException Error(string message, int statusCode = 500)
    {
        return new SmallCatException(message, statusCode);
    }
}

public class SmallCatException : Exception
{
    public SmallCatException(string message, int statusCode)
    {
        Message    = message;
        StatusCode = statusCode;
    }

    public          int    StatusCode { get; }
    public override string Message    { get; }
}