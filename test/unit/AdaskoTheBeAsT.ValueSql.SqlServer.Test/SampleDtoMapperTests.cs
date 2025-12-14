using System;
using System.Data;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AdaskoTheBeAsT.ValueSql.SqlServer.Test;

public class SampleDtoMapperTests
{
    [Fact]
    public void Map_ShouldMapAllProperties_WhenAllValuesPresent()
    {
        // Arrange
        var reader = Substitute.For<IDataReader>();
        var expectedId = 42;
        var expectedName = "Test Name";
        var expectedDesc = "Test Description";
        var expectedPrice = 99.99m;
        var expectedIsActive = true;
        var expectedCreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var expectedExternalId = Guid.NewGuid();

        reader.IsDBNull(Arg.Any<int>()).Returns(false);
        reader.GetInt32(0).Returns(expectedId);
        reader.GetString(1).Returns(expectedName);
        reader.GetString(2).Returns(expectedDesc);
        reader.GetDecimal(3).Returns(expectedPrice);
        reader.GetBoolean(4).Returns(expectedIsActive);
        reader.GetDateTime(5).Returns(expectedCreatedAt);
        reader.GetGuid(6).Returns(expectedExternalId);

        var mapper = default(SampleDtoMapper);

        // Act
        var result = mapper.Map(reader);

        // Assert
        result.Id.Should().Be(expectedId);
        result.Name.Should().Be(expectedName);
        result.Desc.Should().Be(expectedDesc);
        result.Price.Should().Be(expectedPrice);
        result.IsActive.Should().Be(expectedIsActive);
        result.CreatedAt.Should().Be(expectedCreatedAt);
        result.ExternalId.Should().Be(expectedExternalId);
    }

    [Fact]
    public void Map_ShouldHandleNullValues_ForNullableProperties()
    {
        // Arrange
        var reader = Substitute.For<IDataReader>();
        reader.IsDBNull(0).Returns(false);
        reader.IsDBNull(1).Returns(false);
        reader.IsDBNull(2).Returns(true);
        reader.IsDBNull(3).Returns(false);
        reader.IsDBNull(4).Returns(false);
        reader.IsDBNull(5).Returns(false);
        reader.IsDBNull(6).Returns(true);

        reader.GetInt32(0).Returns(1);
        reader.GetString(1).Returns("Name");
        reader.GetDecimal(3).Returns(10m);
        reader.GetBoolean(4).Returns(false);
        reader.GetDateTime(5).Returns(DateTime.UtcNow);

        var mapper = default(SampleDtoMapper);

        // Act
        var result = mapper.Map(reader);

        // Assert
        result.Desc.Should().BeNull();
        result.ExternalId.Should().BeNull();
    }
}
