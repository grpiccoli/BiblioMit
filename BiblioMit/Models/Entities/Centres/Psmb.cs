﻿using BiblioMit.Extensions;
using BiblioMit.Models.Entities.Centres;
using BiblioMit.Models.Entities.Variables;
using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public abstract class Psmb : Indexed, IEquatable<Psmb>
    {
        //COMMON ATTRIBUTES
        #region Common attributes
        public int Id { get; set; }
        [Display(Name = "Code")]
        [DisplayFormat(DataFormatString = "{0,7:N0}")]
        public int Code { get; set; }
        [Range(101101, 116305)]
        public int? CommuneId { get; set; }
        [AllowNull]
        public virtual Commune Commune { get; set; }
        public PsmbType Discriminator { get; set; }
        public string? Name { get; private set; }
        public void SetName(string value)
        {
            NormalizedName = value?.RemoveDiacritics()?.ToUpperInvariant();
            Name = value;
        }
        public string? NormalizedName { get; private set; }
        [Display(Name = "Address")]
        public string? Address { get; set; }
        #endregion
        //COMPARISONS
        #region Comparison
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return this is null;
            }

            if (this is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is Psmb q
                && q.Id == Id
                && q.Discriminator == Discriminator;
        }
        public static bool operator ==([AllowNull] Psmb? x, [AllowNull] Psmb? y)
        {
            if (x is null)
            {
                return y is null;
            }

            return x.Equals(y);
        }
        public static bool operator !=([AllowNull] Psmb? x, [AllowNull] Psmb? y) => !(x == y);
        public override int GetHashCode() => HashCode.Combine(Id, Discriminator);
        public bool Equals([AllowNull] Psmb? other)
        {
            if (other is null)
            {
                return this is null;
            }

            if (this is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id)
            && Discriminator.Equals(other.Discriminator);
        }
        #endregion
        //FARM RESEARCH CRAFT PLANT
        [Display(Name = "RUT")]
        [DisplayFormat(DataFormatString = "{0,9:N0}")]
        public int? CompanyId { get; set; }
        [Display(Name = "Company")]
        [AllowNull]
        public virtual Company Company { get; set; }
        [Display(Name = "Contacts")]
        public virtual ICollection<Contact> Contacts { get; } = new List<Contact>();
        //AREA FARM
        [Display(Name = "Acronym")]
        public string? Acronym { get; private set; }
        public void SetAcronym(string value) => Acronym = value?.ParseAcronym();
        public virtual ICollection<PlanktonAssay> PlanktonAssays { get; } = new List<PlanktonAssay>();
        //CRAFT FARM
        [Display(Name = "Water body type")]
        public WaterBody? WaterBody { get; set; }
        //AREA BED FARM
        public int? PolygonId { get; set; }
        [AllowNull]
        public virtual Polygon Polygon { get; set; }
        //CRAFT FARM PLANT
        public virtual ICollection<SernapescaDeclaration> Declarations { get; } = new List<SernapescaDeclaration>();
        //BED FARM
        [Display(Name = "Samplings")]
        public virtual ICollection<Sampling> Samplings { get; } = new List<Sampling>();
        public virtual ICollection<Variable> Variables { get; } = new List<Variable>();
        public string GetFullName() => string.Join(",", new string?[] { Code.ToString(), Name, Commune?.GetFullName() });
    }
}
