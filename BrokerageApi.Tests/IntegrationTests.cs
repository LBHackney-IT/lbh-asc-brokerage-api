using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Npgsql;
using NUnit.Framework;
using NodaTime;
using NodaTime.Testing;

namespace BrokerageApi.Tests
{
    public class IntegrationTests<TStartup> where TStartup : class
    {
        protected HttpClient Client { get; private set; }
        protected BrokerageContext Context => _factory.Context;
        protected Instant CurrentInstant => _clock.Now;
        protected Instant PreviousInstant => _clock.Now - Duration.FromHours(2);
        protected LocalDate CurrentDate => _clock.Today;

        private MockWebApplicationFactory<TStartup> _factory;
        private IDbContextTransaction _transaction;
        private DbContextOptionsBuilder _builder;
        private IClockService _clock;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _builder = new DbContextOptionsBuilder();
            _builder.UseNpgsql(ConnectionString.TestDatabase(), o => o.UseNodaTime())
                .UseSnakeCaseNamingConvention();
        }

        [SetUp]
        public void BaseSetup()
        {
            var currentTime = SystemClock.Instance.GetCurrentInstant();
            var fakeClock = new FakeClock(currentTime);
            _clock = new ClockService(fakeClock);

            _factory = new MockWebApplicationFactory<TStartup>(_builder, _clock);
            Client = _factory.CreateClient();

            _factory.Context.Database.Migrate();
            _transaction = _factory.Context.Database.BeginTransaction();

            if (IsAuthenticated())
            {
                SetupAuthentication(AsUser());
            }
        }

        [TearDown]
        public void BaseTearDown()
        {
            Client.Dispose();
            _factory.Dispose();
            _transaction.Rollback();
            _transaction.Dispose();
        }

        public async Task<(HttpStatusCode statusCode, TResponse response)> Get<TResponse>(string address)
        {
            var result = await InternalGet(address);

            var response = await ProcessResponse<TResponse>(result);

            return (result.StatusCode, response);
        }

        public async Task<HttpStatusCode> Get(string address)
        {
            var result = await InternalGet(address);

            return result.StatusCode;
        }

        public async Task<(HttpStatusCode statusCode, TResponse response)> Post<TResponse>(string address, object data)
        {
            HttpResponseMessage result = await InternalPost(address, data);

            TResponse response = await ProcessResponse<TResponse>(result);
            return (result.StatusCode, response);
        }

        public async Task<HttpStatusCode> Post(string address, object data)
        {
            HttpResponseMessage result = await InternalPost(address, data);
            return result.StatusCode;
        }

        public async Task<(HttpStatusCode statusCode, TResponse response)> Delete<TResponse>(string address)
        {
            HttpResponseMessage result = await InternalDelete(address);

            TResponse response = await ProcessResponse<TResponse>(result);
            return (result.StatusCode, response);
        }

        public async Task<HttpStatusCode> Delete(string address)
        {
            HttpResponseMessage result = await InternalDelete(address);
            return result.StatusCode;
        }

        private async Task<HttpResponseMessage> InternalGet(string uri)
        {
            var result = await Client.GetAsync(new Uri(uri, UriKind.Relative));
            return result;
        }

        private async Task<HttpResponseMessage> InternalPost(string uri, object data)
        {
            var serializedContent = JsonConvert.SerializeObject(data);
            var content = new StringContent(serializedContent, Encoding.UTF8, "application/json");

            var result = await Client.PostAsync(new Uri(uri, UriKind.Relative), content);
            content.Dispose();
            return result;
        }

        private async Task<HttpResponseMessage> InternalDelete(string uri)
        {
            var result = await Client.DeleteAsync(new Uri(uri, UriKind.Relative));
            return result;
        }

        private void SetupAuthentication(string user)
        {
            switch (user)
            {
                case "Referrer":
                    SetAuthorizationHeader(GenerateToken("saml-socialcare-corepathwayspilot"));
                    break;

                case "Broker":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    CreateApiUser(UserRole.Broker);
                    break;

                case "BrokerageAssistant":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    CreateApiUser(UserRole.BrokerageAssistant);
                    break;
            }
        }

        private void SetAuthorizationHeader(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private void CreateApiUser(UserRole role)
        {
            var user = new User()
            {
                Name = "Api User",
                Email = "api.user@hackney.gov.uk",
                Roles = new List<UserRole>() { role },
                IsActive = true
            };

            Context.Users.Add(user);
            Context.SaveChanges();
        }

        private static async Task<TResponse> ProcessResponse<TResponse>(HttpResponseMessage result)
        {
            var responseContent = await result.Content.ReadAsStringAsync();

            try
            {
                var parseResponse = JsonConvert.DeserializeObject(responseContent, typeof(TResponse));
                var castedResponse = parseResponse is TResponse response ? response : default;
                return castedResponse;
            }
            catch (Exception e) when (e is JsonSerializationException || e is JsonReaderException)
            {
                throw new Exception($"Result Serialisation Failed. Response Had Code {result.StatusCode}, Response: {responseContent}", e);
            }
        }

        private static bool IsAuthenticated()
        {
            return TestContext.CurrentContext.Test.Properties.ContainsKey("AsUser");
        }

        private static string AsUser()
        {
            return (string) TestContext.CurrentContext.Test.Properties.Get("AsUser");
        }

        private static string GenerateToken(string group)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("super-secret-token"));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("sub", "123456789012345678901"),
                    new Claim("email", "api.user@hackney.gov.uk"),
                    new Claim("name", "Api User"),
                    new Claim("groups", "HackneyAll"),
                    new Claim("groups", group)
                }),
                Issuer = "Hackney",
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
