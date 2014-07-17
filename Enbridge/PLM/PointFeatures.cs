using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geocortex.Forms.Client;

namespace Enbridge.PLM
{
    [Serializable]
    public class PointFeatures
    {
        private List<PointFeat> existingFeaturesList;
        private List<PointFeat> pendingFeaturesList;
        public List<DataItem> existingFeaturesDataItems;

        public PointFeatures(string reportId = null)
        {
            this.existingFeaturesList = new List<PointFeat>();
            this.pendingFeaturesList = new List<PointFeat>();
            this.existingFeaturesDataItems = new List<DataItem>();

            if (reportId != null)
            {
                using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
                {
                    conn.Open();
                    SqlCommand comm = conn.CreateCommand();

                    comm.CommandText = "";
                    comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                    comm.CommandText += "Select *, Shape.STAsText() As geomWKT FROM sde.POINT_FEATURE_EVW pointTable ";
                    comm.CommandText += "LEFT JOIN sde.POINT_FEATURE_TYPE_EVW pType ";
                    comm.CommandText += "ON pointTable.FeatureType = pType.ID ";
                    comm.CommandText += "WHERE pointTable.ReportID = @ReportID;";

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

                            this.existingFeaturesList.Add(new PointFeat(reader));
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
        public bool addFeatureByStn(string routeId, string featureType, string description, double stn)
        {
            Enbridge.LinearReferencing.ContLineLocatorSQL loc = new Enbridge.LinearReferencing.ContLineLocatorSQL(routeId);

            double mp, meas, X, Y, Z;
            mp = loc.getMPFromStn(stn, out meas, out X, out Y, out Z);
            string stnSeriesId = loc.getLocation(X, Y);

            this.pendingFeaturesList.Add(new PointFeat(routeId, stnSeriesId, stn, mp, featureType, Y, X, description, Z));
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeId"></param>
        /// <param name="featureType"></param>
        /// <param name="description"></param>
        /// <param name="mp"></param>
        /// <returns></returns>
        public bool addFeatureByMP(string routeId, string featureType, string description, double mp)
        {
            Enbridge.LinearReferencing.ContLineLocatorSQL loc = new Enbridge.LinearReferencing.ContLineLocatorSQL(routeId);
            double stn, meas, X, Y, Z;
            stn = loc.getStnFromMP(mp, out meas, out X, out Y, out Z);
            string stnSeriesId = loc.getLocation(X, Y);

            this.pendingFeaturesList.Add(new PointFeat(routeId, stnSeriesId, stn, mp, featureType, Y, X, description, Z));
            return true;
        }

        public bool addFeatureByLonLat(string routeId, string featureType, string description, double lon, double lat)
        {
            Enbridge.LinearReferencing.ContLineLocatorSQL loc = new Enbridge.LinearReferencing.ContLineLocatorSQL(routeId);
            double stn, meas, X, Y, Z, mp;
            string stnSeriesId = loc.getLocation(lon, lat, out stn, out meas, out mp);
            loc.getStnFromMP(mp, out meas, out X, out Y, out Z);

            this.pendingFeaturesList.Add(new PointFeat(routeId, stnSeriesId, stn, mp, featureType, Y, X, description, Z));
            return true;
        }

        public bool saveToDatabase(string reportID)
        {
            Console.WriteLine("point feature count {0}", this.pendingFeaturesList.Count);
            bool successStatus = false;

            if (this.pendingFeaturesList.Count == 0)
            {
                return true;
            }

            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = "";
                comm.CommandText += "Declare @geom geometry;";
                comm.CommandText += "SET @geom = geometry::STPointFromText(@geom_text, 4326).MakeValid();";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                comm.CommandText += "BEGIN TRANSACTION;";
                comm.CommandText += "INSERT INTO sde.POINT_FEATURE_EVW ";
                comm.CommandText += "(ID, ReportID, RouteID, StationSeriesID, DateAdded, Stationing, ";
                comm.CommandText += "MilePost, FeatureType, Latitude, Longitude, Description, Shape) ";
                comm.CommandText += "VALUES ";
                comm.CommandText += "(@ID, @ReportID, @RouteID, @StationSeriesID, GETDATE(), @Stationing, ";
                comm.CommandText += "@MilePost, @FeatureType, @Latitude, @Longitude, @Description, @geom) ";
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";


                foreach (PointFeat feat in this.pendingFeaturesList)
                {
                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("@ID", feat.ID);
                    comm.Parameters.AddWithValue("@ReportID", reportID);
                    comm.Parameters.AddWithValue("@RouteID", feat.routeId);
                    comm.Parameters.AddWithValue("@StationSeriesID", feat.stnSeriesId);
                    comm.Parameters.AddWithValue("@Stationing", feat.stationing);
                    comm.Parameters.AddWithValue("@MilePost", feat.milePost);
                    comm.Parameters.AddWithValue("@FeatureType", feat.featureType);
                    comm.Parameters.AddWithValue("@Latitude", feat.latitude);
                    comm.Parameters.AddWithValue("@Longitude", feat.longitude);
                    comm.Parameters.AddWithValue("@Description", feat.description);
                    comm.Parameters.AddWithValue("@geom_text", feat.geomString);

                    

                    try
                    {
                        comm.ExecuteNonQuery();
                        successStatus = true;

                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                        successStatus = false;
                    }
                }
                comm.Dispose();
                conn.Close();
            }
            return successStatus;
        }

        [Serializable]
        private class PointFeat
        {
            public string ID;
            public string routeId;
            public string stnSeriesId;
            public double stationing;
            public double milePost;
            public string featureType;
            public double latitude;
            public double longitude;
            public string description;
            public string geomString;


            public PointFeat(string routeId, string stnSeriesId, double stationing, double milePost, string featureType, 
                double lat, double lon, string description, double Z = 0)
            {
                this.ID = Guid.NewGuid().ToString();
                this.routeId = routeId;
                this.stnSeriesId = stnSeriesId;
                this.stationing = stationing;
                this.milePost = milePost;
                this.featureType = featureType;
                this.latitude = lat;
                this.longitude = lon;
                this.description = description;
                this.geomString = String.Format("POINT ({0} {1} {2} {3})", lon, lat, Z, stationing);
            }

            public PointFeat(SqlDataReader reader)
            {
                double stn, mp, lat, lon;
                this.ID = reader["ID"].ToString();
                this.routeId = reader["RouteID"].ToString();
                this.stnSeriesId = reader["StationSeriesID"].ToString();
                if (Double.TryParse(reader["Stationing"].ToString(), out stn))
                    this.stationing = stn;
                else
                    this.stationing = 0;
                if (Double.TryParse(reader["MilePost"].ToString(), out mp))
                    this.milePost = mp;
                else
                    this.milePost = 0;
                this.featureType = reader["FeatureType"].ToString();
                if (Double.TryParse(reader["Latitude"].ToString(), out lat))
                    this.latitude = lat;
                else
                    this.latitude = 45;
                if (Double.TryParse(reader["Longitude"].ToString(), out lon))
                    this.longitude = lon;
                else
                    this.longitude = -92;
                this.description = reader["Description"].ToString();
                this.geomString = reader["geomWKT"].ToString();


                //ID, ReportID, RouteID, StationSeriesID, DateAdded, Stationing, ";
                // "MilePost, FeatureType, Latitude, Longitude, Description, Shape, geomWKT, Type

            }

        }
    }
}
