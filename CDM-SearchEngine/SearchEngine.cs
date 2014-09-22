using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Linq.Expressions;

using Microsoft.SharePoint.Client;
using Nest;
using Elasticsearch.Net;
using Newtonsoft.Json;
using LinqKit;
using CDM_SearchEngine.Northwind;
using CDM_SearchEngine.mdmpartnercustomer;
using System.Net;
using CDM_SearchEngine.ftlpssrslb;
using System.IO;
using System.Web.Services.Protocols;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using Newtonsoft.Json.Linq;

namespace CDM_SearchEngine
{
    public class SearchEngine
    {
        private static SearchEngine instance = null;    
        private IElasticsearchClient clientElastic;
        private ElasticClient clientElasticNest;
        private Web oWebsiteSharePoint;
        private NorthwindEntities contextNorth;
        private MDMPartnerCustomerEntities contextMDM_pc;
        private ClientContext clientContextSharePoint;
        private ReportingService2010SoapClient clientSoap;
        // Define the URI of the public ElasticSearch service.
        //private Uri hostElastic = new Uri("http://10.108.144.123:9200/", UriKind.Absolute);
        private Uri hostElastic = new Uri("http://localhost:9200/", UriKind.Absolute);
        private Uri northwindUri = new Uri(SERVICE_OD_URL_NORTH, UriKind.Absolute); //public Northwind OData service
        private Uri storeUri = new Uri(SERVICE_OD_URL_STORE, UriKind.Absolute);
        private Uri mdmpcUri = new Uri(SERVICE_OD_URL_MDMPC, UriKind.Absolute);

        private const String ID_NOT_FOUND = "<ID NOT FOUND>";
	    private const String HTTP_METHOD_PUT = "PUT";
	    private const String HTTP_METHOD_POST = "POST";
	    private const String HTTP_METHOD_GET = "GET";
	    private const String HTTP_METHOD_DELETE = "DELETE";
	    private const String HTTP_HEADER_CONTENT_TYPE = "Content-Type";
	    private const String HTTP_HEADER_ACCEPT = "Accept";
	    private const String APPLICATION_JSON = "application/json";
	    private const String APPLICATION_XML = "application/xml";
	    private const String APPLICATION_ATOM_XML = "application/atom+xml";
	    private const String APPLICATION_FORM = "application/x-www-form-urlencoded";
	    private const String METADATA = "$metadata";
	    private const String INDEX = "/index.jsp";
	    private const String SEPARATOR = "/";
        private const String NAME = "name";
        private const String DESCRIPTION = "description";
        private const String OWNER = "owner";
        private const String DP = ":";
        private const String QUERYI = "q";
        private const String SPACE = " ";
        private const String EMPTY = "";
        private const String OR = "OR";
        private const String AND = "AND";
        private const String HITS = "hits";
        private const String SEPARATOR_URL = "%2f";
        private const String TOTAL_HITS = "total";
	    private const bool PRINT_RAW_CONTENT = true;
        private const String PARENT_URL_CITROPEDIA = "/sites/it/ea/DMO/Citropedia/Lists";
        private const String HEADER_SP_URL = "http://sharepoint.citrite.net/sites/it/ea/DMO/Citropedia/Lists/BI%20Term/Item/displayifs.aspx?List=";
        private const String HEADER_SSRS_URL = "http://ftlpssrslb/Reports/Pages/Report.aspx?ItemPath=";
	    //private const String SERVICE_OD_URL = "http://localhost:8080/cars-annotations-sample/MyFormula.svc";
        private const String SERVICE_OD_URL_NORTH = "http://services.odata.org/Northwind/Northwind.svc/";
        private const String SERVICE_OD_URL_STORE = "http://services.odata.org/V2/(S(r2ir5rzsz3ygo1dahemljxgj))/OData/OData.svc/";
        private const String SERVICE_OD_URL_MDMPC = "http://service.citrite.net/entity/GenericOData/ods/mdmpartnercustomer";
        private const String ENDPOINT_CITROPEDIA = "http://sharepoint.citrite.net/sites/it/ea/DMO/Citropedia";
        private const String INDEX_NORTH = "northwind";
        private const String INDEX_STORE = "store";
        private const String INDEX_MDMPC = "mdmpartnercustomer";
        private const String USER_DS = "t_leandrod1";
        private const String PASSWORD_DS = "tellago*7";
	    private const String USED_FORMAT = APPLICATION_JSON;
        private const String CITROPEDIA = "citropedia";
        private const String SSRS = "ssrs";
        private const String TITLE = "title";

