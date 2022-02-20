using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Ads
{
    public class Payment
    {
        public int Id { get; set; }
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Total Cost")]
        [Range(minimum:0, maximum:1_000_000_000)]
        public int Price { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime DueDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:YYYY-MM}")]
        [Display(Name = "Period")]
        [DataType(DataType.Date)]
        public DateTime PeriodDate { get; set; }
        public virtual Banner? Banner { get; set; }
        public int BannerId { get; set; }
        public bool Paid() => PaidDate.HasValue;
        public bool OverDue() => DueDate < DateTime.Today && !Paid();
    }
}
