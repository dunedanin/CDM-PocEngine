using CDM_SearchEngine;
using CDM_SearchEngine.mdmpartnercustomer;
using Elasticsearch.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CDM_SearchEngineTest
{
    static class ScenariosTests
    {
        static SearchEngine myEngine;

        public static void setEngine(SearchEngine engine4Tests)
        {
            myEngine = engine4Tests;
        }

        public static String executeQueryPostalCodeOD(IQueryable<String> query)
        {
            return (String)query.First<String>();
        }

        public static String executeQueryMDMOD(IQueryable<String> query)
        {
            return (String)query.First<String>();
        }

        public static IQueryable<String> defineQueryPostalCodeOD()
        {
            // Create a LINQ query to get...            
            IQueryable<String> query = from o in myEngine.context.Customers
                                       where o.CustomerID == "ALFKI"
                                       select o.PostalCode;

            /*IQueryable query2 = from o in myEngine.context.Customers
                                        where o.PostalCode == "12209"
                                       select o;*/

            return query;            
        }

        public static IQueryable<string> defineQueryMDMOD_Country_Org_Name()
        {
            // Create a LINQ query to get...                        
            IQueryable<string> query = myEngine.contextMDM_pc.vw_Partner_Hierarchy_Customer.Where(
                a => a.Country_OrgID == "104662HQ" &&
                     a.Global_OrgID == "104662HQ" &&
                     a.Site_OrgID == "104662HQ"
            ).Select(a => a.Country_Org_Name);                        

            return query;;
        }

        public static int getQuerySingleMDM_PC()
        {
            // Create a LINQ query to get...                        
            return (myEngine.contextMDM_pc.vw_Partner_Hierarchy_Customer.Where(
                a => a.Country_OrgID == "104662HQ" &&
                     a.Global_OrgID == "104662HQ" &&
                     a.Site_OrgID == "104662HQ"
            ).Select(a => a).Count());

            /*foreach (vw_Partner_Hierarchy_Customer record in query)
            {
                Debug.WriteLine(record.Country_CustID);
                Debug.WriteLine(record.Country_Org_Name);
                Debug.WriteLine(record.Country_OrgID);
                Debug.WriteLine(record.Global_CustID);
                Debug.WriteLine(record.Global_Org_Name);
                Debug.WriteLine(record.Global_OrgID);
                Debug.WriteLine(record.Site_CustID);
                Debug.WriteLine(record.Site_OrgID);
                Debug.WriteLine(record.Site_OrgName);
            }*/
        }

        public static bool updateESFromQueryOD()
        {
            // Create a LINQ query to get...                        
            var query = (myEngine.contextMDM_pc.vw_Partner_Hierarchy_Customer.Where(
                a => a.Country_OrgID == "104662HQ" &&
                     a.Global_OrgID == "104662HQ" &&
                     a.Site_OrgID == "104662HQ"
            ).Select(a => a)).ToArray();
            
            var index = "mdmpartnercustomer";
            var type = "vw_Partner_Hierarchy_Customer";
            var id = "1";

            var request = new
            {
                Country_CustID = query[0].Country_CustID,
                Country_Org_Name = query[0].Country_Org_Name,
                Country_OrgID = query[0].Country_OrgID,
                Global_CustID = query[0].Global_CustID,
                Global_Org_Name = query[0].Global_Org_Name,
                Global_OrgID = query[0].Global_OrgID,
                Site_CustID = query[0].Site_CustID,
                Site_OrgID = query[0].Site_OrgID,
                Site_OrgName = query[0].Site_OrgName
            };
            searchAllIndexES();
            return myEngine.postClientIndex(index, type, id, request);
        }

        public static bool searchAllIndexES()
        {

            var index = "mdmpartnercustomer";
            var type = "vw_Partner_Hierarchy_Customer";
            var id = "1";
            
            dynamic d = new {a = "hola"};
            
            var c = myEngine.client.SearchGet(index,type);
            DynamicDictionary re = c.Response;
            var ddd = re.Values.ElementAt(3);
            return true;
        }
    }
}