        private const String MATCH = "MATCH";
        private const String TERM = "TERM";
        private const String SHOULD = "SHOULD";
        private const String MUST = "MUST";

        /*static void Main()
        {           
        }*/

        private SearchEngine() {	      
    		StartupElasticSearch(); //ElasticSearch
            //StartUpOData(); //OData
            StartUpSharePoint();//Citropedia
            StartUpSSRS(); //SSRS
	    }

        //Singleton instance
        public static SearchEngine GetInstance() {
		    if(instance == null) {	
			    instance = new SearchEngine();
    		}
		    return instance;
	    }

        private void StartupElasticSearch(){
            var settings = new ConnectionSettings(hostElastic);
            //clientElastic = new ElasticsearchClient();            
            clientElasticNest = new ElasticClient(settings);
            clientElastic = clientElasticNest.Raw;
        }

        private void StartUpOData()
        {
            // Define the URI of the public Northwind OData service.
            contextNorth = new NorthwindEntities(northwindUri);
            contextMDM_pc = new MDMPartnerCustomerEntities(mdmpcUri);            
            SetCredentialsMDM();
        }

        private void StartUpSharePoint()
        {
            clientContextSharePoint = new ClientContext(ENDPOINT_CITROPEDIA);      
            clientContextSharePoint.Credentials = new NetworkCredential(USER_DS, PASSWORD_DS);
            oWebsiteSharePoint = clientContextSharePoint.Web;
            clientContextSharePoint.Load(oWebsiteSharePoint);
            clientContextSharePoint.ExecuteQuery();
            Debug.WriteLine(oWebsiteSharePoint.Title);

            /*List docList = clientContext.Web.Lists.GetByTitle("Citrix Data Catalog");
            clientContext.Load(docList); 
            CamlQuery camlQuery = new CamlQuery();            
            camlQuery.ViewXml = "<View/>";
            //camlQuery.ViewXml = "<View Scope='RecursiveAll'></View>";

            ListItemCollection listItems = docList.GetItems(camlQuery);
            clientContext.Load(docList); clientContext.Load(listItems);
            clientContext.ExecuteQuery();
            foreach (ListItem listItem in listItems)
                Console.WriteLine("Id: {0} Title: {1}", listItem.Id, listItem["Title"]);*/
            
        }

