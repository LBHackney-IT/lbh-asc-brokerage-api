using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using NodaTime;
using NodaTime.Serialization.JsonNet;
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
        protected User ApiUser => _apiUser;
        private MockWebApplicationFactory<TStartup> _factory;
        private IDbContextTransaction _transaction;
        private DbContextOptionsBuilder _builder;
        private IClockService _clock;
        private User _apiUser;

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
            ConfigureJsonSerializer();

            var currentTime = SystemClock.Instance.GetCurrentInstant();
            var fakeClock = new FakeClock(currentTime);
            _clock = new ClockService(fakeClock);

            _factory = new MockWebApplicationFactory<TStartup>(_builder, _clock);
            Client = _factory.CreateClient();

            _factory.Context.Database.Migrate();
            _transaction = _factory.Context.Database.BeginTransaction();

            if (IsAuthenticated())
            {
                SetupAuthentication(AsUser(), WithApprovalLimit());
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

        private void SetupAuthentication(string user, decimal? withApprovalLimit)
        {
            switch (user)
            {
                case "Referrer":
                    SetAuthorizationHeader(GenerateToken("saml-socialcare-corepathwayspilot"));
                    break;

                case "ReferrerAndBroker":
                    SetAuthorizationHeader(GenerateToken("saml-socialcare-corepathwayspilot", "saml-socialcarefinance-brokerage"));
                    CreateApiUser(withApprovalLimit, UserRole.Broker);
                    break;

                case "Broker":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    CreateApiUser(withApprovalLimit, UserRole.Broker);
                    break;

                case "BrokerageAssistant":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    CreateApiUser(withApprovalLimit, UserRole.BrokerageAssistant);
                    break;

                case "NewUser":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    break;

                case "CareChargesOfficer":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    CreateApiUser(withApprovalLimit, UserRole.CareChargesOfficer);
                    break;

                case "Approver":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    CreateApiUser(withApprovalLimit, UserRole.Approver);
                    break;

                case "BrokerAndApprover":
                    SetAuthorizationHeader(GenerateToken("saml-socialcarefinance-brokerage"));
                    CreateApiUser(withApprovalLimit, UserRole.Broker, UserRole.Approver);
                    break;
            }
        }

        private void SetAuthorizationHeader(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private void CreateApiUser(decimal? approvalLimit, params UserRole[] roles)
        {
            _apiUser = new User()
            {
                Name = "Api User",
                Email = "api.user@hackney.gov.uk",
                Roles = roles.ToList(),
                IsActive = true,
                ApprovalLimit = approvalLimit
            };

            Context.Users.Add(_apiUser);
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

        private static decimal? WithApprovalLimit()
        {
            if (!TestContext.CurrentContext.Test.Properties.ContainsKey("WithApprovalLimit")) return null;

            return (int) TestContext.CurrentContext.Test.Properties.Get("WithApprovalLimit");
        }

        private static string GenerateToken(params string[] groups)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("super-secret-token"));
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>();

            claims.Add(new Claim("sub", "123456789012345678901"));
            claims.Add(new Claim("email", "api.user@hackney.gov.uk"));
            claims.Add(new Claim("name", "Api User"));
            claims.Add(new Claim("groups", "HackneyAll"));

            foreach (string group in groups)
            {
                claims.Add(new Claim("groups", group));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Issuer = "Hackney",
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
