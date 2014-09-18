using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDM_SearchEngine;
using Nest;
using Microsoft.SharePoint.Client;
using System.Linq;
using System.Diagnostics;
using CDM_SearchEngine.ftlpssrslb;
using Elasticsearch.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace CDM_SearchEngineTest
{
    [TestClass]
    public class SearchEngineTests
    {
        private const String OR = "OR";
        private const String AND = "AND";

        SearchEngine myEngine = SearchEngine.GetInstance();        

        [TestMethod]
        public void testGetRequestNew()
        {
            //Get request and it is created as new because it is not in ES

            var index = "utn";
            var type = "tacs";

            Random rnd = new Random();
            var id = rnd.Next(2, 1400000).ToString(); // creates a number between 1 and 12

            var request = new
            {
                name = "Martin",
                file = "1186103",
                year = "2010"
            };

            Assert.AreEqual(201, myEngine.PostRequestStatus(index, type, id, request));
        }

        [TestMethod]
        public void testGetRequestOld()
        {
            //Get request and update
            var myEngine = SearchEngine.GetInstance();

            var index = "utn";
            var type = "tacs";
            var id = "1";

            var request = new
            {
                name = "Martin",
                file = "1186103",
                year = "2014"
            };

            Assert.AreEqual(200, myEngine.GetRequestStatus(index, type, id, request));
        }

        [TestMethod]
        public void testGetRequestExistAndUpdate()
        {
            //Get request and update
            var myEngine = SearchEngine.GetInstance();

            var index = "utn";
            var type = "tacs";
            var id = "1";

            Random rnd = new Random();
            var fileRan = rnd.Next(2, 1400000).ToString();

            var request = new
            {
                name = "Martin",
                file = fileRan.ToString(),
                year = "2014"
            };
            myEngine.PostRequestStatus(index, type, id, request);
            Assert.IsTrue(myEngine.CheckDocumentOnES(index, type, id, request));
        }

        /*[TestMethod]
        public void testSearchCustomerOData()
        {

            var query = ScenariosTests.defineQueryPostalCodeOD();

            Assert.AreEqual("12209", ScenariosTests.executeQueryPostalCodeOD(query));
        }*/

        /*[TestMethod]
        public void testSearchNewOnElastic()
        {
            //new Search, exist on OData but not on ES--> add on ES
            var query = ScenariosTests.defineQueryPostalCodeOD();

            Assert.AreEqual("12209", ScenariosTests.executeQueryPostalCodeOD(query));
        }*/

        /*[TestMethod]
        public void testMDMOD_PartnerCustomer()
        {

            var query = ScenariosTests.defineQueryMDMOD_Country_Org_Name();

            Assert.AreEqual("Atea Finland Oy", ScenariosTests.executeQueryMDMOD(query));
        }*/

        /*[TestMethod]
        public void testSingleRecord_MDM()
        {
            Assert.AreEqual(1, ScenariosTests.getQuerySingleMDM_PC());
        }*/

        /*[TestMethod]
        public void testUpdateESFromOD()
        {
            Assert.IsTrue(ScenariosTests.updateESFromQueryOD());
        }*/

        [TestMethod]
        public void testReadFromSharePointBI_Term_Account()
        {
            Debug.WriteLine(myEngine.SearchOnSP("BI Terms")[1]["Short_x0020_Description"]);
            Assert.AreEqual("Advising Partner DUNS", myEngine.SearchOnSP("BI Terms")[1]["Title"]);
        }

        /*[TestMethod]
        public void testUpdateCitropediaToElastic()
        {
            ListCollection docList = oWebSP.Lists;
            ctxSP.Load(docList);
            ctxSP.ExecuteQuery();
            
            foreach (List list in docList)
            {    
                
                //Debug.WriteLine("Id: {0} Title: {1}", list.Id, list.Title);
             }
            Assert.AreEqual("Citrix Data Catalog", docList[8].Title);
        }*/

        [TestMethod]
        public void testUpdateCitropediaToElasticItem1_FromCitrixCatalog()
        {
            
            Guid id = new Guid("1d46670b-c932-44c1-88dd-6e30479bb759");
           
            var itemsCitrixCatalog = myEngine.SearchOnSPById(id);

            var doc = new
            {
                id = itemsCitrixCatalog[1].Id,
                name = itemsCitrixCatalog[1]["Title"],
                description = itemsCitrixCatalog[1]["Description"],
                owner = itemsCitrixCatalog[1]["System_x0020_Owner"],
                _url = "http://sharepoint.citrite.net/sites/it/ea/DMO/Citropedia/Lists/Data%20Catalog/DispForm.aspx?ID=2&ContentTypeId=0x010091410F034BE2CF40B791C07AB1414330"
            };
            
            var result = myEngine.PostClientIndex("citropedia", "citrix_data_catalog", id.ToString(), doc);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void testConnectToAndReadFromSSRS()
        {
            // Return a list of catalog items in the report server database                    
            var catalogItems = myEngine.GetCatalogItems("/CMD");

            // For each report, display the path of the report in a Listbox
            foreach (var ci in catalogItems)
            {
                Debug.WriteLine(ci.Name);
                Debug.WriteLine(ci.ItemMetadata);
            }

            Assert.AreEqual("AccountPenetrationReport", catalogItems[0].Name);
            Assert.AreEqual("/CMD/AccountPenetrationReport", catalogItems[0].Path);
        }

        [TestMethod]
        public void testSearchByNameReturnString()
        {                        
            ElasticDocument document = new ElasticDocument();            
            document.Search.Owner = "garcia";
            document.Index = "citropedia";
            document.Type = "bi_term";

            SearchCriteria criteria = new SearchCriteria(OR);
            
            var results = myEngine.Search(document, criteria);

            Assert.AreEqual(1, results.Length);
            
        }    

            [TestMethod]
        public void testSearchByNameNotFound()
        {

            ElasticDocument document = new ElasticDocument();
            document.Search.Owner = "garzxscia";
            document.Index = "citropedia";
            document.Type = "bi_term";

            SearchCriteria criteria = new SearchCriteria(OR);

            var results = myEngine.Search(document, criteria);

            Assert.AreEqual(0, results.Length);
        }

            [TestMethod]
            public void testSearchByNameOnRoot()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "garzxscia";

                SearchCriteria criteria = new SearchCriteria(OR);

                var results = myEngine.Search(document, criteria);

                Assert.AreEqual(0, results.Length);               

            }

            [TestMethod]
            public void testSearchByNameFromPortal()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "garcia";
                document.Search.Name = "account";

                SearchCriteria criteria = new SearchCriteria(OR);

                var results = myEngine.Search(document, criteria);
                
                foreach(var e in results)
                    Debug.WriteLine(e.ToString());

                Assert.AreEqual(2, results.Length);
            }

            [TestMethod]
            public void testSearchByNameTwoHitFromPortal()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "Dagmar  garcia";

                SearchCriteria criteria = new SearchCriteria(OR);

                var results = myEngine.Search(document, criteria);
                Assert.AreEqual(2, results.Length);
            }

            [TestMethod]
            public void testSearchByNameHitWeirdFromPortal()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "Dagmar  garccia";
                document.Search.Name = "juan";

                SearchCriteria criteria = new SearchCriteria(OR);

                var results = myEngine.Search(document, criteria);
                Assert.AreEqual(1, results.Length);
            }

            [TestMethod]
            public void testSearchByNameNoHitFromPortal()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "garccia";
                document.Search.Name = "juan";

                SearchCriteria criteria = new SearchCriteria(OR);

                var results = myEngine.Search(document, criteria);
                Assert.AreEqual(0, results.Length);
            }

            [TestMethod]
            public void testSearchByANDNoResultFromPortal()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "Dagmar  garcia";
                document.Search.Name = "pepe";

                SearchCriteria criteria = new SearchCriteria(AND);

                var results = myEngine.Search(document, criteria);
                Assert.AreEqual(0, results.Length);
            }

            [TestMethod]
            public void testSearchByANDFoundFromPortal()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "Dagmar  garcia";
                document.Search.Name = "account";

                SearchCriteria criteria = new SearchCriteria(AND);

                var results = myEngine.Search(document, criteria);
                Debug.WriteLine(results[0]);
                Assert.AreEqual(1, results.Length);
            }

            [TestMethod]
            public void testSearchByANDBadFromPortal()
            {
                ElasticDocument document = new ElasticDocument();
                document.Search.Owner = "garcia";
                document.Search.Name = "accxount";

                SearchCriteria criteria = new SearchCriteria(AND);

                var results = myEngine.Search(document, criteria);
                Assert.AreEqual(0, results.Length);
            }

            [TestMethod]
            public void testSearchFuzzyNoResult()
            {
                var myLikeThisSearch = "fazke";

                var results = myEngine.SearchFuzzy(myLikeThisSearch);
                Assert.AreEqual(2, results.Count());
            }

            [TestMethod]
            public void testUpdateElasticFromDSS()
            {
                Assert.IsTrue(myEngine.UpdateElastic());
            }

            [TestMethod]
            public void testSearchFuzzyWithResult()
            {
                var myLikeThisSearch = "Affiliate (System Integrator)";

                var results = myEngine.SearchFuzzy(myLikeThisSearch);
                Assert.AreEqual(7, results.Count());
            }

            [TestMethod]
            public void testSearchFuzzyWithThreeResult()
            {
                var myLikeThisSearch = "account";

                var results = myEngine.SearchFuzzy(myLikeThisSearch);
                Assert.AreEqual(10, results.Count());
            }
    }
}

