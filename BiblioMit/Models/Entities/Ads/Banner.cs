namespace BiblioMit.Models.Entities.Ads
{
    public class Banner
    {
        public int Id { get; set; }
        public ICollection<Img> Imgs { get; internal set; } = new List<Img>();
        public string? MaskAngle { get; set; }
        public ICollection<Caption> Texts { get; internal set; } = new List<Caption>();
        public ICollection<Rgb> Rgbs { get; internal set; } = new List<Rgb>();
        public virtual ICollection<Payment> Payments { get; internal set; } = new List<Payment>();
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public bool Active() => Payments is not null && Payments.Any() && !Payments.Any(p => p.OverDue());
    }
}
