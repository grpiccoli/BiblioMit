using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    #region snippet1
    public class Contact
    {
        public int Id { get; set; }

        [Display(Name = "Submitted by")]
        // user Id from AspNetUser table
        public string? OwnerId { get; set; }
        [AllowNull]
        public virtual ApplicationUser Owner { get; set; }
        [Display(Name = "Centre")]
        public int ConsessionOrResearchId { get; set; }
        [AllowNull]
        public virtual Psmb ConsessionOrResearch { get; set; }
        [Display(Name = "Name")]
        public string? Name { get; set; }
        [Display(Name = "Surname")]
        public string? Last { get; set; }
        [Display(Name = "Phone number")]
        [DataType(DataType.PhoneNumber)]
        [DisplayFormat(DataFormatString = "{0:+## # #### ####}")]
        public long Phone { get; set; }
        [Display(Name = "Description")]
        public string? Description { get; set; }
        [Display(Name = "Position")]
        public Position Position { get; set; }
        [Display(Name = "Working Hours")]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime OpenHr { get; set; }
        [Display(Name = "Working Hours")]
        [DisplayFormat(DataFormatString = "{0:H:mm}")]
        public DateTime CloseHr { get; set; }
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Display(Name = "Status")]
        public ContactStatus Status { get; set; }
    }
    #endregion
}