using BrokerageApi.V1.Infrastructure;
using Moq;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class MockDbSaver : Mock<IDbSaver>
    {
        public void VerifyChangesSaved()
        {
            Verify(x => x.SaveChangesAsync(), Times.Once());
        }
        public void VerifyChangesNotSaved()
        {
            Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}
