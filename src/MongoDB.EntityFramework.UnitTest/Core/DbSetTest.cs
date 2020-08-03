using AutoFixture;
using MongoDB.EntityFramework.Core;
using MongoDB.EntityFramework.UnitTest.Fakes;
using Moq;

namespace MongoDB.EntityFramework.UnitTest.Core
{
    public partial class DbSetTest
    {
        private Fixture _fixture;
        private Mock<IDbContext> _contextMock;
        private DbSet<EntityFake> _target;

        public DbSetTest()
        {
            _fixture = new Fixture();
            _contextMock = new Mock<IDbContext>();
            _target = new DbSet<EntityFake>(_contextMock.Object);
        }
    }
}