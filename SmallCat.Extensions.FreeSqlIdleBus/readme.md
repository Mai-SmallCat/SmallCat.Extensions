```csharp
# eg: curdAfter 
Action<FreeSql.Aop.CurdAfterEventArgs> = (e)=> 
{
    var realSql = e.Sql;

    e.DbParms.ToList().ForEach(p => { realSql = realSql.Replace(p.ParameterName, $"'{p.Value}'"); });

    MiniProfiler.Current.CustomTiming($"CurdAfter", realSql, executeType: "Execute FreeSQL Query", true);

    Log.Information("[{ServiceName}]: {Sql}", "FreeSql", realSql.Replace("\r\n", ""));
}


# eg: syncStructureBefore
Action<FreeSql.Aop.SyncStructureBeforeEventArgs> = (e)=> 
{
    Log.Information("[{ServiceName}]: 即将要迁移的实体: {info}", "FreeSql", e.EntityTypes.Select(t => t.Name));
}

```