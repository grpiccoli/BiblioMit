﻿using BiblioMit.Models.Entities.Digest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public abstract class SernapescaDeclaration : Indexed
    {
        [ParseSkip]
        public int Id { get; set; }
        [ParseSkip, Required]
        public int EntryId { get; set; }
        [ParseSkip]
        public virtual SernapescaEntry? Entry { get; set; }
        [ParseSkip, Required]
        public DeclarationType Discriminator { get; set; }
        //Tons
        [NotMapped]
        public double Weight { get; set; }//2
        [NotMapped]
        public string? CommuneName { get; set; }
        [NotMapped]
        public int Year//4
        {
            get
            {
                return Date.Year;
            }
            set
            {
                Date = Date.AddYears(value - Date.Year);
            }
        }
        [NotMapped]
        public int Month//5
        {
            get
            {
                return Date.Month;
            }
            set
            {
                Date = Date.AddMonths(value - Date.Month);
            }
        }
        [NotMapped, ParseSkip]
        public DateTime Date { get; set; }
        public int DeclarationNumber { get; set; }//1
        [ParseSkip]
        public int OriginPsmbId { get; set; }
        public virtual Psmb? OriginPsmb { get; set; }//0
        [ParseSkip]
        public virtual ICollection<DeclarationDate> DeclarationDates { get; } = new List<DeclarationDate>();
        public override bool Equals(object? obj)
        {
            if (obj is null) return this is null;
            if (this is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is SernapescaDeclaration q
            && Id == q.Id
            && Discriminator == q.Discriminator
            && OriginPsmbId == q.OriginPsmbId
            && DeclarationNumber == q.DeclarationNumber;
        }
        public static bool operator ==(SernapescaDeclaration? x, SernapescaDeclaration? y)
        {
            if (x is null) return y is null;
            if (y is null) return x is null;
            return x.Equals(y);
        }
        public static bool operator !=(SernapescaDeclaration? x, SernapescaDeclaration? y) => !(x == y);
        public override int GetHashCode() => HashCode.Combine(Id);
        public bool Equals(SernapescaDeclaration other)
        {
            if (other is null) return this is null;
            if (this is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id)
            && Id.Equals(other.Id)
            && Discriminator.Equals(other.Discriminator)
            && OriginPsmbId.Equals(other.OriginPsmbId)
            && DeclarationNumber.Equals(other.DeclarationNumber);
        }
    }
}
