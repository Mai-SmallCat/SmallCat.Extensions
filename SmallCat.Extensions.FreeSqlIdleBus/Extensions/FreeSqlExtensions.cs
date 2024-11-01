using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmallCat.Extensions.FreeSqlIdleBus.Options;
using SmallCat.Extensions.FreeSqlIdleBus.Helpers;

namespace SmallCat.Extensions;

/// <summary>
/// FreeSql 拓展
/// </summary> 
public static class FreeSqlIdleBusExtensions
{
    private static void PrintDataBaseType(ILogger logger)
    {
        var names = Enum.GetNames<DataType>();
        logger.LogInformation("[{Modul}]: {FreeSqlDataType} 目前支持的类型有{names}", "SmallCat.Extensions.FreeSqlIdleBus", "FreeSqlDataType", names);
    }

    private static IHostEnvironment HostEnvironment { get; set; }
    private static bool             _isRegister = false;

    /// <summary>
    /// Use Dynamic WebApi to Configure
    /// </summary>
    /// <param name="services">Service</param>
    /// <param name="freeSqlDbOptions">FreeSQL Config</param>
    /// <returns></returns>
    public static IServiceCollection AddSmallCatDb(
        this IServiceCollection                           services,
        List<SmallCatDbOption>?                           freeSqlDbOptions    = null,
        Action<FreeSql.Aop.CurdAfterEventArgs>?           curdAfter           = null,
        Action<FreeSql.Aop.SyncStructureBeforeEventArgs>? syncStructureBefore = null,
        Action<IAop>?                                     freeSqlDbAopAction  = null
    )
    {
        var serviceProvider = services.BuildServiceProvider(false);
        var logger          = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(string.Empty);
        HostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

        if (_isRegister)
        {
            logger.LogWarning("[{Modul}]: 已经注入该服务，请勿重复注入", "SmallCat.Extensions.FreeSqlIdleBus");
            return services;
        }


        if (freeSqlDbOptions is { Count: 0 }) return services;

        var freeSqlDbRegisterOptions = freeSqlDbOptions.Select(t => new SmallCatDbRegisterOption
        {
            LockerKey                = t.LockerKey.Trim(),
            FreeSqlDataType       = t.FreeSqlDataType,
            AutoSyncStructure     = t.AutoSyncStructure,
            ConnectionString      = t.ConnectionString,
            SyncStructureEntities = FreeSqlContextHelper.LockerAndTypes[t.LockerKey].ToList(),
        }).ToList();

        DbRegister(freeSqlDbRegisterOptions, curdAfter, syncStructureBefore, freeSqlDbAopAction);

        _isRegister = true;
        return services;
    }


    public static IServiceCollection AddSmallCatDb(this IServiceCollection                           services,
                                                   string                                            freeSqlConnectionKey,
                                                   Action<FreeSql.Aop.CurdAfterEventArgs>?           curdAfter           = null,
                                                   Action<FreeSql.Aop.SyncStructureBeforeEventArgs>? syncStructureBefore = null,
                                                   Action<IAop>?                                     freeSqlDbAopAction  = null
    )
    {
        var serviceProvider = services.BuildServiceProvider(false);
        var configuration   = serviceProvider.GetRequiredService<IConfiguration>();
        var logger          = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(string.Empty);
        HostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

        if (_isRegister)
        {
            logger.LogWarning("[{Modul}]: 已经注入该服务，请勿重复注入", "SmallCat.Extensions.FreeSqlIdleBus");
            return services;
        }


        // 打印DataType
        PrintDataBaseType(logger);

        ArgumentNullException.ThrowIfNull(freeSqlConnectionKey);
        var freeSqlConnectionStrings = new List<SmallCatDbConfiguration>();
        configuration.Bind(freeSqlConnectionKey, freeSqlConnectionStrings);

        if (freeSqlConnectionStrings is null or { Count: 0 })
        {
            return services;
        }

        var data = FreeSqlContextHelper.LockerAndTypes;

        var freeSqlDbRegisterOptions = freeSqlConnectionStrings.Select(t => new SmallCatDbRegisterOption
        {
            LockerKey             = t.LockerKey.Trim(),
            FreeSqlDataType       = Enum.Parse<DataType>(t.FreeSqlDataType.Trim()),
            AutoSyncStructure     = t.AutoSyncStructure,
            ConnectionString      = t.ConnectionString,
            SyncStructureEntities = FreeSqlContextHelper.LockerAndTypes[t.LockerKey].ToList(),
        }).ToList();

        DbRegister(freeSqlDbRegisterOptions, curdAfter, syncStructureBefore, freeSqlDbAopAction);

        _isRegister = true;
        return services;
    }


    private static void DbRegister(List<SmallCatDbRegisterOption>                    configurations,
                                   Action<FreeSql.Aop.CurdAfterEventArgs>?           curdAfter           = null,
                                   Action<FreeSql.Aop.SyncStructureBeforeEventArgs>? syncStructureBefore = null,
                                   Action<IAop>?                                     freeSqlDbAopAction  = null
    )
    {
        foreach (var freeSqlDbConfiguration in configurations)
        {
            Cat.Db.Register(freeSqlDbConfiguration.LockerKey, () =>
            {
                var db = new FreeSqlBuilder().UseConnectionString(
                    freeSqlDbConfiguration.FreeSqlDataType,
                    freeSqlDbConfiguration.ConnectionString
                ).Build();

                if (HostEnvironment.IsDevelopment() || freeSqlDbConfiguration.AutoSyncStructure)
                {
                    db.CodeFirst.IsAutoSyncStructure = true;
                    db.CodeFirst.SyncStructure(freeSqlDbConfiguration.SyncStructureEntities.Distinct().ToArray());
                }

                freeSqlDbAopAction?.Invoke(db.Aop);

                if (curdAfter != null)
                {
                    db.Aop.CurdAfter += (_, args) => { curdAfter.Invoke(args); };
                }

                if (syncStructureBefore != null)
                {
                    db.Aop.SyncStructureBefore += (_, args) => { syncStructureBefore.Invoke(args); };
                }

                return db;
            });
        }
    }
}