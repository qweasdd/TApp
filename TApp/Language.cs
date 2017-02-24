namespace TApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Language
    {
        public Language()
        {
            Sourses = new HashSet<Sourse>();
        }

        [Key]
        [Column("Language")]
        [StringLength(50)]
        public string Language1 { get; set; }

        [Required]
        [StringLength(100)]
        public string Extentions { get; set; }
        
        public virtual ICollection<Sourse> Sourses { get; set; }
    }
}
