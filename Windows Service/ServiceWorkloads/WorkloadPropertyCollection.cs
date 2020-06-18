using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    public class WorkloadPropertyCollection : ConfigurationElementCollection
    {
        public WorkloadPropertyCollection()
        {
        }

        public WorkloadProperty this[int index]
        {
            get { return (WorkloadProperty)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(WorkloadProperty property)
        {
            BaseAdd(property);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WorkloadProperty();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WorkloadProperty)element);
        }

        public void Remove(WorkloadProperty property)
        {
            BaseRemove(property);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}
