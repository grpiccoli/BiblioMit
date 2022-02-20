﻿namespace BiblioMit.Models
{
    public class Forum
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime Created { get; set; }
        public Uri? ImageUrl { get; set; }
        public virtual ICollection<Post> Posts { get; internal set; } = new List<Post>();
    }
}
