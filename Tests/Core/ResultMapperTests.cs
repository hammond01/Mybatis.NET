using System.Data;
using Moq;
using MyBatis.NET.Core;
using Xunit;

namespace MyBatis.NET.Tests.Core;

public class ResultMapperTests
{
    public class TestUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Age { get; set; }
        public bool IsActive { get; set; }
        public decimal Salary { get; set; }
    }

    [Fact]
    public void MapToList_BasicMapping_ShouldWork()
    {
        // Arrange
        var readerMock = new Mock<IDataReader>();
        
        // Setup columns
        readerMock.Setup(r => r.FieldCount).Returns(5);
        readerMock.Setup(r => r.GetName(0)).Returns("Id");
        readerMock.Setup(r => r.GetName(1)).Returns("Name");
        readerMock.Setup(r => r.GetName(2)).Returns("Age");
        readerMock.Setup(r => r.GetName(3)).Returns("IsActive");
        readerMock.Setup(r => r.GetName(4)).Returns("Salary");

        // Setup data rows
        var readSequence = readerMock.SetupSequence(r => r.Read());
        readSequence.Returns(true); // Row 1
        readSequence.Returns(false); // End

        // Row 1 values
        readerMock.Setup(r => r.GetValue(0)).Returns(1);
        readerMock.Setup(r => r.GetValue(1)).Returns("John");
        readerMock.Setup(r => r.GetValue(2)).Returns(30);
        readerMock.Setup(r => r.GetValue(3)).Returns(true);
        readerMock.Setup(r => r.GetValue(4)).Returns(1000.50m);

        // Act
        var result = ResultMapper.MapToList<TestUser>(readerMock.Object);

        // Assert
        Assert.Single(result);
        var user = result[0];
        Assert.Equal(1, user.Id);
        Assert.Equal("John", user.Name);
        Assert.Equal(30, user.Age);
        Assert.True(user.IsActive);
        Assert.Equal(1000.50m, user.Salary);
    }

    [Fact]
    public void MapToList_WithNullValues_ShouldHandleNullable()
    {
        // Arrange
        var readerMock = new Mock<IDataReader>();
        
        readerMock.Setup(r => r.FieldCount).Returns(2);
        readerMock.Setup(r => r.GetName(0)).Returns("Id");
        readerMock.Setup(r => r.GetName(1)).Returns("Age");

        var readSequence = readerMock.SetupSequence(r => r.Read());
        readSequence.Returns(true).Returns(false);

        readerMock.Setup(r => r.GetValue(0)).Returns(1);
        readerMock.Setup(r => r.GetValue(1)).Returns(DBNull.Value);

        // Act
        var result = ResultMapper.MapToList<TestUser>(readerMock.Object);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Null(result[0].Age);
    }

    [Fact]
    public void MapToList_TypeMismatch_ShouldConvert()
    {
        // Arrange: DB returns double, Property is decimal
        var readerMock = new Mock<IDataReader>();
        
        readerMock.Setup(r => r.FieldCount).Returns(1);
        readerMock.Setup(r => r.GetName(0)).Returns("Salary");

        var readSequence = readerMock.SetupSequence(r => r.Read());
        readSequence.Returns(true).Returns(false);

        readerMock.Setup(r => r.GetValue(0)).Returns(2000.0d); // Double

        // Act
        var result = ResultMapper.MapToList<TestUser>(readerMock.Object);

        // Assert
        Assert.Single(result);
        Assert.Equal(2000.0m, result[0].Salary);
    }

    [Fact]
    public void MapToList_CaseInsensitiveParams_ShouldWork()
    {
        // Arrange
        var readerMock = new Mock<IDataReader>();
        
        readerMock.Setup(r => r.FieldCount).Returns(2);
        readerMock.Setup(r => r.GetName(0)).Returns("ID"); // Uppercase in DB
        readerMock.Setup(r => r.GetName(1)).Returns("name"); // Lowercase in DB

        var readSequence = readerMock.SetupSequence(r => r.Read());
        readSequence.Returns(true).Returns(false);

        readerMock.Setup(r => r.GetValue(0)).Returns(10);
        readerMock.Setup(r => r.GetValue(1)).Returns("Alice");

        // Act
        var result = ResultMapper.MapToList<TestUser>(readerMock.Object);

        // Assert
        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
        Assert.Equal("Alice", result[0].Name);
    }
}
