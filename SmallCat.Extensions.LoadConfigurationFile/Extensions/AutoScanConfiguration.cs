using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmallCat.Extensions.LoadConfigurationFile.Helpers;

namespace SmallCat.Extensions.LoadConfigurationFile;

public static class SmallCatLoadConfigurationFileExtensions
{
    private static readonly string? CurrentServiceName = System.Reflection.MethodBase.GetCurrentMethod()?.ReflectedType?.Name;

    /// <summary>
    /// 自动扫描Json配置文件，并且加载到configuration对象中。
    /// </summary>
    /// <param name="service"></param>
    /// <param name="filterJsonFileList"></param>  
    /// <returns></returns>
    public static WebApplicationBuilder LoadSmallCatConfigurationFile(this WebApplicationBuilder webApplicationBuilder, params string[] filterJsonFileList)
    {
        var serviceProvider = webApplicationBuilder.Services.BuildServiceProvider(false);
        var logger          = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SmallCat.Extensions.LoadConfigurationFile");

        logger.LogInformation("[{ServiceName}]: 自动读取配置文件，状态: {status}", CurrentServiceName, "进行中");

        var path                      = AppDomain.CurrentDomain.BaseDirectory;
        var smallCatJsonConfiguration = ResourceHelper.GetJsonResources();

        smallCatJsonConfiguration.ForEach(e => { webApplicationBuilder.Configuration.AddJsonStream(e); });

        string[] defaultFilterJsonFiles =
        [
            "appsettings.*.json",
            "appsettings.json",
            "*.runtimeconfig.json",
            "*.deps.json"
        ];

        var filterJsonFiles = defaultFilterJsonFiles.Union(filterJsonFileList).Select(t => t.ToLower()).Distinct();

        var excludeFiles = filterJsonFiles.SelectMany(t => Directory.GetFiles(path, t)).ToList();

        var allJsonFiles = Directory.GetFiles(path, "*.json").ToList();

        var jsonFiles = allJsonFiles.Except(excludeFiles).ToList();

        logger.LogInformation("[{ServiceName}]: 一共加载[{Count}]个Json类型的配置文件", CurrentServiceName, jsonFiles.Count);
        foreach (var jsonFile in jsonFiles)
        {
            webApplicationBuilder.Configuration.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);
            logger.LogInformation("[{ServiceName}]: 已加载: {filename}", CurrentServiceName, jsonFile);
        }

        logger.LogInformation("[{ServiceName}]: 自动读取配置文件，状态: {status}", CurrentServiceName, "已完成");

        return webApplicationBuilder;
    }
}