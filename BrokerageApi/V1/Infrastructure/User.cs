using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrokerageApi.V1.Infrastructure
{
    public class User : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public List<UserRole> Roles { get; set; }

        public bool IsActive { get; set; }
    }
}
