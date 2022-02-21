using System.ComponentModel.DataAnnotations;

namespace CustomDataAnnotations
{
    public sealed class PastDateAttribute : ValidationAttribute
    {
        public PastDateAttribute()
        {
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

            DateTime dt = (DateTime)value;
            if (dt <= DateTime.Now)
            {
                return true;
            }
            return false;
        }
    }
}