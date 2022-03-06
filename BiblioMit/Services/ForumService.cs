using BiblioMit.Data;
using BiblioMit.Models;
using Microsoft.EntityFrameworkCore;

namespace BiblioMit.Services
{
    public class ForumService : IForum
    {
        private readonly ApplicationDbContext _context;

        public ForumService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Create(Forum forum)
        {
            _context.Add(forum);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task Delete(int forumId)
        {
            Forum forum = GetbyId(forumId);
            _context.Remove(forum);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public IEnumerable<ApplicationUser?> GetActiveUsers(int id)
        {
            ICollection<Post> posts = GetbyId(id).Posts;

            if (!posts.Any())
            {
                IEnumerable<ApplicationUser?> postUsers = posts.Select(p => p.User);
                IEnumerable<ApplicationUser?> replyUsers = posts.SelectMany(p => p.Replies).Select(r => r.User);
                return postUsers.Union(replyUsers).Distinct();
            }
            return new List<ApplicationUser?>();
        }

        public IEnumerable<Forum> GetAll()
        {
            return _context.Forums
                .Include(f => f.Posts);
        }

        public Forum GetbyId(int id)
        {
            Forum forum = _context.Forums
                .Where(f => f.Id == id)
                .Include(f => f.Posts)
                    .ThenInclude(p => p.User)
                .Include(f => f.Posts)
                    .ThenInclude(p => p.Replies)
                        .ThenInclude(r => r.User)
                .First();

            return forum;
        }

        public bool HasRecentPost(int id)
        {
            const int hoursAgo = 78;
            DateTime window = DateTime.Now.AddHours(-hoursAgo);
            return GetbyId(id).Posts.Any(p => p.Created > window);
        }

        public Task UpdateForumDescription(int forumId, string newDescription)
        {
            throw new NotImplementedException();
        }

        public Task UpdateForumTitle(int forumId, string newTitle)
        {
            throw new NotImplementedException();
        }
    }
}
