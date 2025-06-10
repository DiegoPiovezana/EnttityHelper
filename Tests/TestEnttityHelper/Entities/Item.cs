using System.ComponentModel.DataAnnotations;

namespace TestEH_UnitTest.Entities;

public class Item
{
    [Key] public long Id { get; set; }
    [Required] public string Name { get; set; }
    public string? Description { get; set; }
}