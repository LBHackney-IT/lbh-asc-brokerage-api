using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using BrokerageApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Npgsql;
using NUnit.Framework;

namespace BrokerageApi.Tests
{
    public class IntegrationTests<TStartup> where TStartup : class
    {
        protected HttpClient Client { get; private set; }
        protected BrokerageContext Context => _factory.Context;

        private MockWebApplicationFactory<TStartup> _factory;
        private IDbContextTransaction _transaction;
        private DbContextOptionsBuilder _builder;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _builder = new DbContextOptionsBuilder();
            _builder.UseNpgsql(ConnectionString.TestDatabase())
                .UseSnakeCaseNamingConvention();
        }

        [SetUp]
        public void BaseSetup()
        {
            _factory = new MockWebApplicationFactory<TStartup>(_builder);

            Client = _factory.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken());

            _factory.Context.Database.Migrate();
            _transaction = _factory.Context.Database.BeginTransaction();
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

        private static string GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("super-secret-token"));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("sub", "123456789012345678901"),
                    new Claim("email", "a.broker@hackney.gov.uk"),
                    new Claim("name", "A Broker"),
                    new Claim("groups", "HackneyAll"),
                    new Claim("groups", "saml-socialcarefinance-brokerage")
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
