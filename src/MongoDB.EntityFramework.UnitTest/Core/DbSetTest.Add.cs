using AutoFixture;
using MongoDB.EntityFramework.UnitTest.Fakes;
using Xunit;

namespace MongoDB.EntityFramework.UnitTest.Core
{
    public partial class DbSetTest
    {
        [Fact]
        public void Add_ValidEntity()
        {
            // Arrange
            var expected = _fixture.Create<EntityFake>();
            _contextMock
                .Setup(x => x.Add<EntityFake>(expected))
                .Verifiable();

            // Act
            _target.Add(expected);

            // Assert
            _contextMock.Verify();
        }

        [Fact]
        public void Add_Default()
        {
            // Arrange
            var expected = default(EntityFake);
            _contextMock
                .Setup(x => x.Add<EntityFake>(expected))
                .Verifiable();

            // Act
            _target.Add(expected);

            // Assert
            _contextMock.Verify();
        }
    }
}