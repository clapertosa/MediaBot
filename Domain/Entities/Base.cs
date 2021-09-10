using System;

namespace Domain.Entities
{
    public abstract class Base<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}