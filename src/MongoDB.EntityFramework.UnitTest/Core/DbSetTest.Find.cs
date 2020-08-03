using System.Threading.Tasks;
using AutoFixture;
using MongoDB.EntityFramework.UnitTest.Fakes;
using Moq;
using Xunit;

namespace MongoDB.EntityFramework.UnitTest.Core
{
    public partial class DbSetTest
    {
        [Fact]
        public async Task Find_ValidId_ValidEntity()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var expected = _fixture.Create<EntityFake>();
            _contextMock
                .Setup(x => x.FindAsync<EntityFake>(id, default))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var actual = await _target.FindAsync(id);

            // Assert
            Assert.Equal(expected, actual);
            _contextMock.Verify();
        }

        [Fact]
        public async Task Find_InvalidId_Default()
        {
            // Arrange
            var id = _fixture.Create<int>();
            var expected = default(EntityFake);
            _contextMock
                .Setup(x => x.FindAsync<EntityFake>(id, default))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            var actual = await _target.FindAsync(id);

            // Assert
            Assert.Equal(expected, actual);
            _contextMock.Verify();
        }
    }
}