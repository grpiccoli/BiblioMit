﻿using BiblioMit.Models.Entities.Semaforo;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class Larva
    {
        public int Id { get; set; }
        public int LarvaeId { get; set; }
        [AllowNull]
        public virtual Larvae Larvae { get; set; }
        public int SpecieId { get; set; }
        [AllowNull]
        public Specie Specie { get; set; }
        public LarvaType LarvaType { get; set; }
        [Display(Description = "Larvas/m3")]
        public double Count { get; set; }
    }
}
