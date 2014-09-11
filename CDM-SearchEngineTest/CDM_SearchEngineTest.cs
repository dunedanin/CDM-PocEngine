using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDM_SearchEngine;
using Nest;
using Microsoft.SharePoint.Client;
using System.Linq;
using System.Diagnostics;

namespace CDM_SearchEngineTest
{
    [TestClass]
    public class SearchEngineTests
    {
        SearchEngine myEngine = SearchEngine.getInstance();
        Web oWebSP;
        ClientContext ctxSP;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            ScenariosTests.setEngine(myEngine);
            ScenariosTests.setContextSharePoint();
            oWebSP = ScenariosTests.oWebSiteSharePoint;
            ctxSP = ScenariosTests.ctxSharePoint;
        }

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

            Assert.AreEqual(201, myEngine.postRequestStatus(index, type, id, request));
        }

        [TestMethod]
        public void testGetRequestOld()
        {
            //Get request and update
            var myEngine = SearchEngine.getInstance();

            var index = "utn";
            var type = "tacs";
            var id = "1";

            var request = new
            {
                name = "Martin",
                file = "1186103",
                year = "2014"
            };

            Assert.AreEqual(200, myEngine.getRequestStatus(index, type, id, request));
        }

        [TestMethod]
        public void testGetRequestExistAndUpdate()
        {
            //Get request and update
            var myEngine = SearchEngine.getInstance();

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
            myEngine.postRequestStatus(index, type, id, request);
            Assert.IsTrue(myEngine.existDocumentOnES(index, type, id, request));
        }

        [TestMethod]
        public void testSearchCustomerOData()
        {

            var query = ScenariosTests.defineQueryPostalCodeOD();

            Assert.AreEqual("12209", ScenariosTests.executeQueryPostalCodeOD(query));
        }

        [TestMethod]
        public void testSearchNewOnElastic()
        {
            //new Search, exist on OData but not on ES--> add on ES
            var query = ScenariosTests.defineQueryPostalCodeOD();

            Assert.AreEqual("12209", ScenariosTests.executeQueryPostalCodeOD(query));
        }

        [TestMethod]
        public void testMDMOD_PartnerCustomer()
        {

            var query = ScenariosTests.defineQueryMDMOD_Country_Org_Name();

            Assert.AreEqual("Atea Finland Oy", ScenariosTests.executeQueryMDMOD(query));
        }


        [TestMethod]
        public void testSingleRecord_MDM()
        {
            Assert.AreEqual(1, ScenariosTests.getQuerySingleMDM_PC());
        }

        [TestMethod]
        public void testUpdateESFromOD()
        {
            Assert.IsTrue(ScenariosTests.updateESFromQueryOD());
        }

        [TestMethod]
        public void testReadFromSharePointBI_Term_Account()
        {
            var context = myEngine.clientContextSharePoint;
            var webSite = myEngine.oWebsiteSharePoint;
            List docList = webSite.Lists.GetByTitle("BI Terms");
            context.Load(docList);
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View/>";
            //camlQuery.ViewXml = "<View Scope='RecursiveAll'></View>";

            ListItemCollection listItems = docList.GetItems(camlQuery);
            
            context.Load(listItems);
            
            context.ExecuteQuery();            
            /*foreach (ListItem listItem in listItems)
                Debug.WriteLine("Id: {0} Title: {1}", listItem.Id, listItem["Title"]); */
            
            Assert.AreEqual("Advising Partner DUNS", listItems[1]["Title"]);
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
            var context = myEngine.clientContextSharePoint;
            var webSite = myEngine.oWebsiteSharePoint;
            Guid id = new Guid("1d46670b-c932-44c1-88dd-6e30479bb759");            
            //List citrixCatalog = webSite.Lists.GetByTitle("Citrix Data Catalog");
            List citrixCatalog = webSite.Lists.GetById(id);

            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View/>";

            ListItemCollection itemsCitrixCatalog = citrixCatalog.GetItems(camlQuery);
            context.Load(itemsCitrixCatalog);
            context.ExecuteQuery();

            /*foreach (ListItem listItem in listItems)
                Debug.WriteLine("Id: {0} Title: {1}", listItem.Id, listItem["Title"]); */
            var doc = new
            {
                id = itemsCitrixCatalog[1].Id,
                name = itemsCitrixCatalog[1]["Title"],
                description = itemsCitrixCatalog[1]["Description"],
                owner = itemsCitrixCatalog[1]["System_x0020_Owner"],
                _url = "http://sharepoint.citrite.net/sites/it/ea/DMO/Citropedia/Lists/Data%20Catalog/DispForm.aspx?ID=2&ContentTypeId=0x010091410F034BE2CF40B791C07AB1414330"
            };
            
            var result = myEngine.postClientIndex("citropedia", "citrix_data_catalog", id.ToString(), doc);
            Assert.IsTrue(result);
        }



    }
}

