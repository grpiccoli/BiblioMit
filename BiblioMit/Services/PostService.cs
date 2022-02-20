using BiblioMit.Data;
using BiblioMit.Extensions;
using BiblioMit.Models;
using Microsoft.EntityFrameworkCore;

namespace BiblioMit.Services
{
    public class PostService : IPost
    {
        private readonly ApplicationDbContext _context;
        public PostService(ApplicationDbContext context) => _context = context;
        public async Task Add(Post post)
        {
            _context.Add(post);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task AddReply(PostReply reply)
        {
            _context.PostReplies.Add(reply);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }
        public Task EditPostContent(int id, string newContent)
        {
            throw new NotImplementedException();
        }
        public IQueryable<Post> GetAll() => _context.Posts
            .Include(p => p.User)
            .Include(p => p.Replies)
            .ThenInclude(r => r.User)
        .Include(p => p.Forum)
        .Where(p => p.UserId != null);
        public Post GetById(int id) => _context.Posts
            .Include(p => p.User)
            .Include(p => p.Replies)
                .ThenInclude(r => r.User)
            .Include(p => p.Forum)
            .First(p => p.Id == id);
        public IEnumerable<Post> GetFilteredPosts(Forum forum, string searchQuery) =>
            string.IsNullOrEmpty(searchQuery)
                ? forum.Posts
                : forum.Posts.HasQuery(searchQuery);
        public IEnumerable<Post> GetFilteredPosts(string searchQuery) =>
            GetAll().HasQuery(searchQuery);
        public IEnumerable<Post> GetLatestsPosts(int n) =>
            GetAll().OrderByDescending(p => p.Created).Take(n);
        public IEnumerable<Post> GetPostsByForum(int id) => 
            _context.Forums.First(p => p.Id == id).Posts;
    }
}
