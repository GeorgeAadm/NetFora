using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFora.Domain.Common;

namespace NetFora.Application.QueryParameters
{
    public class CommentQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public CommentSortBy SortBy { get; set; } = CommentSortBy.CreatedDate;
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
        public int? ModerationFlags { get; set; }
    }
}
