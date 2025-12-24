using System.ComponentModel.DataAnnotations.Schema;

namespace TestEnttityHelper.OthersEntity
{
    [Table("TB_ENTITY_TEST", Schema = "TEST")]
    public class EntityTest
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        
        public double TestDouble { get; set; } = 10.5;  // Db => NUMBER(*) - OK

        public bool TestBoolean { get; set; } = true; // Db => NUMBER(1) - !!!
    }
}
