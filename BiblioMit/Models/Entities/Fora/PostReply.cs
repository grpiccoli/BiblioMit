using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class PostReply
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Created { get; set; }
        [AllowNull]
        public virtual ApplicationUser User { get; set; }
        public int PostId { get; set; }
        [AllowNull]
        public virtual Post Post { get; set; }
    }
}
