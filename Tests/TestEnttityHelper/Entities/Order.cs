using System.ComponentModel.DataAnnotations.Schema;

namespace TestEH_UnitTest.Entities;

[Table("TB_ORDER", Schema = "TEST")]
public class Order
{
    public long Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public ICollection<Item> Items { get; set; } = new List<Item>(); // 1:N
}