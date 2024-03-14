using System.ComponentModel.DataAnnotations.Schema;

namespace TestEnttityHelper.OthersEntity
{
    [Table("TB_ENTITY_TEST", Schema = "SYSTEM")]
    public class EntityTest
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
    }
}
