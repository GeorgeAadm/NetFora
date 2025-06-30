using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetFora.Application.Interfaces.Repositories;
using NetFora.Application.QueryParameters;
using NetFora.Domain.Common;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Data;

namespace NetFora.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Post> AddAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Posts.AnyAsync(p => p.Id == id);
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Post?> GetByIdWithStatsAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Stats)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        
        public async Task<IEnumerable<Post>> GetPostsAsync(PostQueryParameters parameters, string? authorId = null)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Stats)
                .AsQueryable();

            if (!string.IsNullOrEmpty(authorId))
            {
                query = query.Where(p => p.AuthorId == authorId);
            }

            query = ApplyFilters(query, parameters);
            query = ApplySorting(query, parameters);

            return await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetUserPostsAsync(string userId, PostQueryParameters parameters)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Stats)
                .Where(p => p.AuthorId == userId);

            query = ApplyFilters(query, parameters);
            query = ApplySorting(query, parameters);

            return await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetFlaggedPostsAsync(PostQueryParameters parameters)
        {
            var query = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Stats)
                .Where(p => p.ModerationFlags > 0);

            query = ApplyFilters(query, parameters);
            query = ApplySorting(query, parameters);

            return await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }


        #region COUNTS

        public async Task<int> GetTotalCountAsync(PostQueryParameters parameters)
        {
            var query = _context.Posts.AsQueryable();
            query = ApplyFilters(query, parameters);
            return await query.CountAsync();
        }

        public async Task<int> GetUserPostCountAsync(string userId, PostQueryParameters parameters)
        {
            var query = _context.Posts.Where(p => p.AuthorId == userId);
            query = ApplyFilters(query, parameters);
            return await query.CountAsync();
        }
        
        public async Task<int> GetFlaggedPostCountAsync(PostQueryParameters parameters)
        {
            var query = _context.Posts.Where(p => p.ModerationFlags > 0);
            query = ApplyFilters(query, parameters);
            return await query.CountAsync();
        }

        #endregion

        public async Task<bool> IsUserAuthorAsync(int postId, string userId)
        {
            return await _context.Posts.AnyAsync(p => p.Id == postId && p.AuthorId == userId);
        }

        public Task UpdateAsync(Post post)
        {
            throw new NotImplementedException();
            // _context.Posts.Update(post);
            // await _context.SaveChangesAsync();
        }

        private IQueryable<Post> ApplyFilters(IQueryable<Post> query, PostQueryParameters parameters)
        {
            if (parameters.DateFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= parameters.DateFrom.Value);

            if (parameters.DateTo.HasValue)
                query = query.Where(p => p.CreatedAt <= parameters.DateTo.Value);

            if (!string.IsNullOrEmpty(parameters.AuthorDisplayName))
                query = query.Where(p => p.Author.DisplayName.Contains(parameters.AuthorDisplayName));

            if (!string.IsNullOrEmpty(parameters.AuthorUserName))
                query = query.Where(p => p.Author.UserName == parameters.AuthorUserName);

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
                query = query.Where(p => p.Title.Contains(parameters.SearchTerm) || p.Content.Contains(parameters.SearchTerm));

            if (parameters.ModerationFlags.HasValue)
                query = query.Where(p => (p.ModerationFlags & parameters.ModerationFlags.Value) > 0);

            if (parameters.MinLikes.HasValue)
                query = query.Where(p => p.Stats!.LikeCount >= parameters.MinLikes.Value);

            if (parameters.MaxLikes.HasValue)
                query = query.Where(p => p.Stats!.LikeCount <= parameters.MaxLikes.Value);

            if (parameters.HasComments.HasValue)
            {
                if (parameters.HasComments.Value)
                    query = query.Where(p => p.Stats!.CommentCount > 0);
                else
                    query = query.Where(p => p.Stats!.CommentCount == 0);
            }

            return query;
        }

        private IQueryable<Post> ApplySorting(IQueryable<Post> query, PostQueryParameters parameters)
        {
            return parameters.SortBy switch
            {
                PostSortBy.CreatedDate => parameters.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),
                PostSortBy.LikeCount => parameters.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(p => p.Stats!.LikeCount)
                    : query.OrderByDescending(p => p.Stats!.LikeCount),
                PostSortBy.CommentCount => parameters.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(p => p.Stats!.CommentCount)
                    : query.OrderByDescending(p => p.Stats!.CommentCount),
                PostSortBy.Title => parameters.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(p => p.Title)
                    : query.OrderByDescending(p => p.Title),
                PostSortBy.AuthorName => parameters.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(p => p.Author.DisplayName)
                    : query.OrderByDescending(p => p.Author.DisplayName),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }
    }
}
