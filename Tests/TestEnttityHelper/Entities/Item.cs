using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entities;

[Table("TB_ITEM", Schema = "TEST")]
public class Item
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public long OrderId { get; set; } // Foreign key to Order
    public Order Order { get; set; } // N:1 relationship
}