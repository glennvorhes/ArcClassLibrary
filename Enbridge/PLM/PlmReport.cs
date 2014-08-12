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
        public string saveReport()
        {
            List<string> errorList = new List<string>();

            string errorProperties = this.reportProperties.saveToDatabase(this.reportId);
            if (errorProperties != "")
            {
                //this.deleteReport();
                errorList.Add(errorProperties);
                //return errorProperties;
            }

            string errorPointFeatures = this.pointFeatures.saveToDatabase(this.reportId);
            if (errorPointFeatures != "")
            {
                errorList.Add(errorPointFeatures);
                //this.deleteReport();
                //return errors;
            }

            string errorLinearFeatures = this.linearFeatures.saveToDatabase(this.reportId);
            if (errorLinearFeatures != "")
            {
                errorList.Add(errorLinearFeatures);
                //this.deleteReport();
                //return errors;
            }


            if (this.isForeignCrossing)
            {
                string errorForeign = "";
                if (this.foreignCrossing.hasValuesSet)
                {
                    errorForeign = this.foreignCrossing.saveToDatabase(this.reportId);
                }

                if (errorForeign != "")
                {
                    errorList.Add(errorLinearFeatures);
                }
            }
            else
            {
                string errorPermanentRepair = this.permanentRepair.saveToDatabase(this.reportId);
                string errorCorossionInspection = this.corrosionInspection.saveToDatabase(this.reportId);
                string errorRowInfo = this.rowInfo.saveToDatabase(this.reportId);
                string errorFileUpload = this.fileAttachments.savePendingFilesToDatabase(this.reportId);
                
                if (errorPermanentRepair != "")
                {
                    errorList.Add(errorPermanentRepair);
                }
                if (errorPermanentRepair != "")
                {
                    errorList.Add(errorPermanentRepair);
                }
                if (errorRowInfo != "")
                {
                    errorList.Add(errorRowInfo);
                }
                if (errorFileUpload != "")
                {
                    errorList.Add(errorFileUpload);
                }
            }

            if (errorList.Count > 0)
            {
                return String.Join("\n", errorList.ToArray());
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string deleteReport()
        {
            string status = "";

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
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    status = ex.Message;
                }
                finally
                {
                    comm.Dispose();
                    conn.Close();
                }
            }

            return status;
        }


        public string approveReport(string username)
        {
            string status = "";

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
                        throw new IndexOutOfRangeException("No rows found");
                    }
                    
                }
                catch (IndexOutOfRangeException ex)
                {
                    Console.WriteLine(ex.Message);
                    comm.Dispose();
                    conn.Close();
                    return "No matching record found";
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
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    status = ex.Message;
                }
                finally
                {
                    comm.Dispose();
                    conn.Close();
                }
            }
            return status;
        }
    }
}
