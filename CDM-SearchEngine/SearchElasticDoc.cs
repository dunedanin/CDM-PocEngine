using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDM_SearchEngine
{
    public class SearchElasticDoc
    {
        private String name;
        private String description;
        private String owner;


        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public String Description
        {
            get { return description; }
            set { description = value; }
        }

        public String Owner
        {
            get { return owner; }
            set { owner = value; }
        }
    }
}
