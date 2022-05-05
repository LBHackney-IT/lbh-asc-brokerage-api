using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using NodaTime;
using NodaTime.Testing;

namespace BrokerageApi.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private IDbContextTransaction _transaction;
        private IClockService _clock;
        protected BrokerageContext BrokerageContext { get; private set; }
        protected Instant CurrentInstant => _clock.Now;
        protected Instant PreviousInstant => _clock.Now - Duration.FromHours(2);
        protected LocalDate CurrentDate => _clock.Today;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseNpgsql(ConnectionString.TestDatabase())
                .UseSnakeCaseNamingConvention();

            var currentTime = SystemClock.Instance.GetCurrentInstant();
            var fakeClock = new FakeClock(currentTime);
            _clock = new ClockService(fakeClock);

            BrokerageContext = new BrokerageContext(builder.Options, _clock);
            BrokerageContext.Database.Migrate();
            _transaction = BrokerageContext.Database.BeginTransaction();
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
    }
}
