using FreeSql.Internal.Model;

namespace SmallCat.Extensions.FreeSqlIdleBus.PageDtos;

/// <summary>
/// 分页结果
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class PageOutput<TModel>
{
    public PageOutput()
    {
    }

    public PageOutput(IEnumerable<TModel> datas, BasePagingInfo pageInfo)
    {
        Datas = datas.ToList();
        Calculate(pageInfo);
    }

    public PageOutput(List<TModel> datas, BasePagingInfo pageInfo)
    {
        Datas = datas;
        Calculate(pageInfo);
    }

    private void Calculate(BasePagingInfo pageInfo)
    {
        DataTotal = (int)pageInfo.Count;
        PageSize  = pageInfo.PageSize == 0 ? 1 : pageInfo.PageSize;
        PageIndex = pageInfo.PageNumber;
        PageCount = DataTotal / PageSize + (DataTotal % PageSize > 0 ? 1 : 0);
    }

    public int          PageSize  { get; set; } = 0;
    public int          PageIndex { get; set; } = 1;
    public int          PageCount { get; set; } = 0;
    public int          DataTotal { get; set; } = 0;
    public List<TModel> Datas     { get; set; }
}