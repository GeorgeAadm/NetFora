using System;
using NetFora.Domain.Common;

namespace NetFora.Application.QueryParameters
{
    public class PostQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;


        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }


        public string? AuthorName { get; set; }


        public string? SearchTerm { get; set; }
        public int? ModerationFlags { get; set; }


        public int? MinLikes { get; set; }
        public int? MaxLikes { get; set; }
        public bool? HasComments { get; set; }


        public PostSortBy SortBy { get; set; } = PostSortBy.CreatedDate;
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    }
}
