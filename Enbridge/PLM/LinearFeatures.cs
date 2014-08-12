using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geocortex.Forms.Client;

namespace Enbridge.PLM
{
    /// <summary>
    /// two lists of LinearFeat as defined by the private class
    /// </summary>
    [Serializable]
    public class LinearFeatures
    {

        private List<LinearFeat> existingFeaturesList;
        private List<LinearFeat> pendingFeaturesList;
        public List<DataItem> existingFeaturesDataItems;
        private List<string> pendingDelete;


        /// <summary>
        /// Create the LinearFeatures object
        /// </summary>
        /// <param name="reportId">optional reportId property for existing reports</param>
        public LinearFeatures(string reportId = null)
        {
            this.existingFeaturesList = new List<LinearFeat>();
            this.pendingFeaturesList = new List<LinearFeat>();
            this.existingFeaturesDataItems = new List<DataItem>();
            this.pendingDelete = new List<string>();


            if (reportId != null)
            {
                using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
                {
                    conn.Open();
                    SqlCommand comm = conn.CreateCommand();

                    comm.CommandText = "";
                    comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                    comm.CommandText += "Select *, Shape.STAsText() As geomWKT FROM sde.LINEAR_FEATURE_EVW linearTable ";
                    comm.CommandText += "LEFT JOIN sde.LINEAR_FEATURE_TYPE_EVW lType ";
                    comm.CommandText += "ON linearTable.FeatureType = lType.ID ";
                    comm.CommandText += "WHERE linearTable.ReportID = @ReportID;";

                    comm.Parameters.AddWithValue("@ReportID", reportId);

                    try
                    {
                        SqlDataReader reader = comm.ExecuteReader();

                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString();
                            string type = reader["Type"].ToString();
                            string desc = reader["Description"].ToString();
                            string display = string.Format("{0}: {1}", type, desc);

                            this.existingFeaturesDataItems.Add(
                                new DataItem(display, id)
                                );

                            this.existingFeaturesList.Add(new LinearFeat(reader));
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        comm.Dispose();
                        conn.Close();
                    }
                }
            }

        }

        /// <summary>
        /// Add a point feature
        /// </summary>
        /// <param name="routeId"></param>
        /// <param name="featureType"></param>
        /// <param name="description"></param>
        /// <param name="stn"></param>
        /// <returns></returns>
        //public bool addFeatureByStn(string routeId, string featureType, string description, double stn)
        //{
        //    Enbridge.LinearReferencing.ContLineLocatorSQL loc = new Enbridge.LinearReferencing.ContLineLocatorSQL(routeId);

        //    double mp, meas, X, Y, Z;
        //    mp = loc.getMPFromStn(stn, out meas, out X, out Y, out Z);
        //    string stnSeriesId = loc.getLocation(X, Y);

        //    this.pendingFeaturesList.Add(new PointFeat(routeId, stnSeriesId, stn, mp, featureType, Y, X, description, Z));
        //    return true;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeId"></param>
        /// <param name="featureType"></param>
        /// <param name="description"></param>
        /// <param name="mp"></param>
        /// <returns></returns>
        public bool addFeatureByMP(string routeId, string featureType, string description, double startMP, double endMP)
        {
            Enbridge.LinearReferencing.ContLineLocatorSQL loc = new Enbridge.LinearReferencing.ContLineLocatorSQL(routeId);
            double stn1, meas1, X1, Y1, Z1, stn2, meas2, X2, Y2, Z2;
            stn1 = loc.getStnFromMP(startMP, out meas1, out X1, out Y1, out Z1);
            stn2 = loc.getStnFromMP(endMP, out meas2, out X2, out Y2, out Z2);
            string stnSeriesId1 = loc.getLocation(X1, Y1);
            string stnSeriesId2 = loc.getLocation(X2, Y2);

            string geomText = loc.makeSegmentLineString(meas1, meas2);
            Console.WriteLine("Geometry Text {0}", geomText);

            this.pendingFeaturesList.Add(
                new LinearFeat(routeId, stnSeriesId1, stnSeriesId2, 
                    stn1, stn2, startMP, endMP, featureType, 
                    Y1, X1, Y2, X2, description, geomText));
            return true;
        }