        private void StartUpSSRS()
        {
            try
            {
              clientSoap = new ReportingService2010SoapClient(); 
              clientSoap.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(USER_DS, PASSWORD_DS);
              clientSoap.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

              /*TrustedUserHeader Myheader = new TrustedUserHeader();
              string reportName = "/CMD/AccountPenetrationReport";
              byte[] reportDefinition = null;
              clientSoap.GetItemDefinition(Myheader, reportName, out reportDefinition);*/

             }
            catch (SoapException e)
            {
                Console.WriteLine(e.Detail.InnerXml.ToString());
            }            
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private void SetCredentialsMDM()
        {
            contextMDM_pc.Credentials = new NetworkCredential(USER_DS, PASSWORD_DS);
        }

        public int? GetRequestStatus(String index, String type, String id, Object request)
        {
            return clientElastic.Get(index, type, id).HttpStatusCode;                
        }

        public int? PostRequestStatus(String index, String type, String id, Object request)
        {
            var indexResponse = clientElastic.Index(index, type, id, request);
            return indexResponse.HttpStatusCode;
        }

        public bool PostClientIndex(String index, String type, String id, Object request)
        {
            return clientElastic.Index(index, type, id, request).Success;
        }

        public bool CheckDocumentOnES(String index, String type, String id, Object request)
        {                        
            //string sss = getResponse.Response["_source"];            
            return clientElastic.Get(index, type, id).Success;           
        }

        public bool CheckDocumentOnOD(String index, String type, String id, Object request)
        {
            //northwind + customers + alfki (index+type+id)

            /*IQueryable<String> query = from o in context.Customers
                                       where o.CustomerID == "ALFKI"
                                       select o;

            var keywords = new List<string>() { "Test1", "Test2" };

            var predicate = PredicateBuilder.False(query);
            
            foreach (var key in keywords)
            {
                predicate = predicate.Or(a => a.Text.Contains(key));
            }

            var query2 = context.Customers.AsQueryable().Where(predicate);*/

            return clientElastic.Get(index, type, id).Success;
        }
       
        public ElasticsearchDynamicValue[] Search(ElasticDocument objectToSearch, SearchCriteria p_criteria)
        //public IEnumerable<ElasticDocument> Search(ElasticDocument objectToSearch)
        {
            ElasticsearchDynamicValue[] response = null;
            QueryContainer queryContainer = new QueryContainer();

           /*
            var queryMatchName = new MatchQueryDescriptor<ElasticDocument>().OnField("_search.name").Query(objectToSearch.Search.Name);
            var queryMatchDescription = new MatchQueryDescriptor<ElasticDocument>().OnField("_search.description").Query(objectToSearch.Search.Description);
            var queryMatchOwner = new MatchQueryDescriptor<ElasticDocument>().OnField("_search.owner").Query(objectToSearch.Search.Owner);

            var queryTermName = new TermQueryDescriptor<ElasticDocument>().OnField("_search.name").Value(objectToSearch.Search.Name);
            var queryTermDescription = new TermQueryDescriptor<ElasticDocument>().OnField("_search.description").Value(objectToSearch.Search.Description);
            var queryTermOwner = new TermQueryDescriptor<ElasticDocument>().OnField("_search.owner").Value(objectToSearch.Search.Owner);
            */

            if (p_criteria.Condition.Equals(SHOULD) && p_criteria.QueryType.Equals(MATCH))
                queryContainer = new QueryDescriptor<ElasticDocument>().Bool(b => b.
                            Should(s => s.Match(m => m.OnField("_search.name").Query(objectToSearch.Search.Name)) ||
                                        s.Match(m => m.OnField("_search.description").Query(objectToSearch.Search.Description)) ||
                                        s.Match(m => m.OnField("_search.owner").Query(objectToSearch.Search.Owner))));

            if (p_criteria.Condition.Equals(MUST) && p_criteria.QueryType.Equals(MATCH))
                queryContainer = new QueryDescriptor<ElasticDocument>().Bool(b => b.
                            Must(s => s.Match(m => m.OnField("_search.name").Query(objectToSearch.Search.Name)) &&
                                      s.Match(m => m.OnField("_search.description").Query(objectToSearch.Search.Description)) &&
                                      s.Match(m => m.OnField("_search.owner").Query(objectToSearch.Search.Owner))));

            if (p_criteria.Condition.Equals(SHOULD) && p_criteria.QueryType.Equals(TERM))
                queryContainer = new QueryDescriptor<ElasticDocument>().Bool(b => b.
                        Should(s => s.Term(m => m.OnField("_search.name").Value(objectToSearch.Search.Name)) ||
                                    s.Term(m => m.OnField("_search.description").Value(objectToSearch.Search.Description)) ||
                                    s.Term(m => m.OnField("_search.owner").Value(objectToSearch.Search.Owner))));

            if (p_criteria.Condition.Equals(MUST) && p_criteria.QueryType.Equals(TERM))
                queryContainer = new QueryDescriptor<ElasticDocument>().Bool(b => b.
                            Must(s => s.Term(m => m.OnField("_search.name").Value(objectToSearch.Search.Name)) &&
                                      s.Term(m => m.OnField("_search.description").Value(objectToSearch.Search.Description)) &&
                                      s.Term(m => m.OnField("_search.owner").Value(objectToSearch.Search.Owner))));
            
            var searchDescriptor = new SearchDescriptor<ElasticDocument>().
                Query(queryContainer);

            var request = clientElasticNest.Serializer.Serialize(searchDescriptor);

            var results = clientElasticNest.Raw.Search(request);

            int total_hits = (int)results.Response[HITS][TOTAL_HITS];
            ElasticsearchDynamicValue hits = results.Response[HITS][HITS];

            if (total_hits > 0)
                response = new ElasticsearchDynamicValue[total_hits];
            else
                response = new ElasticsearchDynamicValue[0];

            for (int i = 0; i < total_hits; i++)
            {
                response[i] = hits[i];
            }

            return response;
            
        }

        private String GetMyFuzzyText(ElasticDocument doc)
        {
            return doc.Search.Name + SPACE + doc.Search.Description + SPACE + doc.Search.Owner;
        }
        private String GenerateDocumentUri(ElasticDocument objectToSearch, String unionSearch)
        {
            String document = null;

            if (CheckSearchValue(objectToSearch.Search.Name))
                document = NAME + DP + objectToSearch.Search.Name.Trim() + unionSearch;

            if (CheckSearchValue(objectToSearch.Search.Description))
                document = document + DESCRIPTION + DP + objectToSearch.Search.Description.Trim();

            if (CheckSearchValue(objectToSearch.Search.Owner))
                document = document + OWNER + DP + objectToSearch.Search.Owner.Trim();
            
            return document;
        }

        private bool CheckSearchValue(String value)
        {
            if (value == null || value.Trim() == EMPTY)
                return false;

            return true;
        }

        private void GetSource(String index, String type, String id)
        {
            //clientElastic.GetSource(index, type, id, qs => qs
              //          .Routing("routingvalue").AddQueryString("name", "Dagmar Garcia"));
        }
        public ElasticsearchResponse<DynamicDictionary> SearchGet(String index, String type, Func<SearchRequestParameters, SearchRequestParameters> requestParameters)
        {
            return clientElastic.SearchGet(index, type, requestParameters);
        }
        public ElasticsearchResponse<DynamicDictionary> SearchGet(Func<SearchRequestParameters, SearchRequestParameters> requestParameters)
        {
            return clientElastic.SearchGet(requestParameters);
        }

        public ListItemCollection SearchOnSP(String itemName)
        {            
            List docList = oWebsiteSharePoint.Lists.GetByTitle(itemName);
            
            return GetItemFromSP(docList);
        }

        public ListItemCollection SearchOnSPById(Guid id)
        {
            List docList = oWebsiteSharePoint.Lists.GetById(id);

            return GetItemFromSP(docList);
        }

        private ListItemCollection GetItemFromSP(List docList)
        {
            clientContextSharePoint.Load(docList);
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View/>";

            ListItemCollection listItems = docList.GetItems(camlQuery);

            clientContextSharePoint.Load(listItems);
            clientContextSharePoint.ExecuteQuery();

            return listItems;
        }

        public CatalogItem[] GetCatalogItems(String itemPath)
        {
            TrustedUserHeader Myheader = new TrustedUserHeader();
            CatalogItem[] catalogItems = null;

            try
            {
                clientSoap.ListChildren(Myheader, itemPath, true, out catalogItems);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            
            
            return catalogItems;
        }    

        private CatalogItem[] GetAllCatalogSSRS()
        {
            TrustedUserHeader Myheader = new TrustedUserHeader();
            CatalogItem[] catalogItems = null;
            clientSoap.ListChildren(Myheader, "/", true, out catalogItems);

            return catalogItems;
        }
    
        public bool UpdateElastic()
        {
            if (UpdateFromSharePoint() && UpdateFromSSRS())
                return true;
            
            return false;
        }

        private bool UpdateFromSharePoint()
        {
            ElasticDocument document = new ElasticDocument();
            String idItemCitroUrl, headerItemUrl;            
            //CamlQuery camlQuery = new CamlQuery();
            CamlQuery camlQuery = CamlQuery.CreateAllItemsQuery();
            //camlQuery.ViewXml = "<View/>";

            ListCollection itemsCitro = GetAllCitropediaLists();
            //List itemsBi = items.GetByTitle("BI Terms");
            ListItemCollection listItems;

            document.Index = CITROPEDIA;           	 
 
            foreach (List itemList in itemsCitro)
            {
                if (IsBiTerm(itemList.Title))
                {
                    listItems = itemList.GetItems(camlQuery);
                    clientContextSharePoint.Load(listItems);

                    clientContextSharePoint.Load(listItems,
                        items=> items.Include(
                        item=>item.ContentType,
                        item=>item.ContentType.Name));

                    clientContextSharePoint.ExecuteQuery();

                    document.Type = itemList.Id.ToString();
                    headerItemUrl = HEADER_SP_URL + "List=" + document.Type;

                    foreach (var item in listItems)
                    {
                        var v_name = item.FieldValues.Where(entry => entry.Key.ToLower().Contains(TITLE)).First().Value;
                        var v_description = item.FieldValues.Where(entry => entry.Key.ToLower().Contains(DESCRIPTION)).First().Value;
                        var dd = item.FieldValues.Where(entry => entry.Key.ToLower().Contains(OWNER));
                        var v_owner = item.FieldValues.Where(entry => entry.Key.ToLower().Contains(OWNER)).First().Value;

                        if (v_name != null)
                            document.Search.Name = v_name.ToString();
                        else
                            document.Search.Name = SPACE;

                        if (v_description != null)
                            document.Search.Description = v_description.ToString();
                        else
                            document.Search.Description = SPACE;

                        if (v_owner != null)
                            document.Search.Owner = v_owner.ToString();
                        else
                            document.Search.Owner = SPACE;

                        idItemCitroUrl = "&ID=" + item.Id + "&ContentTypeId=" + item.ContentType.Id.ToString();
                        document.Url = HEADER_SP_URL + idItemCitroUrl;
                        document.Id = item.Id.ToString();

                        var docToElastic = ConvertDocToElastic(document);

                        var result = PostClientIndex(document.Index, document.Type, document.Id, docToElastic);
                    }
                }

            }

            return true;
        }

        private bool IsBiTerm(String list)
        {
            if (list.Equals("BI Terms") || list.Equals("BI Terms JPTest") || list.Equals("BI Terms Old"))
                return true;
 
            return false;
        }

        private Object ConvertDocToElastic(ElasticDocument document)
        {
            var result = new
                        {
                            _search = new
                            {
                                name = document.Search.Name,
                                description = document.Search.Description,
                                owner = document.Search.Owner
                            },

                            _body = new {
                                name = document.Search.Name,
                                description = document.Search.Description,
                                owner = document.Search.Owner
                            },

                            _url = document.Url
                        };

            return result;
        }

        private bool UpdateFromSSRS()
        {
            ElasticDocument document = new ElasticDocument();
            String idItemSSRSUrl; 
            var itemsCatalog = GetAllCatalogSSRS();

            document.Index = SSRS;

            foreach(var itemCatalog in itemsCatalog)
            {
                document.Type = itemCatalog.ID;
                var items = GetCatalogItems(itemCatalog.Path);

                if (items!=null)
                {
                    foreach (var item in items)
                    {
                        document.Search.Name = item.Name;
                        document.Search.Description = item.Description;
                        document.Search.Owner = item.CreatedBy;

                        idItemSSRSUrl = SEPARATOR_URL + itemCatalog.Name + SEPARATOR_URL + item.Name;
                        document.Url = HEADER_SSRS_URL + idItemSSRSUrl;
                        document.Id = item.ID;

                        var docToElastic = ConvertDocToElastic(document);

                        var result = PostClientIndex(document.Index, document.Type, document.Id, docToElastic);

                    }
                }
            }

            return true;
        }

        private ListCollection GetAllCitropediaLists()
        {
            ListCollection collList = oWebsiteSharePoint.Lists;
            clientContextSharePoint.Load(collList);
            clientContextSharePoint.ExecuteQuery();

            /*foreach (var e in collList)
            {
                Debug.WriteLine(e);                                
            }*/

            return collList;
        }

        public IEnumerable<ElasticDocument> SearchFuzzy(String likeText)
        {

            /*var result = es.Search<ElasticDocument>(s=>s
                        .Query(q=>
                                q.Term(p=>p.Name, searchItem.Name)
                                 && q.Term(p=>p.Owner, searchItem.Owner)));*/
 

            /*var result = clientElasticNest.Search<ElasticDocument>(s => s
            .Query(q =>
                    q.FuzzyLikeThis(p => p.OnFields(f => f.Search)
                            .OnFields(f => f.Search)
                            .OnFields(f => f.Search.Owner)
                            .OnFields(f => f.Search.Description)
                        .LikeText(likeText)
                   )
              ));*/
            string fieldName = "_search.name";
            string fieldDescription = "_search.description";
            string fieldOwner = "_search.owner";

            IEnumerable<string> fields = new List<string>() { fieldName, fieldDescription, fieldOwner };
            
            var result = clientElasticNest.Search<ElasticDocument>(s => s
            .Query(q =>
                    q.FuzzyLikeThis(p => p.OnFields(fields).LikeText(likeText)
                   )
              ));

            return result.Documents;

        }

    }
}
