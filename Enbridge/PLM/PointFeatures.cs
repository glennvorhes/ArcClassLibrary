﻿using System;
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
        private List<string> pendingDelete;
        public List<DataItem> existingFeaturesDataItems;

        public PointFeatures(string reportId = null)
        {
            this.existingFeaturesList = new List<PointFeat>();
            this.pendingFeaturesList = new List<PointFeat>();
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
        /// delete a feature
        /// </summary>
        /// <param name="featId">the feature id</param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportID"></param>
        /// <returns></returns>
        public string saveToDatabase(string reportID)
        {
            Console.WriteLine("point feature count {0}", this.pendingFeaturesList.Count);
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
                comm.CommandText += "DELETE FROM sde.POINT_FEATURE_EVW WHERE ID = @ID;";
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
                    }
                }

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

                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                        errors += ex.Message;
                    }
                }
                comm.Dispose();
                conn.Close();
            }
            return errors;
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
                this.ID = reader["ID"].ToString();
                this.routeId = reader["RouteID"].ToString();
                this.stnSeriesId = reader["StationSeriesID"].ToString();
                this.stationing = PLM_Helpers.resultToDouble(reader["Stationing"]);
                this.milePost = PLM_Helpers.resultToDouble(reader["MilePost"]);
                this.featureType = reader["FeatureType"].ToString();
                this.latitude = PLM_Helpers.resultToDouble(reader["Latitude"], 45);
                this.longitude = PLM_Helpers.resultToDouble(reader["Longitude"], -92);
                this.description = reader["Description"].ToString();
                this.geomString = reader["geomWKT"].ToString();
            }
        }
    }
}
