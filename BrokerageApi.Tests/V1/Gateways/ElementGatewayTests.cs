using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Dsl;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Gateways
{
    [TestFixture]
    public class ElementGatewayTests : DatabaseTests
    {
        private ElementGateway _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _classUnderTest = new ElementGateway(BrokerageContext);
        }

        [Test]
        public async Task CanGetCurrentElements()
        {
            var elements = (await CreateElementBuilder()).CreateMany();
            await SeedElements(elements.ToArray());

            var resultElements = await _classUnderTest.GetCurrentAsync();

            resultElements.Should().BeEquivalentTo(elements.OrderBy(e => e.Id));
        }

        [Test]
        public async Task CanGetById()
        {
            var expectedElement = (await CreateElementBuilder()).Create();
            await SeedElements(expectedElement);

            var resultElement = await _classUnderTest.GetByIdAsync(expectedElement.Id);

            resultElement.Should().BeEquivalentTo(expectedElement);
        }

        [Test]
        public async Task CanGetBySocialCareId()
        {
            const string socialCareId = "expectedId";
            var expectedElements = (await CreateElementBuilder()).With(e => e.SocialCareId, socialCareId).CreateMany();
            var unexpectedElements = (await CreateElementBuilder()).With(e => e.SocialCareId, $"different{socialCareId}").CreateMany();
            await SeedElements(expectedElements.Concat(unexpectedElements).ToArray());

            var resultElements = await _classUnderTest.GetBySocialCareId(socialCareId);

            resultElements.Should().BeEquivalentTo(expectedElements.OrderBy(e => e.Id));
        }

        [Test]
        public async Task CanGetCurrentBySocialCareId()
        {
            // Arrange
            const string socialCareId = "expectedId";

            var approvedElements = (await CreateElementBuilder())
                .With(e => e.SocialCareId, socialCareId)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, Clock.Today - Period.FromDays(60))
                .With(e => e.EndDate, Clock.Today + Period.FromDays(30))
                .CreateMany();

            var ongoingElements = (await CreateElementBuilder())
                .With(e => e.SocialCareId, socialCareId)
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .With(e => e.StartDate, Clock.Today - Period.FromDays(60))
                .Without(e => e.EndDate)
                .CreateMany();

            var inProgressElements = (await CreateElementBuilder())
                .With(e => e.SocialCareId, socialCareId)
                .With(e => e.StartDate, Clock.Today + Period.FromDays(7))
                .Without(e => e.EndDate)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .CreateMany();

            var endedElements = (await CreateElementBuilder())
                .With(e => e.SocialCareId, socialCareId)
                .With(e => e.StartDate, Clock.Today - Period.FromDays(60))
                .With(e => e.EndDate, Clock.Today - Period.FromDays(30))
                .With(e => e.InternalStatus, ElementStatus.Approved)
                .CreateMany();

            var expectedElements = ongoingElements.Concat(approvedElements);
            var unexpectedElements = inProgressElements.Concat(endedElements);

            await SeedElements(expectedElements
                .Concat(unexpectedElements)
                .ToArray());

            // Act
            var resultElements = (await _classUnderTest.GetCurrentBySocialCareId(socialCareId)).ToArray();

            // Assert
            resultElements.Should().BeEquivalentTo(expectedElements.OrderBy(e => e.Id));
        }

        [Test]
        public async Task CanAddElement()
        {
            var service = _fixture.BuildService().Create();

            var elementType = _fixture.BuildElementType(service.Id)
                .Create();

            var provider = _fixture.BuildProvider().Create();

            var newElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .Create();

            await BrokerageContext.Services.AddAsync(service);
            await BrokerageContext.ElementTypes.AddAsync(elementType);
            await BrokerageContext.Providers.AddAsync(provider);
            await BrokerageContext.SaveChangesAsync();

            await _classUnderTest.AddElementAsync(newElement);

            var addedElement = await BrokerageContext.Elements.SingleOrDefaultAsync(e => e.Id == newElement.Id);
            addedElement.Should().NotBeNull();
            addedElement.Should().BeEquivalentTo(newElement);
        }

        private async Task SeedElements(params Element[] elements)
        {
            await BrokerageContext.Elements.AddRangeAsync(elements);
            await BrokerageContext.SaveChangesAsync();
        }

        private async Task<IPostprocessComposer<Element>> CreateElementBuilder()
        {
            var (provider, service) = await SeedProviderAndService();
            var elementType = await SeedElementType(service.Id);
            return _fixture.BuildElement(elementType.Id, provider.Id);
        }
    }
}
