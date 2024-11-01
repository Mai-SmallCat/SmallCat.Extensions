using System.Collections.Concurrent;
using System.Reflection;
using SmallCat.Extensions.FreeSqlIdleBus.Attributes;


namespace SmallCat.Extensions.FreeSqlIdleBus.Helpers;

internal static class FreeSqlContextHelper
{
    private static List<Assembly> Assemblies { get; } = AssemblyHelper.GetProjectAssemblies();

    internal static ConcurrentDictionary<string, ConcurrentBag<Type>> LockerAndTypes { get; } = GetLockerAndTypes();

    private static ConcurrentDictionary<string, ConcurrentBag<Type>> GetLockerAndTypes()
    {
        var types = Assemblies.SelectMany(t => t.GetTypes().Where(type => type is { IsPublic: true, IsSealed: false, IsInterface: false, IsGenericType: false })).ToList();

        var defaultLockers = new List<string> { "DefaultLocker" };
        // 多线程处理

        var bagDict = new ConcurrentDictionary<string, ConcurrentBag<Type>>();

        types.ForEach(type =>
        {
            var attrs = type.GetCustomAttributes(false);
            var tableAttributes = attrs
                .Where(attr =>
                    attr.GetType() == typeof(FreeSql.DataAnnotations.TableAttribute) ||
                    attr.GetType() == typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute)
                );

            // 如果不包含TableAttribute，则跳过下一个， FreeSQL兼容Microsoft的DataAnnotations
            if (!tableAttributes.Any()) return;

            var freeSqlTableAttribute = tableAttributes.FirstOrDefault(t => t.GetType() == typeof(FreeSql.DataAnnotations.TableAttribute));
            if (freeSqlTableAttribute == null || ((FreeSql.DataAnnotations.TableAttribute)freeSqlTableAttribute).DisableSyncStructure)
            {
                return;
            }

            var dbLockerAttribute = attrs.FirstOrDefault(t => t.GetType() == typeof(SmallCatDbLockerAttribute));
            var lockers           = dbLockerAttribute == null ? defaultLockers : ((SmallCatDbLockerAttribute)dbLockerAttribute).Lockers;

            if (!lockers.Any())
            {
                lockers = defaultLockers;
            }

            foreach (var locker in lockers.Distinct())
            {
                if (bagDict.TryGetValue(locker, out var value))
                {
                    value.Add(type);
                }
                else
                {
                    bagDict.AddOrUpdate(locker, _ => [type], (_, _) => [type]);
                }
            }
        });

        return bagDict;
    }
}