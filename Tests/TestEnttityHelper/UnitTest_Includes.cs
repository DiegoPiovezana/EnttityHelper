using EH;
using System.Diagnostics.Metrics;

namespace TestEH_UnitTest
{
    [TestFixture]
    public class EntityInclusionTests
    {
        private EnttityHelper _enttityHelper;

        [SetUp]
        public void Setup()
        {
            //_enttityHelper = new EnttityHelper("Data Source=localhost:1521/xe;User Id=system;Password=oracle");
            _enttityHelper = new EnttityHelper("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;");
        }

        [Test]
        public void IncludeAll_SingleEntityWithFK_ReturnsTrue()
        {
            // Arrange
            TestEntity entity = new TestEntity
            {
                Id = 1,
                Name = "Test Entity",
                ForeignEntity = new ForeignEntity { Id = 1, Description = "Foreign Entity" },
                RelatedEntities = new List<RelatedEntity>
                {
                    new RelatedEntity { Id = 1, Details = "Related Entity 1", TestEntityId = 1 },
                    new RelatedEntity { Id = 2, Details = "Related Entity 2", TestEntityId = 1 }
                }
            };

            // Act
            bool isIncluded = _enttityHelper.IncludeAll(entity);

            // Assert
            Assert.IsTrue(isIncluded);
        }

        [Test]
        public void IncludeAll_EmptyEntityList_ReturnsFalse()
        {
            // Act
            bool isIncluded = _enttityHelper.IncludeAll(new List<TestEntity>());

            // Assert
            Assert.IsFalse(isIncluded);
        }       

        [Test]
        public void IncludeEntityFK_ValidEntityAndForeignKey_ReturnsTrue()
        {
            // Arrange
            TestEntity entity = new TestEntity
            {
                Id = 1,
                Name = "Test Entity",
                ForeignEntity = new ForeignEntity { Id = 1, Description = "Foreign Entity" }
            };
            string fkName = "ForeignEntityId";

            // Act
            bool isIncluded = _enttityHelper.IncludeEntityFK(entity, fkName);

            // Assert
            Assert.IsTrue(isIncluded);
        }

        [Test]
        public void IncludeEntityFK_NullEntity_ReturnsFalse()
        {
            // Arrange
            string fkName = "ForeignEntityId";

            // Act
            bool isIncluded = _enttityHelper.IncludeEntityFK<TestEntity>(null, fkName);

            // Assert
            Assert.IsFalse(isIncluded);
        }

        [Test]
        public void IncludeInverseEntity_ValidEntityAndInverseProperty_ReturnsTrue()
        {
            // Arrange
            TestEntity entity = new TestEntity
            {
                Id = 1,
                Name = "Test Entity",
                RelatedEntities = new List<RelatedEntity>
                {
                    new RelatedEntity { Id = 1, Details = "Related Entity 1", TestEntityId = 1 },
                    new RelatedEntity { Id = 2, Details = "Related Entity 2", TestEntityId = 1 }
                }
            };
            string inversePropertyName = "RelatedEntities";

            // Act
            bool isIncluded = _enttityHelper.IncludeInverseEntity(entity, inversePropertyName);

            // Assert
            Assert.IsTrue(isIncluded);
        }

        [Test]
        public void IncludeInverseEntity_NullEntity_ReturnsFalse()
        {
            // Arrange
            string inversePropertyName = "RelatedEntities";

            // Act
            bool isIncluded = _enttityHelper.IncludeInverseEntity<TestEntity>(null, inversePropertyName);

            // Assert
            Assert.IsFalse(isIncluded);
        }
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Foreign key property
        public int ForeignEntityId { get; set; }

        // Navigation property to represent the relationship
        public ForeignEntity ForeignEntity { get; set; }

        // Inverse property to test IncludeInverseEntity
        public ICollection<RelatedEntity> RelatedEntities { get; set; } = new List<RelatedEntity>();
    }

    public class ForeignEntity
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class RelatedEntity
    {
        public int Id { get; set; }
        public string Details { get; set; }

        // Inverse property pointing back to TestEntity
        public int TestEntityId { get; set; }
        public TestEntity TestEntity { get; set; }
    }
}
