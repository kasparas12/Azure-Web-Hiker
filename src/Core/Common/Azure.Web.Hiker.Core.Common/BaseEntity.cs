using System;

namespace Azure.Web.Hiker.Core.Common
{
    public class BaseEntity
    {
        public BaseEntity()
        {
        }
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
