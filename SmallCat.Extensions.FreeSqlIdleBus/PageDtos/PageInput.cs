using FreeSql.Internal.Model;

namespace SmallCat.Extensions.FreeSqlIdleBus.PageDtos;

public  class PageInput
{
    /// <summary>
    /// 页码
    /// </summary>
    public int? PageIndex { get; set; }

    /// <summary>
    /// 页面大小
    /// </summary>
    public int? PageSize { get; set; }


    public BasePagingInfo ToBasePagingInfo()
    {
        return new BasePagingInfo
        {
            PageNumber = PageIndex is null ? 0 : PageIndex < 0 ? 0 : PageIndex.Value,
            PageSize = PageSize is null ? 10 : PageSize < 0 ? 10 : PageSize.Value,
            Count = 0
        };
    }
}