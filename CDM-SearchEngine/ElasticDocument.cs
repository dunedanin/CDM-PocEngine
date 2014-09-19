using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDM_SearchEngine
{
    [ElasticType(Name = "_source")]
    public class ElasticDocument
    {

        private String index;
        private String type;
        private String id;        
        private SearchElasticDoc _search;
        private String _body;
        public String _url;

        public ElasticDocument()
        {
            Search = new SearchElasticDoc();
        }

        [ElasticProperty(Type = FieldType.Nested)]
        public SearchElasticDoc Search
        {
            get { return _search; }
            set { _search = value; }
        }
        
        public String Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public String Index
        {
            get { return index; }
            set { index = value; }
        }
       
        public String Type
        {
            get { return type; }
            set { type = value; }
        }

        public String Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public String Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}
