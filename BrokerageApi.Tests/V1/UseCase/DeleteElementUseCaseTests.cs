using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.UseCase
{
    public class DeleteElementUseCaseTests
    {
        private DeleteElementUseCase _classUnderTest;
        private Mock<IReferralGateway> _mockReferralGateway;
        private Fixture _fixture;
        private Mock<IUserService> _mockUserService;
        private MockDbSaver _mockDbSaver;

        [SetUp]
        public void Setup()
        {
            _fixture = FixtureHelpers.Fixture;
            _mockReferralGateway = new Mock<IReferralGateway>();
            _mockUserService = new Mock<IUserService>();
            _mockDbSaver = new MockDbSaver();

            _classUnderTest = new DeleteElementUseCase(
                _mockReferralGateway.Object,
                _mockUserService.Object,
                _mockDbSaver.Object
            );
        }

        [Test]
        public async Task DeletesElement()
        {
            const int elementId = 123;
            const string userName = "a.broker@hackney.gov.uk";

            ReturnsUser(userName);

            var elements = CreateElements(elementId + 1).ToList();
            elements.Add(CreateElement(elementId));
            var referral = CreateReferral(ReferralStatus.InProgress, elements, userName);
            ReturnsReferral(referral);

            await _classUnderTest.ExecuteAsync(referral.Id, elementId);

            referral.Elements.Should().NotContain(e => e.Id == elementId);
            _mockDbSaver.VerifyChangesSaved();
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenReferralDoesntExist()
        {
            const int referralId = 123;

            ReturnsReferral(null);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referralId, 456);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Referral not found for: {referralId} (Parameter 'referralId')");
        }

        [Test]
        public async Task ThrowsArgumentNullExceptionWhenElementDoesntExist()
        {
            const int elementId = 123;
            const string userName = "a.broker@hackney.gov.uk";

            ReturnsUser(userName);

            var elements = CreateElements(elementId + 1);
            var referral = CreateReferral(ReferralStatus.InProgress, elements.ToList(), userName);
            ReturnsReferral(referral);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, elementId);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"Element not found for: {elementId} (Parameter 'elementId')");
        }

        [Test]
        public async Task ThrowsInvalidOperationExceptionWhenReferralNotInProgress([Values] ReferralStatus status)
        {
            const int elementId = 123;
            const string userName = "a.broker@hackney.gov.uk";

            ReturnsUser(userName);

            var referral = CreateReferral(status, null, userName);
            ReturnsReferral(referral);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, elementId);

            if (status != ReferralStatus.InProgress)
            {
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("Referral is not in a valid state for editing");
            }
            else
            {
                await act.Should().NotThrowAsync<InvalidOperationException>();
            }
        }

        [Test]
        public async Task ThrowsUnauthorizedAccessExceptionWhenReferralIsAssignedToSomeoneElse()
        {
            const int elementId = 123;
            const string userName = "a.broker@hackney.gov.uk";

            ReturnsUser(userName);

            var element = CreateElement(elementId);
            var referral = CreateReferral(ReferralStatus.InProgress, new List<Element>
            {
                element
            }, "assigned@to.com");
            ReturnsReferral(referral);

            Func<Task> act = () => _classUnderTest.ExecuteAsync(referral.Id, elementId);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage($"Referral is not assigned to {userName}");
        }

        private void ReturnsUser(string userName)
        {

            _mockUserService
                .SetupGet(x => x.Name)
                .Returns(userName);
        }

        private Referral CreateReferral(ReferralStatus referralStatus = ReferralStatus.InProgress, List<Element> elements = null, string assignedToCom = null)
        {
            var referralBuilder = _fixture.Build<Referral>()
                .With(r => r.Status, referralStatus)
                .With(r => r.Elements, elements);

            if (!(assignedToCom is null)) referralBuilder = referralBuilder.With(r => r.AssignedTo, assignedToCom);

            return referralBuilder.Create();
        }

        private void ReturnsReferral(Referral referral)
        {
            _mockReferralGateway
                .Setup(x => x.GetByIdWithElementsAsync(It.IsAny<int>()))
                .ReturnsAsync(referral);
        }

        private IEnumerable<Element> CreateElements(int minId = 0)
        {
            var elements = _fixture.Build<Element>()
                .With(e => e.Id, _fixture.CreateInt(minId, minId))
                .CreateMany();
            return elements;
        }
        private Element CreateElement(int elementId)
        {
            var element = _fixture.Build<Element>()
                .With(e => e.Id, elementId)
                .Create();
            return element;
        }
    }

}
