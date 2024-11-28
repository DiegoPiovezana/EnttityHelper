using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using EH;
using TestEH.Entities;

namespace TestXUnit
{
    public class EHTests
    {
        private readonly Mock<EnttityHelper> _mockEntityHelper;
        private readonly EnttityHelper _entityHelper;

        public EHTests()
        {
            _mockEntityHelper = new Mock<EnttityHelper>();
            _entityHelper = new EnttityHelper("Data Source=localhost:1521/xe;User Id=system;Password=oracle");
        }



        //[Fact]
        public void Get_ShouldReturnPagedData_WhenPageSizeAndPageIndexAreProvided()
        {
            // Arrange
            var pageSize = 10;
            var pageIndex = 1;
            var expectedEntities = new List<User>
            {
                new User { Id = 1, Name = "Entity 1" },
                new User { Id = 2, Name = "Entity 2" }
            };

            // Mock the Get method to return a paged result
            _mockEntityHelper.Setup(helper => helper.Get<User>(true, null, null, pageSize, pageIndex, null, true))
                .Returns(expectedEntities);

            // Act
            var result = _entityHelper.Get<User>(true, null, null, pageSize, pageIndex, null, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Entity 1", result[0].Name);
        }

        //[Fact]
        public void Get_ShouldReturnEmptyList_WhenNoEntitiesMatch()
        {
            // Arrange
            var pageSize = 10;
            var pageIndex = 1;
            var expectedEntities = new List<User>();

            // Mock the Get method to return an empty list
            _mockEntityHelper.Setup(helper => helper.Get<User>(true, null, null, pageSize, pageIndex, null, true))
                .Returns(expectedEntities);

            // Act
            var result = _entityHelper.Get<User>(true, null, null, pageSize, pageIndex, null, true);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }




        //[Fact]
        public void ExecuteSelect_ShouldReturnPagedEntities_WhenPageSizeAndPageIndexAreProvided()
        {
            // Arrange
            var query = "SELECT * FROM User";
            var pageSize = 10;
            var pageIndex = 1;
            var expectedEntities = new List<User>
                {
                    new User { Id = 1, Name = "Entity 1" },
                    new User { Id = 2, Name = "Entity 2" }
                };

            // Mock the ExecuteSelect method to return the expected entities
            _mockEntityHelper.Setup(helper => helper.ExecuteSelect<User>(query, pageSize, pageIndex, null, null, true))
                .Returns(expectedEntities);

            // Act
            var result = _entityHelper.ExecuteSelect<User>(query, pageSize, pageIndex, null, null, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Entity 1", result[0].Name);
        }

        //[Fact]
        public void ExecuteSelect_ShouldReturnNull_WhenQueryFails()
        {
            // Arrange
            var query = "SELECT * FROM User WHERE 1 = 0"; // Invalid query
            var pageSize = 10;
            var pageIndex = 1;

            // Mock the ExecuteSelect method to return null
            _mockEntityHelper.Setup(helper => helper.ExecuteSelect<User>(query, pageSize, pageIndex, null, null, true))
                .Returns((List<User>?)null);

            // Act
            var result = _entityHelper.ExecuteSelect<User>(query, pageSize, pageIndex, null, null, true);

            // Assert
            Assert.Null(result);
        }



        //[Fact]
        public void ExecuteSelectDt_ShouldReturnPagedDataTable_WhenPageSizeAndPageIndexAreProvided()
        {
            // Arrange
            var query = "SELECT * FROM User";
            var pageSize = 10;
            var pageIndex = 1;
            var expectedDataTable = new DataTable();
            expectedDataTable.Columns.Add("Id");
            expectedDataTable.Columns.Add("Name");
            expectedDataTable.Rows.Add(1, "Entity 1");
            expectedDataTable.Rows.Add(2, "Entity 2");

            // Mock the ExecuteSelectDt method to return the expected DataTable
            _mockEntityHelper.Setup(helper => helper.ExecuteSelectDt(query, pageSize, pageIndex, null, null, true))
                .Returns(expectedDataTable);

            // Act
            var result = _entityHelper.ExecuteSelectDt(query, pageSize, pageIndex, null, null, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
            Assert.Equal("Entity 1", result.Rows[0]["Name"]);
        }

        //[Fact]
        public void ExecuteSelectDt_ShouldReturnNull_WhenQueryFails()
        {
            // Arrange
            var query = "SELECT * FROM User WHERE 1 = 0"; // Invalid query
            var pageSize = 10;
            var pageIndex = 1;

            // Mock the ExecuteSelectDt method to return null
            _mockEntityHelper.Setup(helper => helper.ExecuteSelectDt(query, pageSize, pageIndex, null, null, true))
                .Returns((DataTable?)null);

            // Act
            var result = _entityHelper.ExecuteSelectDt(query, pageSize, pageIndex, null, null, true);

            // Assert
            Assert.Null(result);
        }


    }
}
