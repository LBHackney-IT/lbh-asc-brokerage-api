using System;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Testing;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class EndElementUseCaseTests
    {
        private Fixture _fixture;
        private Mock<IElementGateway> _mockElementGateway;
        private EndElementUseCase _classUnderTest;
        private MockDbSaver _dbSaver;
        private ClockService _clock;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockElementGateway = new Mock<IElementGateway>();
            _dbSaver = new MockDbSaver();

            var currentTime = SystemClock.Instance.GetCurrentInstant();
            var fakeClock = new FakeClock(currentTime);
            _clock = new ClockService(fakeClock);

            _classUnderTest = new EndElementUseCase(_mockElementGateway.Object, _dbSaver.Object, _clock);
        }

        [Test]
        public async Task CanEndElementWithoutEndDate()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var element = CreateElement();
            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            await _classUnderTest.ExecuteAsync(element.Id, endDate);

            element.EndDate.Should().Be(endDate);
            element.UpdatedAt.Should().Be(_clock.Now);
            _dbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task CanEndElementWithEndDate()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var element = CreateElement(ElementStatus.Approved, endDate.PlusDays(5));
            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            await _classUnderTest.ExecuteAsync(element.Id, endDate);

            element.EndDate.Should().Be(endDate);
            element.UpdatedAt.Should().Be(_clock.Now);
            _dbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementNotFound()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            const int elementId = 1234;
            _mockElementGateway.Setup(x => x.GetByIdAsync(elementId))
                .ReturnsAsync((Element) null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(elementId, endDate);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found {elementId} (Parameter 'id')");
            _dbSaver.VerifyChangesNotSaved();
        }

        [Test]
        public async Task ThrowsInvalidOperationWhenElementNotApproved([Values] ElementStatus status)
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var element = CreateElement(status);
            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(element.Id, endDate);

            if (status != ElementStatus.Approved)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage($"Element {element.Id} is not approved");
                _dbSaver.VerifyChangesNotSaved();
            }
        }

        [Test]
        public async Task ThrowsWhenElementEndDateIsBeforeRequested()
        {
            var endDate = LocalDate.FromDateTime(DateTime.Today);
            var element = CreateElement(ElementStatus.Approved, endDate.PlusDays(-5));
            _mockElementGateway.Setup(x => x.GetByIdAsync(element.Id))
                .ReturnsAsync(element);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(element.Id, endDate);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Element {element.Id} has an end date before the requested end date");
            _dbSaver.VerifyChangesNotSaved();
        }

        private Element CreateElement(ElementStatus status = ElementStatus.Approved, LocalDate? endDate = null)
        {
            var builder = _fixture.BuildElement(1, 1)
                .With(e => e.InternalStatus, status)
                .Without(e => e.EndDate);

            if (endDate != null)
            {
                builder = builder.With(e => e.EndDate, endDate);
            }

            return builder.Create();
        }
    }
}
