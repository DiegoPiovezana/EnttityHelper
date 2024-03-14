using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiegoPiov.UserManagement
{
    [Table("TB_CAREERS")]
    public class Career 
    {
        [Key()] public Int64 IdCareer { get; set; }
        //[Required][MaxLength(200)][Index(IsUnique = true)] public string Name { get; set; }
        [Required][MaxLength(200)] public string Name { get; set; }
        [Required] public double CareerLevel { get; internal set; } 
        [Required] public bool Active { get; set; }


        public override string ToString()
        {
            return Name;
        }
       

    }
}
