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

namespace CDM_SearchEngine
{
    public class SearchEngine
    {
        private static SearchEngine instance = null;    
        public ElasticsearchClient client;	    
        
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
	
	    private const bool PRINT_RAW_CONTENT = true;
	
	    /*private const String SERVICE_OD_URL = "http://localhost:8080/cars-annotations-sample/MyFormula.svc";*/
        private const String SERVICE_OD_URL_NORTH = "http://services.odata.org/Northwind/Northwind.svc/";
        private const String SERVICE_OD_URL_STORE = "http://services.odata.org/V2/(S(r2ir5rzsz3ygo1dahemljxgj))/OData/OData.svc/";
        private const String SERVICE_OD_URL_MDMPC = "http://service.citrite.net/entity/GenericOData/ods/mdmpartnercustomer";
        private const String INDEX_NORTH = "northwind";
        private const String INDEX_STORE = "store";
        private const String INDEX_MDMPC = "mdmpartnercustomer";
        private const String USER_DS = "t_leandrod1";
        private const String PASSWORD_DS = "tellago*7";
	    private const String USED_FORMAT = APPLICATION_JSON;
        public NorthwindEntities context;
        public MDMPartnerCustomerEntities contextMDM_pc;

        // Define the URI of the public Northwind OData service.
        private Uri northwindUri = new Uri(SERVICE_OD_URL_NORTH, UriKind.Absolute);
        private Uri storeUri = new Uri(SERVICE_OD_URL_STORE, UriKind.Absolute);
        private Uri mdmpcUri = new Uri(SERVICE_OD_URL_MDMPC, UriKind.Absolute);
        // Define the URI of the public ElasticSearch service.
        //private Uri hostEs = new Uri("http://192.168.0.186:9200", UriKind.Absolute);
        private Uri hostEs = new Uri("http://10.108.168.99:9200", UriKind.Absolute);        
        
        public SearchEngine() {	      
    		startupES();
            createInstanceOD();
            loadContextSharePoint();
            connectToSSRS();
	    }

        public static SearchEngine getInstance() {
		    if(instance == null) {	
			    instance = new SearchEngine();
    		}
		    return instance;
	    }

        private void startupES(){
    	// on startup    	    	    
            var settings = new ConnectionSettings(hostEs).SetDefaultIndex("peliculas");
            client = new ElasticsearchClient();// ElasticClient(settings);
        }

        public void createInstanceOD()
        {
            // Define the URI of the public Northwind OData service.
            context = new NorthwindEntities(northwindUri);
            contextMDM_pc = new MDMPartnerCustomerEntities(mdmpcUri);
            setCredentialsMDM();
        }

        public void loadContextSharePoint()
        {
            ClientContext clientContext = new ClientContext("http://sharepoint.citrite.net/sites/it/ea/DMO/Citropedia");///Lists/BI%20Term/AllItems.aspx");       
            clientContext.Credentials = new NetworkCredential(USER_DS , PASSWORD_DS);
            
            Web oWebsite = clientContext.Web;
            clientContext.Load(oWebsite);
            clientContext.ExecuteQuery();
            Console.WriteLine(oWebsite.Title);

            
        }

        public void connectToSSRS()
        {
            ReportingService2010 rs = new ReportingService2010();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            //rs.Url = "http://ftlpssrslb/reportserver/reportservice2010.asmx";
            rs.Url = "http://ftlpssrslb/Reports/Pages/Folder.aspx";
            
            Property name = new Property();
            name.Name = "Name";
            
            Property description = new Property();
            description.Name = "Description";

            Property[] properties = new Property[2];
            properties[0] = name;
            properties[1] = description;

            try
            {
                Property[] returnProperties = rs.GetProperties(
                "/Reports/Pages", properties);

                foreach (Property p in returnProperties)
                {
                    Console.WriteLine(p.Name + ": " + p.Value);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void setCredentialsMDM()
        {
            contextMDM_pc.Credentials = new NetworkCredential(USER_DS, PASSWORD_DS);
        }

        public int? getRequestStatus(String index, String type, String id, Object request)
        {
            return client.Get(index, type, id).HttpStatusCode;                
        }

        public int? postRequestStatus(String index, String type, String id, Object request)
        {
            var indexResponse = client.Index(index, type, id, request);
            return indexResponse.HttpStatusCode;
        }

        public bool postClientIndex(String index, String type, String id, Object request)
        {
            return client.Index(index, type, id, request).Success;
        }

        public bool existDocumentOnES(String index, String type, String id, Object request)
        {                        
            //string sss = getResponse.Response["_source"];            
            return client.Get(index, type, id).Success;           
        }

        public bool existDocumentOnOD(String index, String type, String id, Object request)
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

            return client.Get(index, type, id).Success;
        }   

    }
}
