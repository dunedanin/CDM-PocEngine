﻿using System;
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
        public ElasticsearchClient clientElastic;
        public ElasticClient clientElasticNest;
        public Web oWebsiteSharePoint;
        public NorthwindEntities contextNorth;
        public MDMPartnerCustomerEntities contextMDM_pc;
        public ClientContext clientContextSharePoint;
        public ReportingService2010SoapClient clientSoap;
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
	    //private const String SERVICE_OD_URL = "http://localhost:8080/cars-annotations-sample/MyFormula.svc";
        private const String SERVICE_OD_URL_NORTH = "http://services.odata.org/Northwind/Northwind.svc/";
        private const String SERVICE_OD_URL_STORE = "http://services.odata.org/V2/(S(r2ir5rzsz3ygo1dahemljxgj))/OData/OData.svc/";
        private const String SERVICE_OD_URL_MDMPC = "http://service.citrite.net/entity/GenericOData/ods/mdmpartnercustomer";
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

        public SearchEngine() {	      
    		startupElasticSearch(); //ElasticSearch
            startUpOData(); //OData
            startUpSharePoint();//Citropedia
            startUpSSRS(); //SSRS
	    }

        //Singleton instance
        public static SearchEngine getInstance() {
		    if(instance == null) {	
			    instance = new SearchEngine();
    		}
		    return instance;
	    }

        private void startupElasticSearch(){
            var settings = new ConnectionSettings(hostElastic).SetDefaultIndex("citropedia");
            clientElastic = new ElasticsearchClient();
            clientElasticNest = new ElasticClient(settings);

        }

        public void startUpOData()
        {
            // Define the URI of the public Northwind OData service.
            contextNorth = new NorthwindEntities(northwindUri);
            contextMDM_pc = new MDMPartnerCustomerEntities(mdmpcUri);            
            setCredentialsMDM();
        }

        public void startUpSharePoint()
        {
            clientContextSharePoint = new ClientContext("http://sharepoint.citrite.net/sites/it/ea/DMO/Citropedia");///Lists/BI%20Term/AllItems.aspx");       
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

        public void startUpSSRS()
        {
            try
            {
              clientSoap = new ReportingService2010SoapClient(); 
              clientSoap.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(USER_DS, PASSWORD_DS);
              clientSoap.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

              TrustedUserHeader Myheader = new TrustedUserHeader();
              string reportName = "/CMD/AccountPenetrationReport";
              byte[] reportDefinition = null;
              clientSoap.GetItemDefinition(Myheader, reportName, out reportDefinition);

             
                
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

        public void setCredentialsMDM()
        {
            contextMDM_pc.Credentials = new NetworkCredential(USER_DS, PASSWORD_DS);
        }

        public int? getRequestStatus(String index, String type, String id, Object request)
        {
            return clientElastic.Get(index, type, id).HttpStatusCode;                
        }

        public int? postRequestStatus(String index, String type, String id, Object request)
        {
            var indexResponse = clientElastic.Index(index, type, id, request);
            return indexResponse.HttpStatusCode;
        }

        public bool postClientIndex(String index, String type, String id, Object request)
        {
            return clientElastic.Index(index, type, id, request).Success;
        }

        public bool existDocumentOnES(String index, String type, String id, Object request)
        {                        
            //string sss = getResponse.Response["_source"];            
            return clientElastic.Get(index, type, id).Success;           
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

            return clientElastic.Get(index, type, id).Success;
        }

        public ElasticsearchDynamicValue[] SearchByOR(String p_name, String p_description, String p_owner)
        {
            ElasticsearchDynamicValue[] response = null;
            Func<SearchRequestParameters, SearchRequestParameters> requestParameters;
            SearchRequestParameters request = new SearchRequestParameters();
            
            var UNIONOR = SPACE + OR + SPACE;
            String document = null;

            if (checkSearchValue(p_name))
                document = NAME + DP + p_name + UNIONOR;

            if (checkSearchValue(p_description))
                document = document + DESCRIPTION + DP + p_description;

            if (checkSearchValue(p_owner))
                document = document + OWNER + DP + p_owner;
                        
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

        private bool checkSearchValue(String value)
        {
            if (value == null || value.Trim() == EMPTY)
                return false;

            return true;
        }


    }
}
