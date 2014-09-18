using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDM_SearchEngine
{
    public class SearchCriteria
    {
        private String criteria;

        public String Criteria
        {
            get { return criteria; }
            set { criteria = value; }
        }

        public SearchCriteria(String p_criteria)
        {
            Criteria = p_criteria;
        }

    }
}
