using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CDM_SearchEngine;
using Nest;
//using CDM_SearchEngine.Northwind;
using System.Linq;

namespace CDM_SearchEngineTest
{
    [TestClass]
    public class SearchEngineTests
    {
        SearchEngine myEngine = SearchEngine.getInstance();

        [TestInitialize()]
        public void MyTestInitialize()
        {
            ScenariosTests.setEngine(myEngine);
        }

        [TestMethod]
        public void testGetRequestNew()
        {
            //Get request and it is created as new because it is not in ES
            
            var index = "utn";
            var type = "tacs";

            Random rnd = new Random();
            var id = rnd.Next(2, 1400000).ToString(); // creates a number between 1 and 12

            var request = new {                
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
    }
}

