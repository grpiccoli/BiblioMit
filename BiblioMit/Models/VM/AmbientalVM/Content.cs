using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BiblioMit.Models.VM.AmbientalVM
{
    public class Content
    {
        [BindRequired]
        public string Name { get; set; } = null!;
        [BindRequired]
        public string Code { get; set; } = null!;
        public string? Commune { get; set; } = null!;
        public string? Province { get; set; } = null!;
        [BindRequired]
        public string Area { get; set; } = null!;
    }
}
