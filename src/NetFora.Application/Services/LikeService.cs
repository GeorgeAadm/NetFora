using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFora.Application.Interfaces;

namespace NetFora.Application.Services
{
    public class LikeService : ILikeService
    {

        public Task<int> GetLikeCountAsync(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsPostLikedByUserAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LikePostAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnlikePostAsync(int postId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
