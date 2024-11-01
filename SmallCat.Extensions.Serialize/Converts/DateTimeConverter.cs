using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmallCat.Extensions.Serialize.Converts;

/// <summary>
/// 时间序列化
/// </summary>
public class DateTimeConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// 读取
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty);
    }

    /// <summary>
    /// 写入
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}