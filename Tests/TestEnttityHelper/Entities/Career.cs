using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entities
{
    [Table("TB_CAREERS", Schema = "TEST")]
    public class Career
    {
        [Key()] public Int64 IdCareer { get; set; }
        [Required][MaxLength(200)] public string Name { get; set; }
        [Required] public double CareerLevel { get; internal set; }
        [Required] public bool Active { get; set; }



        public Career() { }

        public Career(Int64 idCareer, string name)
        {
            IdCareer = idCareer;
            Name = name;
            Active = true;
        }

        public override string ToString()
        {
            return Name;
        }


    }
}