        //public bool addFeatureByLonLat(string routeId, string featureType, string description, double lon, double lat)
        //{
        //    Enbridge.LinearReferencing.ContLineLocatorSQL loc = new Enbridge.LinearReferencing.ContLineLocatorSQL(routeId);
        //    double stn, meas, X, Y, Z, mp;
        //    string stnSeriesId = loc.getLocation(lon, lat, out stn, out meas, out mp);
        //    loc.getStnFromMP(mp, out meas, out X, out Y, out Z);

        //    this.pendingFeaturesList.Add(new PointFeat(routeId, stnSeriesId, stn, mp, featureType, Y, X, description, Z));
        //    return true;
        //}

        
        public bool deleteFeature(string featId)
        {

            int removeIndex = -1;
            for (int i = 0; i < this.existingFeaturesList.Count; i++)
            {
                if (this.existingFeaturesList[i].ID.ToLower() == featId.ToLower())
                {
                    removeIndex = i;
                }
            }

            if (removeIndex != -1)
            {
                this.existingFeaturesList.RemoveAt(removeIndex);
            }

            removeIndex = -1;

            for (int i = 0; i < this.existingFeaturesDataItems.Count; i++)
            {
                if (this.existingFeaturesDataItems[i].Value.ToString().ToLower() == featId.ToLower())
                {
                    removeIndex = i;
                }
            }

            if (removeIndex != -1)
            {
                this.existingFeaturesDataItems.RemoveAt(removeIndex);
            }


            this.pendingDelete.Add(featId);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportID"></param>
        /// <returns></returns>
        public string saveToDatabase(string reportID)
        {
            Console.WriteLine("linear feature count {0}", this.pendingFeaturesList.Count);
            string errors = "";

            if (this.pendingFeaturesList.Count == 0 && this.pendingDelete.Count == 0)
            {
                return errors;
            }

            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                comm.CommandText += "BEGIN TRANSACTION;";
                comm.CommandText += "DELETE FROM sde.LINEAR_FEATURE_EVW WHERE ID = @ID;";
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                foreach (string featId in this.pendingDelete)
                {
                    comm.Parameters.AddWithValue("@ID", featId);
                    try
                    {
                        comm.ExecuteNonQuery();

                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                        errors += ex.Message;
                        conn.Close();
                        return errors;
                    }
                }

                comm.CommandText = "";
                comm.CommandText += "Declare @geom geometry;";
                comm.CommandText += "SET @geom = geometry::STLineFromText(@geom_text, 4326).MakeValid();";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                comm.CommandText += "BEGIN TRANSACTION;";
                comm.CommandText += "INSERT INTO sde.LINEAR_FEATURE_EVW ";
                comm.CommandText += "(ID, Shape, ReportID, RouteID, StationSeriesIDStart, StationSeriesIDEnd, ";
                comm.CommandText += "DateAdded, StationingStart, StationingEnd, MilePostStart, MilePostEnd, FeatureType, ";
                comm.CommandText += "LatitudeStart, LongitudeStart, LatitudeEnd, LongitudeEnd, Description) ";
                comm.CommandText += "VALUES ";
                comm.CommandText += "(@ID, @geom, @ReportID, @RouteID, @StationSeriesIDStart, @StationSeriesIDEnd, ";
                comm.CommandText += "GETDATE(), @StationingStart, @StationingEnd, @MilePostStart, @MilePostEnd, @FeatureType, ";
                comm.CommandText += "@LatitudeStart, @LongitudeStart, @LatitudeEnd, @LongitudeEnd, @Description);";
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";



                foreach (LinearFeat feat in this.pendingFeaturesList)
                {
                    //comm.CommandText = String.Format(commandString, feat.geomString);
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@ID", feat.ID);
                    comm.Parameters.AddWithValue("@geom_text", feat.geomString);
                    comm.Parameters.AddWithValue("@ReportID", reportID);
                    comm.Parameters.AddWithValue("@RouteID", feat.routeId);
                    comm.Parameters.AddWithValue("@StationSeriesIDStart", feat.stnSeriesIdStart);
                    comm.Parameters.AddWithValue("@StationSeriesIDEnd", feat.stnSeriesIdEnd);
                    comm.Parameters.AddWithValue("@StationingStart", feat.stationingStart);
                    comm.Parameters.AddWithValue("@StationingEnd", feat.stationingEnd);
                    comm.Parameters.AddWithValue("@MilePostStart", feat.milePostStart);
                    comm.Parameters.AddWithValue("@MilePostEnd", feat.milePostEnd);
                    comm.Parameters.AddWithValue("@FeatureType", feat.featureType);
                    comm.Parameters.AddWithValue("@LatitudeStart", feat.latitudeStart);
                    comm.Parameters.AddWithValue("@LongitudeStart", feat.longitudeStart);
                    comm.Parameters.AddWithValue("@LatitudeEnd", feat.latitudeEnd);
                    comm.Parameters.AddWithValue("@LongitudeEnd", feat.longitudeEnd);
                    comm.Parameters.AddWithValue("@Description", feat.description);

                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                        errors += ex.Message;
                        conn.Close();
                        return errors;
                    }
                }
                comm.Dispose();
                conn.Close();
            }
            return errors;
        }

