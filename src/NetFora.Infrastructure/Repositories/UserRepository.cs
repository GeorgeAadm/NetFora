using System;
using System.Threading.Tasks;
using NetFora.Domain.Entities;
using NetFora.Infrastructure.Interfaces;

namespace NetFora.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<bool> ExistsAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetDisplayNameAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetUserCommentCountAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetUserPostCountAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
