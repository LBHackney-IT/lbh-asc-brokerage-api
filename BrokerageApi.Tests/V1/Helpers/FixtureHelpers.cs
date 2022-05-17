using AutoFixture;
using AutoFixture.AutoMoq;
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
    }
}
