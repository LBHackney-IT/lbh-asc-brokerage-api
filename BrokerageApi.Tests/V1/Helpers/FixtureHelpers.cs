using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Controllers.Parameters;
using BrokerageApi.V1.Infrastructure.AuditEvents;
using BrokerageApi.V1.Services;
using BrokerageApi.V1.Services.Interfaces;
using MicroElements.AutoFixture.NodaTime;
using NodaTime;
using NodaTime.Testing;

namespace BrokerageApi.Tests.V1.Helpers
{
    public static class FixtureHelpers
    {
        public static Fixture Fixture => CreateFixture();

        private static Fixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Customize(new NodaTimeCustomization());
            fixture.Customizations.Add(new LocalDateGenerator());
            fixture.Customize(new AutoMoqCustomization());

            return fixture;
        }

        public static int CreateInt(this IFixture fixture, int min, int max)
        {
            return fixture.Create<int>() % (max - min + 1) + min;
        }

        public static IPostprocessComposer<CarePackage> BuildCarePackage(this IFixture fixture, string socialCareId = null, bool withElements = false)
        {
            var builder = fixture.Build<CarePackage>()
                .Without(c => c.ReferralAmendments);

            if (!withElements)
            {
                builder = builder.Without(cp => cp.Elements);
            }

            if (socialCareId != null)
            {
                builder = builder.With(cp => cp.SocialCareId, socialCareId);
            }

            return builder;
        }

        public static IPostprocessComposer<Provider> BuildProvider(this IFixture fixture)
        {
            return fixture.Build<Provider>()
                .Without(p => p.Elements)
                .With(p => p.Type, ProviderType.Framework);
        }

        public static IPostprocessComposer<Service> BuildService(this IFixture fixture)
        {
            return fixture.Build<Service>()
                .Without(s => s.Services)
                .Without(s => s.ElementTypes)
                .Without(s => s.Parent)
                .Without(s => s.ParentId)
                .With(s => s.IsArchived, false);
        }

        public static IPostprocessComposer<ElementType> BuildElementType(this IFixture fixture, int serviceId, ElementTypeType type = ElementTypeType.Service)
        {
            return fixture.Build<ElementType>()
                .Without(et => et.Service)
                .Without(et => et.Elements)
                .With(et => et.ServiceId, serviceId)
                .With(et => et.Type, type)
                .With(et => et.CostOperation, MathOperation.Add)
                .With(et => et.PaymentOperation, MathOperation.Add)
                .With(et => et.IsArchived, false);
        }

        public static IPostprocessComposer<Referral> BuildReferral(this IFixture fixture, ReferralStatus? status = null)
        {
            var builder = fixture.Build<Referral>()
                .Without(r => r.Elements)
                .Without(r => r.ReferralElements)
                .Without(r => r.AssignedBroker)
                .Without(r => r.AssignedApprover)
                .Without(r => r.AssignedBrokerEmail)
                .Without(r => r.AssignedApproverEmail)
                .Without(r => r.ReferralAmendments)
                .With(r => r.WorkflowType, WorkflowType.Assessment)
                .With(r => r.CareChargeStatus, CareChargeStatus.New);

            if (status != null)
            {
                builder = builder.With(r => r.Status, status);
            }

            return builder;
        }
        public static IPostprocessComposer<Element> BuildElement(this IFixture fixture, int elementTypeId, int? providerId = null)
        {
            return fixture.Build<Element>()
                .Without(e => e.CarePackages)
                .Without(e => e.Referrals)
                .Without(e => e.ReferralElements)
                .Without(e => e.ParentElement)
                .Without(e => e.ParentElementId)
                .Without(e => e.SuspendedElement)
                .Without(e => e.SuspendedElementId)
                .Without(e => e.SuspensionElements)
                .Without(e => e.Provider)
                .Without(e => e.ElementType)
                .Without(e => e.EndDate)
                .With(e => e.ProviderId, providerId)
                .With(e => e.ElementTypeId, elementTypeId)
                .With(e => e.InternalStatus, ElementStatus.InProgress)
                .With(e => e.IsSuspension, false)
                .With(e => e.Clock, FakeClockFactory());
        }

        public static IPostprocessComposer<Element> WithoutCost(this IPostprocessComposer<Element> elementBuilder)
        {
            return elementBuilder.Without(e => e.EndDate)
                .Without(e => e.Monday)
                .Without(e => e.Tuesday)
                .Without(e => e.Wednesday)
                .Without(e => e.Thursday)
                .Without(e => e.Friday)
                .Without(e => e.Cost)
                .Without(e => e.DailyCosts);
        }

        public static IPostprocessComposer<ReferralElement> BuildReferralElement(this IFixture fixture, int referralId, int elementId, bool withPending = false)
        {
            var builder = fixture.Build<ReferralElement>()
                .Without(re => re.Element)
                .Without(re => re.Referral)
                .Without(re => re.CarePackage)
                .With(re => re.ReferralId, referralId)
                .With(re => re.ElementId, elementId);

            if (!withPending)
            {
                builder = builder
                    .Without(re => re.PendingCancellation)
                    .Without(re => re.PendingComment)
                    .Without(re => re.PendingEndDate);
            }

            return builder;
        }

        public static IPostprocessComposer<User> BuildUser(this IFixture fixture)
        {
            return fixture.Build<User>()
                .Without(u => u.ApproverCarePackages)
                .Without(u => u.BrokerCarePackages)
                .With(u => u.IsActive, true);
        }

        public static IPostprocessComposer<AuditEvent> BuildAuditEvent(this IFixture fixture)
        {
            return fixture.Build<AuditEvent>()
                .Without(ae => ae.Referral)
                .Without(ae => ae.User);
        }

        public static IPostprocessComposer<ReferralAmendment> BuildReferralAmendment(this IFixture fixture)
        {
            return fixture.Build<ReferralAmendment>()
                .Without(a => a.Referral)
                .Without(a => a.ReferralId);
        }

        public static IPostprocessComposer<ServiceUser> BuildServiceUser(this IFixture fixture)
        {
            return fixture.Build<ServiceUser>();
        }

        public static IPostprocessComposer<GetServiceUserRequest> BuildServiceUserRequest(this IFixture fixture, string socialCareId)
        {

            return fixture.Build<GetServiceUserRequest>()
            .With(su => su.SocialCareId, socialCareId);
        }

        public static IClockService FakeClockFactory()
        {
            var currentTime = SystemClock.Instance.GetCurrentInstant();
            var fakeClock = new FakeClock(currentTime);

            return new ClockService(fakeClock);
        }
    }
}
