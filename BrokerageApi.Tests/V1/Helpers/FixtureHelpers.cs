using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Dsl;
using BrokerageApi.V1.Infrastructure;
using MicroElements.AutoFixture.NodaTime;

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
        public static IPostprocessComposer<Provider> BuildProvider(this IFixture fixture)
        {
            return fixture.Build<Provider>()
                .Without(p => p.Elements)
                .Without(p => p.Services)
                .Without(p => p.ProviderServices);
        }
        public static IPostprocessComposer<Service> BuildService(this IFixture fixture)
        {
            return fixture.Build<Service>()
                .Without(s => s.Providers)
                .Without(s => s.Services)
                .Without(s => s.ElementTypes)
                .Without(s => s.ProviderServices)
                .Without(s => s.Parent)
                .Without(s => s.ParentId);
        }
        public static IPostprocessComposer<ProviderService> BuildProviderService(this IFixture fixture, int providerId, int serviceId)
        {
            return fixture.Build<ProviderService>()
                .Without(ps => ps.Provider)
                .Without(ps => ps.Service)
                .With(ps => ps.ProviderId, providerId)
                .With(ps => ps.ServiceId, serviceId);
        }
        public static IPostprocessComposer<ElementType> BuildElementType(this IFixture fixture, int serviceId)
        {
            return fixture.Build<ElementType>()
                .Without(et => et.Service)
                .Without(et => et.Elements)
                .With(et => et.ServiceId, serviceId);
        }
        public static IPostprocessComposer<Element> BuildElement(this IFixture fixture, int providerId, int elementTypeId)
        {
            return fixture.Build<Element>()
                .Without(e => e.CarePackages)
                .Without(e => e.Referrals)
                .Without(e => e.ReferralElements)
                .Without(e => e.RelatedElement)
                .Without(e => e.RelatedElements)
                .Without(e => e.RelatedElementId)
                .Without(e => e.Provider)
                .Without(e => e.ElementType)
                .With(e => e.ProviderId, providerId)
                .With(e => e.ElementTypeId, elementTypeId);
        }
    }
}
