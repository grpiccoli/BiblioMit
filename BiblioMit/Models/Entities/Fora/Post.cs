using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime Created { get; set; }
        public int ForumId { get; set; }
        [AllowNull]
        public virtual Forum Forum { get; set; }
        public string? UserId { get; set; }
        [AllowNull]
        public virtual ApplicationUser User { get; set; }
        public virtual IEnumerable<PostReply> Replies { get; internal set; } = new List<PostReply>();
    }
}
