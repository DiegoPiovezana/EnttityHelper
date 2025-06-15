using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entities;

[Table("TB_ITEM", Schema = "TEST")]
public class Item
{
    [Key] public long Id { get; set; }
    [Required] public string Name { get; set; }
    public string? Description { get; set; }
}