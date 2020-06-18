using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    public class WorkloadConfigurationColleciton : ConfigurationElementCollection
    {
        public WorkloadConfigurationColleciton()
        {
        }

        public WorkloadConfiguration this[int index]
        {
            get { return (WorkloadConfiguration)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(WorkloadConfiguration property)
        {
            BaseAdd(property);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WorkloadConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WorkloadConfiguration)element);
        }

        public void Remove(WorkloadConfiguration property)
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
