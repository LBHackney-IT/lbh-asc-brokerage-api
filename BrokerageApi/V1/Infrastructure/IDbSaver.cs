using System.Threading.Tasks;

namespace BrokerageApi.V1.Infrastructure
{
    public interface IDbSaver
    {
        public Task SaveChangesAsync();
    }
}
