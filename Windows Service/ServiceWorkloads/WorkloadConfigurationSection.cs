using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceWorkloads
{
    public class WorkloadConfigurationSection : ConfigurationSection
    {

        [ConfigurationProperty("workloads", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(WorkloadConfigurationColleciton),
            AddItemName = "workload",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public WorkloadConfigurationColleciton Workloads
        {
            get
            {
                return (WorkloadConfigurationColleciton)base["workloads"];
            }
        }

    }

    public class WorkloadConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        public String WorkloadName
        {
            get
            {
                return (string)base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("type", DefaultValue = "", IsRequired = true)]
        public String WorkloadType
        {
            get
            {
                return (string)base["type"];
            }
            set
            {
                base["type"] = value;
            }
        }

        [ConfigurationProperty("assembly", DefaultValue = "", IsRequired = false)]
        public String WorkloadAssembly
        {
            get => (string)base["assembly"];
            set => base["assembly"] = value;
        }


        [ConfigurationProperty("Properties", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(WorkloadPropertyCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public WorkloadPropertyCollection WorkloadProperties
        {
            get
            {
                return (WorkloadPropertyCollection)base["Properties"];
            }
        }
    }
    public class WorkloadProperty : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        public String PropertyName {
            get => (string)base["name"];
            set => base["name"] = value;
        }

        [ConfigurationProperty("value", DefaultValue = "", IsRequired = true)]
        public String PropertyValue {
            get => (string)base["value"];
            set => base["value"] = value;

        }

    }

}
