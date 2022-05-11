using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using BrokerageApi.V1.Infrastructure;

namespace BrokerageApi.V1.Boundary.Response
{
    public class ServiceResponse
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Name { get; set; }

        public string Description { get; set; }

        public List<ElementTypeResponse> ElementTypes { get; set; }

        public bool ShouldSerializeElementTypes() => ElementTypes != null;
    }

}
