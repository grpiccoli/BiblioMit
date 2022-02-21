using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class AreaCodeProvince
    {
        public int ProvinceId { get; set; }
        [AllowNull]
        public virtual Province Province { get; set; }
        public int AreaCodeId { get; set; }
        [AllowNull]
        public virtual AreaCode AreaCode { get; set; }
    }
}
