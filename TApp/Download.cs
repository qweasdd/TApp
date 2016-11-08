namespace TApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Download
    {
        public int Id { get; set; }

        public long RepositoryID { get; set; }

        [Required]
        [StringLength(500)]
        public string Path { get; set; }

        public string Content { get; set; }
    }
}
