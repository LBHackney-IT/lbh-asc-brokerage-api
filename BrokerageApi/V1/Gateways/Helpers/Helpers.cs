using System;
using System.Linq;


namespace BrokerageApi.Tests.V1.Gateways.Helpers
{

    public static class ParsingHelpers
    {

        public static string ParsedQuery(string query)
        {
            var separators = new[] { " " };
            var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            var words = query.Split(separators, options).ToList();
            var terms = words.ConvertAll(w => $"{w}:*");

            return String.Join(" & ", terms);
        }
    }
}
