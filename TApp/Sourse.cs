namespace TApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Sourse")]
    public partial class Sourse
    {
        public Sourse()
        {
            Languages = new HashSet<Language>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RepositoryID { get; set; }

        [Required]
        [StringLength(200)]
        public string Url { get; set; }
        
        public virtual ICollection<Language> Languages { get; set; }
    }
}
