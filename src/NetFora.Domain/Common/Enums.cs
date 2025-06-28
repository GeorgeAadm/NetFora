namespace NetFora.Domain.Common
{
    public enum PostSortBy
    {
        CreatedDate,
        LikeCount,
        CommentCount,
        Title,
        AuthorName
    }

    public enum CommentSortBy
    {
        CreatedDate,
        AuthorName
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}
