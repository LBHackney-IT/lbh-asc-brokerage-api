using System.Collections.Generic;
using AutoFixture;
using BrokerageApi.Tests.V1.Helpers;
using BrokerageApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BrokerageApi.Tests.V1.Infrastructure
{
    public class ElementStatusTests : DatabaseTests
    {
        [TestCase(-10, -10, ElementStatus.Ended)]
        [TestCase(-10, +10, ElementStatus.Active)]
        [TestCase(+10, +20, ElementStatus.Inactive)]
        public void StatusReflectsEndDate(int startDateOffset, int endDateOffset, ElementStatus expectedStatus)
        {
            var element = new Element(BrokerageContext)
            {
                InternalStatus = ElementStatus.Approved,
                StartDate = CurrentDate.PlusDays(startDateOffset),
                EndDate = CurrentDate.PlusDays(endDateOffset)
            };

            element.Status.Should().Be(expectedStatus);
        }

        [TestCase(ElementStatus.Approved, ElementStatus.Suspended)]
        [TestCase(ElementStatus.InProgress, ElementStatus.Active)]
        public void StatusReflectsSuspension(ElementStatus suspensionStatus, ElementStatus expectedStatus)
        {
            var suspensionElement = new Element(BrokerageContext)
            {
                InternalStatus = suspensionStatus,
                StartDate = CurrentDate.PlusDays(-5),
                EndDate = CurrentDate.PlusDays(5)
            };

            var previousSuspensionElement = new Element(BrokerageContext)
            {
                InternalStatus = ElementStatus.Approved,
                StartDate = CurrentDate.PlusDays(-15),
                EndDate = CurrentDate.PlusDays(-10)
            };

            var upcomingSuspensionElement = new Element(BrokerageContext)
            {
                InternalStatus = ElementStatus.Approved,
                StartDate = CurrentDate.PlusDays(5),
                EndDate = CurrentDate.PlusDays(10)
            };

            var element = new Element(BrokerageContext)
            {
                InternalStatus = ElementStatus.Approved,
                StartDate = CurrentDate.PlusDays(-10),
                SuspensionElements = new List<Element> { suspensionElement, previousSuspensionElement, upcomingSuspensionElement }
            };

            element.Status.Should().Be(expectedStatus);
        }
    }
}
