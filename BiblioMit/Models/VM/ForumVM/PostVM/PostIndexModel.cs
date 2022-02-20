using BiblioMit.Models.ReplyViewModels;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.PostViewModels
{
    public class PostIndexModel
    {
        public PostIndexModel(
            string? title,
            string? authorId,
            string? authorEmail,
            string? authorName,
            string? forumName,
            Uri? authorImageUrl,
            IEnumerable<PostReplyModel> replies)
        {
            Title = title;
            AuthorId = authorId;
            AuthorEmail = authorEmail;
            AuthorName = authorName;
            ForumName = forumName;
            AuthorImageUrl = authorImageUrl;
            Replies = replies;
        }
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public Uri? AuthorImageUrl { get; set; }
        public int? AuthorRating { get; set; }
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d-MM-yy}")]
        public DateTime Created { get; set; }
        public string? PostContent { get; set; }
        public int ForumId { get; set; }
        public string? ForumName { get; set; }

        public IEnumerable<PostReplyModel> Replies { get; set; }
    }
}