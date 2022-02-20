using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;

namespace BiblioMit.Models.ManageViewModels
{
    public class ExternalLoginsViewModel
    {
        public Collection<UserLoginInfo> CurrentLogins { get; } = new Collection<UserLoginInfo>();

        public Collection<AuthenticationScheme> OtherLogins { get; } = new Collection<AuthenticationScheme>();

        public bool ShowRemoveButton { get; set; }

        public string? StatusMessage { get; set; }
    }
}
