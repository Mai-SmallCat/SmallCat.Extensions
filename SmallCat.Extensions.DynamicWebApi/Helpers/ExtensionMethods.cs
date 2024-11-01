﻿using System.Text.RegularExpressions;

namespace SmallCat.Extensions.DynamicWebApi.Helpers;

internal static class ExtensionMethods
{
    /// <summary>
    /// 判断字符串是否为空
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNullOrEmpty<T>(this ICollection<T> source)
    {
        return source is not { Count: > 0 };
    }

    public static bool IsIn(this string str, params string[] data)
    {
        return data.Any(item => str == item);
    }

    public static string RemovePostFix(this string str, params string[] postFixes)
    {
        switch (str)
        {
            case null:
                return null;
            case "":
                return string.Empty;
        }

        if (postFixes.IsNullOrEmpty())
        {
            return str;
        }

        foreach (var postFix in postFixes)
        {
            if (str.EndsWith(postFix))
            {
                return str.Left(str.Length - postFix.Length);
            }
        }

        return str;
    }

    public static string RemovePreFix(this string str, params string[] preFixes)
    {
        switch (str)
        {
            case null:
                return null;
            case "":
                return string.Empty;
        }

        if (preFixes.IsNullOrEmpty())
        {
            return str;
        }

        foreach (var preFix in preFixes)
        {
            if (str.StartsWith(preFix))
            {
                return str.Right(str.Length - preFix.Length);
            }
        }

        return str;
    }

    public static string Left(this string str, int len)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length < len)
        {
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        }

        return str.Substring(0, len);
    }

    public static string Right(this string str, int len)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length < len)
        {
            throw new ArgumentException("len argument can not be bigger than given string's length!");
        }

        return str.Substring(str.Length - len, len);
    }

    public static string GetCamelCaseFirstWord(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length == 1)
        {
            return str;
        }

        var res = Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})");

        if (res.Length < 1)
        {
            return str;
        }
        else
        {
            return res[0];
        }
    }

    public static string GetPascalCaseFirstWord(this string str)
    {
        if (str == null)
        {
            throw new ArgumentNullException(nameof(str));
        }

        if (str.Length == 1)
        {
            return str;
        }

        var res = Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})");

        return res.Length < 2 ? str : res[1];
    }

    public static string GetPascalOrCamelCaseFirstWord(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length <= 1)
        {
            return str;
        }

        return str[0] >= 65 && str[0] <= 90 ? GetPascalCaseFirstWord(str) : GetCamelCaseFirstWord(str);
    }
}