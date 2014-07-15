using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enbridge.PLM
{
    [Serializable]
    public class PlmReport
    {
        #region class properties

        /// <summary>
        /// Report ID
        /// </summary>
        public string reportId;

        /// <summary>
        /// Foreign crossing object
        /// </summary>
        public ForeignCrossing foreignCrossing;

        /// <summary>
        /// 
        /// </summary>
        public ReportProperties reportProperties;

        public RowInfo rowInfo;
        
        public PermanentRepair permanentRepair;

        public CorrosionInpsection corrosionInspection;

        public FileAttachments fileAttachments;

        public PointFeatures pointFeatures;

        public LinearFeatures linearFeatures;

        public bool isForeignCrossing {get; private set;}

        private bool existingReport;

        #endregion class properties

        #region constructors

        /// <summary>
        /// Object to keep track of PLM report properties
        /// </summary>
        public PlmReport()
        {
            isForeignCrossing = true;
            
            this.reportId = Guid.NewGuid().ToString();
            this.existingReport = false;
            this.foreignCrossing = new ForeignCrossing();
            this.reportProperties = new ReportProperties();
            this.rowInfo = new RowInfo();
            this.corrosionInspection = new CorrosionInpsection();
            this.permanentRepair = new PermanentRepair();
            this.fileAttachments = new FileAttachments();
            this.pointFeatures = new PointFeatures();
            this.linearFeatures = new LinearFeatures();

        }

        /// <summary>
        /// Constructor for exiting report based on id
        /// </summary>
        /// <param name="reportId"></param>
        public PlmReport(string reportId)
        {
            
            this.reportId = reportId;
            this.existingReport = true;
            this.reportProperties = new ReportProperties(reportId);
            this.foreignCrossing = new ForeignCrossing(reportId);
            this.pointFeatures = new PointFeatures(reportId);
            this.linearFeatures = new LinearFeatures(reportId);
            if (!this.foreignCrossing.hasValuesSet)
            {
                this.isForeignCrossing = false;
                this.rowInfo = new RowInfo(reportId);
                this.corrosionInspection = new CorrosionInpsection(reportId);
                this.permanentRepair = new PermanentRepair(reportId);
                this.fileAttachments = new FileAttachments(reportId);
            }
            else
            {
                this.isForeignCrossing = true;
                this.rowInfo = new RowInfo();
                this.corrosionInspection = new CorrosionInpsection();
                this.permanentRepair = new PermanentRepair();
                this.fileAttachments = new FileAttachments();

            }

        }

        #endregion constructors


        public void setIsForeignCrossing(bool isForeign)
        {
            this.isForeignCrossing = isForeign;
        }


        /// <summary>
        /// Submit the form to the database
        /// </summary>
        public bool saveReport()
        {
            bool saveSuccessStatus = false;

            saveSuccessStatus = this.reportProperties.saveToDatabase(this.reportId);
            if (!saveSuccessStatus)
            {
                this.deleteReport();
                return saveSuccessStatus;
            }

            saveSuccessStatus = this.pointFeatures.saveToDatabase(this.reportId);
            if (!saveSuccessStatus)
            {
                this.deleteReport();
                return saveSuccessStatus;
            }

            saveSuccessStatus = this.linearFeatures.saveToDatabase(this.reportId);
            if (!saveSuccessStatus)
            {
                this.deleteReport();
                return saveSuccessStatus;
            }


            if (this.isForeignCrossing)
            {
                if (this.foreignCrossing.hasValuesSet)
                {
                    saveSuccessStatus = this.foreignCrossing.saveToDatabase(this.reportId);
                }

                if (!saveSuccessStatus)
                {
                    this.deleteReport();
                }
            }
            else
            {
                bool successPermanentRepair, successCorossionInspection,
                    successRowInfo, successFileUpload;

                successPermanentRepair = this.permanentRepair.saveToDatabase(this.reportId);
                successCorossionInspection = this.corrosionInspection.saveToDatabase(this.reportId);
                successRowInfo = this.rowInfo.saveToDatabase(this.reportId);
                successFileUpload = this.fileAttachments.savePendingFilesToDatabase(this.reportId);

                saveSuccessStatus = (successPermanentRepair && successCorossionInspection &&
                    successRowInfo && successFileUpload);

                if (!saveSuccessStatus)
                {
                    this.deleteReport();
                }

            }
            return saveSuccessStatus;
        }

        public bool deleteReport()
        {
            bool deleteSuccessful = false;

            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                comm.CommandText += "BEGIN TRANSACTION;";
                comm.CommandText += "DELETE FROM sde.PLM_REPORT_EVW WHERE ID=@reportID;";
                comm.CommandText += "DELETE FROM sde.FOREIGNCROSSING_EVW WHERE ReportID=@reportID;";
                comm.CommandText += "DELETE FROM sde.CORROSIONINSPECTION_EVW WHERE ReportID=@reportID;";
                comm.CommandText += "DELETE FROM sde.ROWINFO_EVW WHERE ReportID=@reportID;";
                comm.CommandText += "DELETE FROM sde.PERMANENT_REPAIR_EVW WHERE ReportID=@reportID;";
                comm.CommandText += "DELETE FROM sde.ATTACHMENTS_EVW WHERE Report_ID=@reportID;";
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                comm.Parameters.AddWithValue("@reportID", this.reportId);

                try
                {
                    comm.ExecuteNonQuery();
                    deleteSuccessful = true;
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

            return deleteSuccessful;
        }


        public bool approveReport(string username)
        {
            bool approveSuccess = false;

            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT COUNT(*) FROM sde.PLM_REPORT_EVW WHERE ID=@reportID;";

                comm.Parameters.AddWithValue("@reportID", this.reportId);

                try
                {
                    int rowCount = (int)comm.ExecuteScalar();
                    if (rowCount < 1)
                    {
                        //No rows found
                        throw new Exception("No rows found");
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    comm.Dispose();
                    conn.Close();
                    return false;
                }

                comm.Parameters.Clear();

                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                comm.CommandText += "BEGIN TRANSACTION;";
                comm.CommandText += "UPDATE sde.PLM_REPORT_EVW SET ";
                comm.CommandText += "ApprovedBy=@ApprovedBy, ApprovedDate=GETDATE() ";
                comm.CommandText += "WHERE ID=@reportID;";
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                comm.Parameters.AddWithValue("@reportID", this.reportId);
                comm.Parameters.AddWithValue("@ApprovedBy", username);

                try
                {
                    comm.ExecuteNonQuery();
                    approveSuccess = true;
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

            return approveSuccess;
        }


        
    }
}
