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

namespace CDM_SearchEngine
{
    public class SearchEngine
    {
        private static SearchEngine instance = null;    
        private ElasticsearchClient clientElastic;        
        private Web oWebsiteSharePoint;
        private NorthwindEntities contextNorth;
        private MDMPartnerCustomerEntities contextMDM_pc;
        private ClientContext clientContextSharePoint;
        private ReportingService2010SoapClient clientSoap;
        // Define the URI of the public ElasticSearch service.
        private Uri hostElastic = new Uri("http://10.108.168.99:9200", UriKind.Absolute);
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
        private const String TOTAL_HITS = "total";
	    private const bool PRINT_RAW_CONTENT = true;
        private const String PARENT_URL_CITROPEDIA = "/sites/it/ea/DMO/Citropedia/Lists";
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

        /*static void Main()
        {            
            SearchEngine test = new SearchEngine();
            test.startUpSSRS();
            
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
            var settings = new ConnectionSettings(hostElastic).SetDefaultIndex("citropedia");
            clientElastic = new ElasticsearchClient();            
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

        public ElasticsearchDynamicValue[] SearchByOR(String p_name, String p_description, String p_owner)
        {
            ElasticsearchDynamicValue[] response = null;
            Func<SearchRequestParameters, SearchRequestParameters> requestParameters;
            SearchRequestParameters request = new SearchRequestParameters();

            ObjectSearch objectToSearch = new ObjectSearch(p_name, p_description, p_owner);

            var UNIONOR = SPACE + OR + SPACE;
            String document = GenerateDocumentUri(objectToSearch, UNIONOR);
                                                
            request.AddQueryString(QUERYI, document);
            requestParameters = s => s = request;
            var results = clientElastic.SearchGet(requestParameters);

            int total_hits = (int) results.Response[HITS][TOTAL_HITS];
            ElasticsearchDynamicValue hits = results.Response[HITS][HITS];

            if (total_hits>0)
                response = new ElasticsearchDynamicValue[total_hits];
            else
                response = new ElasticsearchDynamicValue[0];

            for (int i = 0; i < total_hits; i++)
            {                
                response[i] = hits[i];
            }

            return response;
        }

        public ElasticsearchDynamicValue[] SearchByAND(String p_name, String p_description, String p_owner)
        {
            ElasticsearchDynamicValue[] response = null;
            Func<SearchRequestParameters, SearchRequestParameters> requestParameters;
            SearchRequestParameters request = new SearchRequestParameters();

            ObjectSearch objectToSearch = new ObjectSearch(p_name, p_description, p_owner);

            var UNIONAND = SPACE + AND + SPACE;
            String document = GenerateDocumentUri(objectToSearch, UNIONAND);

            request.AddQueryString(QUERYI, document);
            requestParameters = s => s = request;
            var results = clientElastic.SearchGet(requestParameters);

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
        private String GenerateDocumentUri(ObjectSearch objectToSearch, String unionSearch)
        {
            String document = null;

            if (CheckSearchValue(objectToSearch.name))
                document = NAME + DP + objectToSearch.name.Trim() + unionSearch;

            if (CheckSearchValue(objectToSearch.description))
                document = document + DESCRIPTION + DP + objectToSearch.description.Trim();

            if (CheckSearchValue(objectToSearch.owner))
                document = document + OWNER + DP + objectToSearch.owner.Trim();
            
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
            clientSoap.ListChildren(Myheader, itemPath, true, out catalogItems);
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
            GetAllCitropediaLists();
            return true;
        }

        private bool UpdateFromSSRS()
        {
            return true;
        }

        private ListCollection GetAllCitropediaLists()
        {
            ListCollection collList = oWebsiteSharePoint.Lists;
            clientContextSharePoint.Load(collList);
            clientContextSharePoint.ExecuteQuery();

            foreach (var e in collList)
            {
                Debug.WriteLine(e);                
                //PARENT_URL_CITROPEDIA
                //e.DefaultViewUrl
                    //	"/sites/it/ea/DMO/Citropedia/Lists/Approver Groups/AllItems.aspx"

            }

            return collList;

        }

    }
}
