﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.ViewModels
{
    public class IndividualSoftTissueViewModel
    {
        [Display(Name = "Subject Id")]
        public int Id { get; set; }

        [Display(Name = "Sample Id")]
        public int SamplingId { get; set; }

        [Display(Name = "Type of Soft Tissue Finding")]
        public SoftType SoftType { get; set; }

        [Display(Name = "Tissues")]
        public Collection<TissueView> Tissues { get; } = new Collection<TissueView>();

        [Display(Name = "Count")]
        [Range(1, 1000, ErrorMessage = "This value must be between {0} and {1}")]
        public int? Count { get; set; }

        public Dictionary<string, bool> Configs { get; } = new Dictionary<string, bool>();

        [Display(Name = "Presence")]
        public bool Check { get; set; }
    }
}
