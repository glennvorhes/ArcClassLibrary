﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
    }
}
