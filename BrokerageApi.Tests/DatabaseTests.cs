using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Testing;

namespace BrokerageApi.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private IDbContextTransaction _transaction;
        private IClockService _clock;
        protected Fixture Fixture { get; set; }
        protected BrokerageContext BrokerageContext { get; private set; }
        protected Instant CurrentInstant => _clock.Now;
        protected Instant PreviousInstant => _clock.Now - Duration.FromHours(2);
        protected LocalDate CurrentDate => _clock.Today;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            ConfigureJsonSerializer();

            Fixture = FixtureHelpers.Fixture;
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

        protected async Task<(Provider provider, Service service)> SeedProviderAndService()
        {
            var provider = Fixture.BuildProvider().Create();
            var service = Fixture.BuildService().Create();

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            return (provider, service);
        }

        protected async Task<ElementType> SeedElementType(int serviceId)
        {
            var elementType = Fixture.BuildElementType(serviceId).Create();

            await BrokerageContext.ElementTypes.AddAsync(elementType);
            await BrokerageContext.SaveChangesAsync();

            return elementType;
        }

        private static void ConfigureJsonSerializer()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                settings.Converters.Add(new StringEnumConverter());

                return settings;
            };
        }
    }
}
