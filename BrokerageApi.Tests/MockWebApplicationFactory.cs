using System;
using System.Data.Common;
using BrokerageApi.V1.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BrokerageApi.Tests
{
    public class MockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly DbContextOptionsBuilder _builder;

        public MockWebApplicationFactory(DbContextOptionsBuilder builder)
        {
            _builder = builder;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                Context = new BrokerageContext(_builder.Options);
                services.AddSingleton(Context);

                var serviceProvider = services.BuildServiceProvider();
                var dbContext = serviceProvider.GetRequiredService<BrokerageContext>();

                dbContext.Database.Migrate();
            });
        }

        public BrokerageContext Context { get; set; }
    }
}
