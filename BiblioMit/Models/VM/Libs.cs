using System.Collections.Generic;
namespace BiblioMit.Models.VM
{
    public class Libs
    {
        public string? Version { get; set; }
        public string? DefaultProvider { get; set; }
        public List<LibManLibrary> Libraries { get; set; }
    }
}
