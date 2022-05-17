using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.Dsl;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.Tests.V1.Helpers
{
    public static class FixtureHelpers
    {
        public static Fixture Fixture => CreateFixture();
        private static Fixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Customizations.Add(new LocalDateGenerator());

            return fixture;
        }

        public static int CreateInt(this IFixture fixture, int min, int max)
        {
            return fixture.Create<int>() % (max - min + 1) + min;
        }
        public static IPostprocessComposer<CarePackage> BuildCarePackage(this IFixture fixture, string socialCareId)
        {
            return fixture.Build<CarePackage>()
                .With(cp => cp.SocialCareId, socialCareId);
        }
    }
}
