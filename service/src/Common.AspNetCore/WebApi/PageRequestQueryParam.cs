using Common.Core.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Common.AspNetCore.WebApi;

public class PageRequestQueryParam
{
    [Description("Page number to retrieve (1-based index). Defaults to 1 if not specified.")]
    [FromQuery] public int? Page { get; set; }

    [Description("Maximum number of items to return per page. Defaults to 10 if not specified. Use 0 for unlimited single page request.")]
    [FromQuery] public int? Limit { get; set; }

    public PageRequest ToUseCaseObject(bool defaultUnpaged = false)
    {
        var def = (defaultUnpaged ? PageRequest.Unpaged : PageRequest.Default);
        return def with
        {
            Page = Page ?? def.Page,
            Limit = Limit ?? def.Limit,
        };
    }
}