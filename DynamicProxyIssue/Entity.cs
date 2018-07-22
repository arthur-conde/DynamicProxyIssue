using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicProxyIssue
{
    public interface IEntity1<TDetail> where TDetail : IEntity2
    {
        ICollection<TDetail> Details { get; set; }
    }

    public class Entity1 : IEntity1<Entity2>
    {
        public virtual ICollection<Entity2> Details { get; set; }

        public Entity1()
        {
            Details = new Collection<Entity2>();
        }
    }

    public interface IEntity2
    {
        string Value { get; set; }
    }

    public class Entity2 : IEntity2
    {
        public string Value { get; set; }
    }
}
