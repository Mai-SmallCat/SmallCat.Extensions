using System.Linq.Expressions;
using FreeRedis.Internal;

namespace SmallCat.Extensions;

public static class Cat
{
    public static IdleBus<IFreeSql> Db { get; } = new(TimeSpan.FromMinutes(5));

    /// <summary>
    /// FreeSqlDataBase
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IFreeSql GetDb(string name = "DefaultLocker")
    {
        var db = Db.Get(name);

        Expression<Func<Lock, bool>> aa = @lock => true;

        return db ?? throw new ArgumentException($"Not found Db '{name}'!");
    }
}