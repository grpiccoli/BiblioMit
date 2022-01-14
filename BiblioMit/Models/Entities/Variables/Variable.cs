using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Variables
{
    public class Variable
    {
        public int Id { get; set; }
        public int PsmbId { get; set; }
        [Display(Name = "PSMB Area")]
        public virtual Psmb Psmb { get; set; }
        public int VariableTypeId { get; set; }
        [Display(Name = "Type of Variable")]
        public virtual VariableType VariableType { get; set; }
        [Display(Name = "Value")]
        public int Value { get; set; }
        [Display(Name = "Date")]
        public DateTime Date { get; set; }
    }
}
