using System.Collections.Specialized;
using System.Threading.Tasks;
using BrokerageApi.V1.Controllers;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class UserRolesMiddlewareTest : DatabaseTests
    {
        private UserRolesMiddleware _sut;
        private UserGateway _userGateway;

        [SetUp]
        public void Init()
        {
            _sut = new UserRolesMiddleware(null);
            _userGateway = new UserGateway(BrokerageContext);
        }

        [Test]
        public async Task DoesGetRolesFromJWToken()
        {
            // Ideally these could be moved to some kind of Data Provider, but
            // I couldn't get [DataTestMethod] or [DataRow] to work
            // Fake JWT's created at jwt.io
            var jwtTests = new NameValueCollection();
            // "care.chargesofficer@hackney.gov.uk"
            jwtTests.Add(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJjYXJlLmNoYXJnZXNvZmZpY2VyQGhhY2tuZXkuZ292LnVrIiwiaWF0IjoyfQ.2_UG2zfthTPNbk7Bxzpbb2NxyAkkW_PI26NgF5PDRPY",
                "care_charges_officer");
            // "brokerage.assistant@hackney.gov.uk"
            jwtTests.Add(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJicm9rZXJhZ2UuYXNzaXN0YW50QGhhY2tuZXkuZ292LnVrIiwiaWF0IjoyfQ.9d4CuUlDEPeNjn3rdkN5NNZCFZptQ0M2MmB11Fg-o90",
                "brokerage_assistant");
            // "an.approver@hackney.gov.uk"
            jwtTests.Add(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJhbi5hcHByb3ZlckBoYWNrbmV5Lmdvdi51ayIsImlhdCI6Mn0._Re7Z8g3XELlWF_6KwWKiyRrpjEMsGGunpHwb7xK_qk",
                "approver");
            // "a.broker@hackney.gov.uk"
            jwtTests.Add(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJhLmJyb2tlckBoYWNrbmV5Lmdvdi51ayIsImlhdCI6Mn0.LxrOZU8uz-zHtMwR6AseCApmLfKqkNsFvlwHd9drpd4",
                "broker");
            // "deactivated.user@hackney.gov.uk"
            jwtTests.Add(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJkZWFjdGl2YXRlZC51c2VyQGhhY2tuZXkuZ292LnVrIiwiaWF0IjoyfQ.QAFwwEQerIx-9GlRWz14ti-jrD07SYMZnCMvuTtbAnU",
                ""); // empty, no roles
            // "billy.nomates@hackney.gov.uk"
            jwtTests.Add(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJiaWxseS5ub21hdGVzQGhhY2tuZXkuZ292LnVrIiwiaWF0IjoyfQ.hEHh8uLF6SgHcYLQeytkxU5OvUqo80zlIKl-s9D3yMY",
                "" // not actually sure
            );
            // no email in JWT
            jwtTests.Add(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoyfQ.NxI-RFXwMfbphQ9yozZgTmGr31IAhPv8bnVg61dKPRg",
                "" // not actually sure
            );

            foreach (string jwt in jwtTests)
            {
                string shouldMatch = jwtTests[jwt];

                // Arrange
                var httpContext = new DefaultHttpContext();

                httpContext.HttpContext.Request.Headers.Add(Constants.Authorization, jwt);

                // Act
                await _sut.InvokeAsync(httpContext, _userGateway).ConfigureAwait(false);

                string userRoles = httpContext.HttpContext.Request.Headers[Constants.UserRoles];

                userRoles.Should().BeEquivalentTo(shouldMatch);
            }
        }
    }
}
