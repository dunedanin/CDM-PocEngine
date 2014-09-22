using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDM_SearchEngine
{
    public class SearchCriteria
    {
        private String condition;
        private String queryType;

        public String QueryType
        {
            get { return queryType; }
            set { queryType = value; }
        }

        public String Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        public SearchCriteria(String p_condition, String p_queryType)
        {
            Condition = p_condition;
            QueryType = p_queryType;

        }

    }
}
