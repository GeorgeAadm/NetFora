﻿using System;
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
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Comments.AnyAsync(c => c.Id == id);
        }

        #region GET

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsForPostAsync(int postId, CommentQueryParameters parameters)
        {
            var query = _context.Comments
                .Include(c => c.Author)
                .Where(c => c.PostId == postId);

            query = ApplyFilters(query, parameters);
            query = ApplySorting(query, parameters);

            return await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetFlaggedCommentsAsync(CommentQueryParameters parameters)
        {
            var query = _context.Comments
                .Include(c => c.Author)
                .Where(c => c.ModerationFlags > 0);

            query = ApplyFilters(query, parameters);
            query = ApplySorting(query, parameters);

            return await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        #endregion

        #region COUNTS

        public async Task<int> GetCommentCountForPostAsync(int postId)
        {
            return await _context.Comments.CountAsync(c => c.PostId == postId);
        }


        public async Task<int> GetTotalCountForPostAsync(int postId, CommentQueryParameters parameters)
        {
            var query = _context.Comments.Where(c => c.PostId == postId);
            query = ApplyFilters(query, parameters);
            return await query.CountAsync();
        }

        public async Task<int> GetFlaggedCommentCountAsync(CommentQueryParameters parameters)
        {
            var query = _context.Comments.Where(c => c.ModerationFlags > 0);
            query = ApplyFilters(query, parameters);
            return await query.CountAsync();
        }

        #endregion

        public async Task<bool> IsUserAuthorAsync(int commentId, string userId)
        {
            return await _context.Comments.AnyAsync(c => c.Id == commentId && c.AuthorId == userId);
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        private IQueryable<Comment> ApplyFilters(IQueryable<Comment> query, CommentQueryParameters parameters)
        {
            if (parameters.ModerationFlags.HasValue)
                query = query.Where(c => (c.ModerationFlags & parameters.ModerationFlags.Value) > 0);

            return query;
        }

        private IQueryable<Comment> ApplySorting(IQueryable<Comment> query, CommentQueryParameters parameters)
        {
            return parameters.SortBy switch
            {
                CommentSortBy.CreatedDate => parameters.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(c => c.CreatedAt)
                    : query.OrderByDescending(c => c.CreatedAt),
                CommentSortBy.AuthorName => parameters.SortDirection == SortDirection.Ascending
                    ? query.OrderBy(c => c.Author.DisplayName)
                    : query.OrderByDescending(c => c.Author.DisplayName),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };
        }
    }
}
