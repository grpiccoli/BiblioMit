using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Variables
{
    public class VariableType
    {
        public int Id { get; set; }
        [Display(Name = "Name")]
        public string? Name { get; set; }
        [Display(Name = "Units")]
        public string? Units { get; set; }
        public virtual ICollection<Variable> Variables { get; } = new List<Variable>();
    }
}
