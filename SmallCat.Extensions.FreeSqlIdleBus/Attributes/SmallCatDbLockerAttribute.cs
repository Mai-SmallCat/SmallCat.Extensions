namespace SmallCat.Extensions.FreeSqlIdleBus.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class SmallCatDbLockerAttribute : Attribute
{
    public List<string> Lockers { get; set; } = [];

    public SmallCatDbLockerAttribute()
    {
    }

    public SmallCatDbLockerAttribute(params string[] lockers)
    {
        Lockers = lockers.Distinct().ToList();
    }
}