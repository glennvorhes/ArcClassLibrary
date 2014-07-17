using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Enbridge.PLM;

namespace UnitTests
{
    [TestClass]
    public class PLM_UnitTests
    {
        [TestMethod]
        public void TestLoadPLMReportRoot()
        {
            Enbridge.PLM.PlmReport report = new Enbridge.PLM.PlmReport();

            report.reportProperties.setReportProperties("glenn", Guid.NewGuid(), "yes", "no", null, null, Guid.NewGuid(), "new report");

            report.setIsForeignCrossing(false);

            report.foreignCrossing.setForeignCrossingValues(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "yes", "no", "no", "no", DateTime.Now, "glenn", "ticket", "other");

            report.permanentRepair.setTab1Values(DateTime.Now, DateTime.Now, "yes", "broke", "20", "4", "sweet", "2", "thick", "a house", "no");
            report.permanentRepair.setTab2Values("yes", "2342", "remark", "no", DateTime.MinValue, "45", "-92", "no", "here", "fittings");
            report.corrosionInspection.setTab1Values(Guid.NewGuid(), Guid.NewGuid(), "good", "joe", DateTime.MinValue, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "some");
            report.corrosionInspection.setTab2Values(Guid.NewGuid(), Guid.NewGuid(), "20");
            report.corrosionInspection.setTab3Values(Guid.NewGuid(), "some", "adsf", "adsfa", Guid.NewGuid(), "10", "small");
            report.rowInfo.setLocationTabValues("tract", "district", "lineSection", Guid.NewGuid(), Guid.NewGuid(), "descript", "section", "town", "range");
            report.rowInfo.setTenantTabValues("glenn", "4444", "45555", Guid.NewGuid(), "comments", "yes");
            report.rowInfo.setAccessTabValues(Guid.NewGuid(), "yes", "40", "20", "no", "yes", "yes");
            report.rowInfo.setWorkAreaTabValues("50", "40", "yes", "no", "no");
            report.pointFeatures.addFeatureByStn("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}", "{791FC1FB-47C2-4DEB-97A8-7B90FEE38921}", "desc", 1000);

            bool success = report.saveReport();
            
            Assert.IsTrue(success, "something went wrong");
        }



        [TestMethod]
        public void AddPointGeometryByStn()
        {
            Enbridge.PLM.PlmReport report = new Enbridge.PLM.PlmReport();
            report.reportProperties.setReportProperties("glenn", Guid.NewGuid(), "yes", "no", null, null, Guid.NewGuid(), "new report");
            report.pointFeatures.addFeatureByStn("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}", Guid.NewGuid().ToString(), "desc", 1000);

            bool success = report.saveReport();

            Assert.IsTrue(success, "something went wrong");
        }

        [TestMethod]
        public void AddPointGeometryByStnAndMP()
        {
            Enbridge.PLM.PlmReport report = new Enbridge.PLM.PlmReport();
            report.reportProperties.setReportProperties("glenn", Guid.NewGuid(), "yes", "no", null, null, Guid.NewGuid(), "new report");
            report.pointFeatures.addFeatureByStn("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}", Guid.NewGuid().ToString(), "desc3", 1000);
            report.pointFeatures.addFeatureByMP("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}", Guid.NewGuid().ToString(), "desc2", 800);
            report.pointFeatures.addFeatureByLonLat("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}", Guid.NewGuid().ToString(), "desc1", -97.159, 48.734);

            bool success = report.saveReport();

            Assert.IsTrue(success, "something went wrong");
        }

        [TestMethod]
        public void TestAddLinear()
        {
            LinearFeatures linearFeats = new LinearFeatures();

            bool success = linearFeats.addFeatureByMP("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}", Guid.NewGuid().ToString(), "desc", 1000, 1001);
            bool saveSuccess = linearFeats.saveToDatabase(Guid.NewGuid().ToString());
            Assert.IsTrue(success);
            Assert.IsTrue(saveSuccess);
        }

        [TestMethod]
        public void TestAddPoint()
        {
            PointFeatures pointFeats = new PointFeatures();

            bool success = pointFeats.addFeatureByMP("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}", Guid.NewGuid().ToString(), "desc", 1010);
            bool saveSuccess = pointFeats.saveToDatabase(Guid.NewGuid().ToString());
            Assert.IsTrue(success);
            Assert.IsTrue(saveSuccess);
        }

        [TestMethod]
        public void getExistingFeatures()
        {
            PointFeatures pointFeats = new PointFeatures("{C2031F6A-A596-41FA-B87D-A7A03AA0F435}");
            LinearFeatures linearFeats = new LinearFeatures("{C2031F6A-A596-41FA-B87D-A7A03AA0F435}");
            Assert.AreEqual(pointFeats.existingFeaturesDataItems.Count, 3);
            Assert.AreEqual(linearFeats.existingFeaturesDataItems.Count, 3);
        }

        [TestMethod]
        public void createExisting()
        {
            PlmReport report = new PlmReport("{C2031F6A-A596-41FA-B87D-A7A03AA0F435}");
            Assert.AreEqual(report.pointFeatures.existingFeaturesDataItems.Count, 3);
            Assert.AreEqual(report.linearFeatures.existingFeaturesDataItems.Count, 3);
        }
    }
}