        [Serializable]
        private class LinearFeat
        {
            public string ID;
            public string routeId;
            public string stnSeriesIdStart;
            public string stnSeriesIdEnd;
            public double stationingStart;
            public double stationingEnd;
            public double milePostStart;
            public double milePostEnd;
            public string featureType;
            public double latitudeStart;
            public double longitudeStart;
            public double latitudeEnd;
            public double longitudeEnd;
            public string description;
            public string geomString;


            public LinearFeat(string routeId, string stnSeriesIdStart, string stnSeriesIdEnd, 
                double stationingStart, double stationingEnd, 
                double milePostStart, double milePostEnd, 
                string featureType,
                double latStart, double lonStart, 
                double latEnd, double lonEnd,
                string description, 
                string geomWKT,
                double ZStart = 0, double ZEnd = 0)
            {
                this.ID = Guid.NewGuid().ToString();
                this.routeId = routeId;
                this.stnSeriesIdStart = stnSeriesIdStart;
                this.stnSeriesIdEnd = stnSeriesIdEnd;
                this.stationingStart = stationingStart;
                this.stationingEnd = stationingEnd;
                this.milePostStart = milePostStart;
                this.milePostEnd = milePostEnd;
                this.featureType = featureType;
                this.latitudeStart = latStart;
                this.longitudeStart = lonStart;
                this.latitudeEnd = latEnd;
                this.longitudeEnd = lonEnd;
                this.description = description;
                this.geomString = geomWKT;
            }

            public LinearFeat(SqlDataReader reader)
            {
                this.ID = reader["ID"].ToString();
                this.routeId = reader["RouteID"].ToString();
                this.stnSeriesIdStart = reader["StationSeriesIDStart"].ToString();
                this.stnSeriesIdEnd = reader["StationSeriesIDEnd"].ToString();
                this.stationingStart = PLM_Helpers.resultToDouble(reader["StationingStart"]);
                this.stationingEnd = PLM_Helpers.resultToDouble(reader["StationingEnd"]);
                this.milePostStart = PLM_Helpers.resultToDouble(reader["MilePostStart"]);
                this.milePostEnd = PLM_Helpers.resultToDouble(reader["MilePostEnd"]);
                this.featureType = reader["FeatureType"].ToString();
                this.latitudeStart = PLM_Helpers.resultToDouble(reader["LatitudeStart"]);
                this.longitudeStart = PLM_Helpers.resultToDouble(reader["LongitudeStart"]);
                this.latitudeEnd = PLM_Helpers.resultToDouble(reader["LatitudeEnd"]);
                this.longitudeEnd = PLM_Helpers.resultToDouble(reader["LongitudeEnd"]);
                this.description = reader["Description"].ToString();
                this.geomString = reader["geomWKT"].ToString();
            }
        }
    }
}
