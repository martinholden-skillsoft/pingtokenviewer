using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenToken
{
    public class MultiStringDictionary : Dictionary<string, List<string>>
    {
        public void Add(string key, string value)
        {
            if (this.ContainsKey(key))
                this[key].Add(value);
            else
                this.Add(key, new List<string>() { value });
        }
    }
}
