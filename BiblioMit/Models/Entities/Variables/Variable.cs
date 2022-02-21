using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models.Entities.Variables
{
    public class Variable
    {
        public int Id { get; set; }
        public int PsmbId { get; set; }
        [Display(Name = "PSMB Area"), AllowNull]
        public virtual Psmb Psmb { get; set; }
        public int VariableTypeId { get; set; }
        [Display(Name = "Type of Variable"), AllowNull]
        public virtual VariableType VariableType { get; set; }
        [Display(Name = "Value")]
        public int Value { get; set; }
        [Display(Name = "Date")]
        public DateTime Date { get; set; }
    }
}
