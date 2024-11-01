using FreeSql;

namespace SmallCat.Extensions.FreeSqlIdleBus.Options;

public class SmallCatDbOptionBase
{
    /// <summary>
    /// 锁类型
    /// </summary>
    public string LockerKey { get; set; }

    /// <summary>
    /// 自动迁移数据库
    /// </summary>
    public bool AutoSyncStructure { get; set; }

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; }
}

internal class SmallCatDbConfiguration : SmallCatDbOptionBase
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public string FreeSqlDataType { get; set; }
}

public class SmallCatDbOption : SmallCatDbOptionBase
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public DataType FreeSqlDataType { get; set; }
}

public class SmallCatDbRegisterOption : SmallCatDbOption
{
    /// <summary>
    /// 需要迁移的实体
    /// </summary>
    public List<Type> SyncStructureEntities { get; set; } = [];
}