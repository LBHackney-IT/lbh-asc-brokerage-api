using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrokerageApi.V1.Infrastructure
{
    [Table("example_table")]
    public class DatabaseEntity
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
