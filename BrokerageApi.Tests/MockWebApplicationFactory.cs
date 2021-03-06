using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BrokerageApi.Tests
{
    public class MockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly DbContextOptionsBuilder _builder;
        private readonly IClockService _clock;

        public MockWebApplicationFactory(DbContextOptionsBuilder builder, IClockService clock)
        {
            _builder = builder;
            _clock = clock;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                Context = new BrokerageContext(_builder.Options, _clock);
                services.AddSingleton(Context);
                services.AddSingleton(_clock);

                var serviceProvider = services.BuildServiceProvider();
                var dbContext = serviceProvider.GetRequiredService<BrokerageContext>();

                dbContext.Database.Migrate();
            });
        }

        public BrokerageContext Context { get; set; }
    }
}
