namespace BiblioMit.Models.ViewModels
{
    public class ContactCommune : CommuneList
    {
        public int Id { get; set; }
    }
    public class CommuneList
    {
        public string? Commune { get; set; }
        public string? Province { get; set; }
    }
}
