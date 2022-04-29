using System;

namespace BrokerageApi.V1.Infrastructure
{
    public struct ElementCost : IEquatable<ElementCost>
    {
        public ElementCost(decimal quantity, decimal cost)
        {
            Quantity = quantity;
            Cost = cost;
        }

        public decimal Quantity { get; }

        public decimal Cost { get; }

        public override int GetHashCode()
        {
            return Quantity.GetHashCode() ^ Cost.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ElementCost))
                return false;

            return Equals((ElementCost) obj);
        }

        public bool Equals(ElementCost other)
        {
            return (Quantity == other.Quantity) && (Cost == other.Cost);
        }

        public static bool operator ==(ElementCost cost1, ElementCost cost2)
        {
            return cost1.Equals(cost2);
        }

        public static bool operator !=(ElementCost cost1, ElementCost cost2)
        {
            return !cost1.Equals(cost2);
        }
    }
}
