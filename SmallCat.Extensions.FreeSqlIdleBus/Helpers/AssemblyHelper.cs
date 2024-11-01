using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace SmallCat.Extensions.FreeSqlIdleBus.Helpers;

internal static class AssemblyHelper
{
    internal static List<Assembly> GetProjectAssemblies()
    {
        // 发布环境使用。
        var context = DependencyContext.Default?.RuntimeLibraries;
        if (context == null)
        {
            return [];
        }

        var projectAssemblyNames = context.Where(e => e.Type == "project").Select(e => e.Name).ToList();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(e => projectAssemblyNames.Contains(e.GetName().Name ?? "Un known")).ToList();

        return assemblies;
    }
}