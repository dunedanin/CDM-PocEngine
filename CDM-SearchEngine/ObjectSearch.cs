using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDM_SearchEngine
{
    class ObjectSearch
    {
        public String name;
        public String description;
        public String owner;

        public ObjectSearch(String p_name, String p_description, String p_owner)
        {
            name = p_name;
            description = p_description;
            owner = p_owner;
        }
    }
}
