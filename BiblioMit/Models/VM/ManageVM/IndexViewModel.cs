using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.ManageViewModels
{
    public class IndexViewModel
    {
        [Display(Name = "User Name")]
        public string? Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        public string? StatusMessage { get; set; }

        public Uri? ProfileImageUrl { get; set; }

        public DateTime MemberSince { get; set; }

        public int UserRating { get; set; }
    }
}
