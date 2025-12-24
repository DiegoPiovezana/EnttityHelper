
namespace TestEH.Entities
{
    // Example of a entity that will be mapped but not exist in the database.

    internal class Classification
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }


}
