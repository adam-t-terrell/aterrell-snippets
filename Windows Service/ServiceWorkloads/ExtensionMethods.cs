using System;
using System.IO;
using System.Linq;

namespace ServiceWorkloads
{
    public static class ExtnesionMethods
    {
        public static string BaseFilename(this FileInfo f)
        {
            return f.Name.Substring(0, f.Name.Length - f.Extension.Length);
        }

        public static bool ContainsKey(this System.Collections.Specialized.NameValueCollection c, String key)
        {
            return (from k in c.AllKeys where k == key select k).Any();
        }
    }
}
