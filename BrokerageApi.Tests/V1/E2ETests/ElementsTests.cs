using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Boundary.Response;
using BrokerageApi.V1.Factories;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NodaTime;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.E2ETests
{
    public class InstantComparer : IEquivalencyStep
    {
        public bool CanHandle(IEquivalencyValidationContext context, IEquivalencyAssertionOptions config)
        {
            return context.Subject is Instant && context.Expectation is Instant;
        }

        public bool Handle(IEquivalencyValidationContext context, IEquivalencyValidator parent, IEquivalencyAssertionOptions config)
        {
            var a = (Instant) context.Subject;
            var b = (Instant) context.Expectation;

            return a.ToUnixTimeMilliseconds() == b.ToUnixTimeMilliseconds();
        }
    }

    public class ElementsTests : IntegrationTests<Startup>
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetElements()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var elements = _fixture.BuildElement(elementType.Id, provider.Id).CreateMany();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.Elements.AddRangeAsync(elements);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<List<ElementResponse>>($"/api/v1/elements/current");

            code.Should().Be(HttpStatusCode.OK);
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            response.ToArray().Should().BeEquivalentTo(elements.OrderBy(e => e.Id).Select(e => e.ToResponse()));
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetElementsWithParent()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id).Create();
            var childElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ParentElementId, parentElement.Id)
                .Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.Elements.AddRangeAsync(parentElement, childElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<List<ElementResponse>>($"/api/v1/elements/current");

            code.Should().Be(HttpStatusCode.OK);
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            var resultChild = response.Single(e => e.Id == childElement.Id);
            resultChild.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse());
        }

        [Test, Property("AsUser", "Broker")]
        public async Task CanGetElementsById()
        {
            var provider = _fixture.BuildProvider().Create();
            var service = _fixture.BuildService().Create();
            var elementType = _fixture.BuildElementType(service.Id).Create();
            var parentElement = _fixture.BuildElement(elementType.Id, provider.Id).Create();
            var childElement = _fixture.BuildElement(elementType.Id, provider.Id)
                .With(e => e.ParentElementId, parentElement.Id)
                .Create();

            await Context.Services.AddAsync(service);
            await Context.ElementTypes.AddAsync(elementType);
            await Context.Providers.AddAsync(provider);
            await Context.Elements.AddRangeAsync(parentElement, childElement);
            await Context.SaveChangesAsync();

            Context.ChangeTracker.Clear();

            var (code, response) = await Get<ElementResponse>($"/api/v1/elements/{childElement.Id}");

            code.Should().Be(HttpStatusCode.OK);
            AssertionOptions.EquivalencySteps.Insert<InstantComparer>();
            response.ParentElement.Should().BeEquivalentTo(parentElement.ToResponse());
        }
    }
}
