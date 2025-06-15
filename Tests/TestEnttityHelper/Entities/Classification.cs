using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entities
{
    // Example of a entity that will be mapped but not exist in the database.

    [Table("TB_CLASSIFICATION", Schema = "TEST")]
    internal class Classification
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }


}
