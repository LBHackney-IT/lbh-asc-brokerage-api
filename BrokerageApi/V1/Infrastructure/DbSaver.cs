using System.Threading.Tasks;

namespace BrokerageApi.V1.Infrastructure
{
    public class DbSaver : IDbSaver
    {
        private readonly BrokerageContext _context;
        public DbSaver(BrokerageContext context)
        {
            _context = context;
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
