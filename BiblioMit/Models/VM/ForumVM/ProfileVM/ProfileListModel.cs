namespace BiblioMit.Models.ProfileViewModels
{
    public class ProfileListModel
    {
        public IEnumerable<ProfileModel> Profiles { get; set; } = new List<ProfileModel>();
    }
}
