using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enbridge.PLM
{
    /// <summary>
    /// Report Properties
    /// </summary>
    [Serializable]
    public class ReportProperties
    {
        /// <summary>
        /// 
        /// </summary>
        public string plmFacility = null;
        /// <summary>
        /// 
        /// </summary>
        public string reportName = null;
        /// <summary>
        /// the user name
        /// </summary>
        public string username = null;
        /// <summary>
        /// ID of report type
        /// </summary>
        public string reportType = null;
        /// <summary>
        /// Sleeve used
        /// </summary>
        public bool? sleeveUsed = null;
        /// <summary>
        /// Welding done
        /// </summary>
        public bool? weldingDone = null;
        /// <summary>
        /// Stopple equipment used
        /// </summary>
        public bool? stoppleEquipmentUsed = null;
        /// <summary>
        /// AFR needed
        /// </summary>
        /// 

        public DateTime LoadedDate;

        public bool? afrNeeded = null;

        public bool hasValuesSet { get; private set; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ReportProperties()
        {
            this.hasValuesSet = false;
            this.LoadedDate = DateTime.Now;
        }

        /// <summary>
        /// Existing report constructor
        /// </summary>
        /// <param name="reportId"></param>
        public ReportProperties(string reportId)
        {
            this.hasValuesSet = false;

            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT * FROM sde.PLM_REPORT_EVW WHERE ID = @ID";

                comm.Parameters.AddWithValue("@ID", reportId);

                try
                {
                    SqlDataReader reader = comm.ExecuteReader();
                    if (reader.Read())
                    {
                        this.hasValuesSet = true;
                        this.plmFacility = PLM_Helpers.processResultToString(reader["PLM_Facility_ID"]);
                        this.reportName = PLM_Helpers.processResultToString(reader["ReportName"]);
                        this.username = PLM_Helpers.processResultToString(reader["UserName"]);
                        this.reportType = PLM_Helpers.processResultToString(reader["ReportTypeID"]);
                        this.sleeveUsed = PLM_Helpers.processOneZeroToNullBool(reader["SleeveUsed"]);
                        this.weldingDone = PLM_Helpers.processOneZeroToNullBool(reader["WeldingDone"]);
                        this.stoppleEquipmentUsed = PLM_Helpers.processOneZeroToNullBool(reader["StoppleEquipment"]);
                        this.afrNeeded = PLM_Helpers.processOneZeroToNullBool(reader["AFRNeeded"]);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="reportType"></param>
        /// <param name="sleeveUsed"></param>
        /// <param name="weldingDone"></param>
        /// <param name="stoppleEquipmentUsed"></param>
        /// <param name="afrNeeded"></param>
        /// <param name="plmFacility"></param>
        /// <param name="reportName"></param>
        public void setReportProperties(string username, Object reportType, Object sleeveUsed, Object weldingDone, Object stoppleEquipmentUsed, Object afrNeeded,
            Object plmFacility, string reportName) 
        {
            this.hasValuesSet = true;
            this.plmFacility = PLM_Helpers.getComboBoxSelectedValue(plmFacility);
            this.reportName = reportName;

            this.username = username;

            this.reportType = PLM_Helpers.getComboBoxSelectedValue(reportType);

            this.sleeveUsed = PLM_Helpers.trueFalseValue(sleeveUsed);
            this.weldingDone = PLM_Helpers.trueFalseValue(weldingDone);
            this.stoppleEquipmentUsed = PLM_Helpers.trueFalseValue(stoppleEquipmentUsed);
            this.afrNeeded = PLM_Helpers.trueFalseValue(afrNeeded);

            Console.WriteLine(this.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string returnString = "";
            returnString += string.Format("PLM ID: {0}\n", this.plmFacility);
            returnString += string.Format("Report Name: {0}\n", this.reportName);
            returnString += string.Format("User Name: {0}\n", this.username);
            returnString += string.Format("Sleeve used: {0}\n", (this.sleeveUsed == null ? "null" : this.sleeveUsed.ToString()));
            returnString += string.Format("Welding done: {0}\n", (this.weldingDone == null ? "null" : this.weldingDone.ToString()));
            returnString += string.Format("Stopple Equipment: {0}\n", (this.stoppleEquipmentUsed == null ? "null" : this.stoppleEquipmentUsed.ToString()));
            returnString += string.Format("AFR needed: {0}", (this.afrNeeded == null ? "null" : this.afrNeeded.ToString()));
            return returnString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportID"></param>
        /// <returns></returns>
        public string saveToDatabase(string reportID)
        {
            string errors = "";
            if (!this.hasValuesSet)
            {
                return errors;
            }
            
            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();

                bool updateExisting = false;
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT COUNT(*) FROM sde.PLM_REPORT_EVW WHERE ID = @reportID;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                try
                {
                    int rowCount = (int)comm.ExecuteScalar();
                    updateExisting = (rowCount > 0 ? true : false);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    conn.Close();
                    return ex.Message;
                }

                comm.Parameters.Clear();
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                comm.CommandText += "BEGIN TRANSACTION;";
                if (updateExisting)
                {
                    comm.CommandText += "UPDATE sde.PLM_REPORT_EVW SET ";
                    comm.CommandText += "ReportName=@reportName, UserName=@userName, ReportTypeID=@reportTypeID, PLM_Facility_ID=@plm_ID, SleeveUsed=@sleeveUsed, ";
                    comm.CommandText += "WeldingDone=@weldingDone, StoppleEquipment=@stoppleEquip, AFRNeeded=@afr ";
                    comm.CommandText += "WHERE ID=@reportID; ";
                }
                else
                {
                    comm.CommandText += "INSERT INTO sde.PLM_REPORT_EVW ";
                    comm.CommandText += "(ID, ReportName, UserName, LoadedDate, ReportTypeID, PLM_Facility_ID, SleeveUsed, WeldingDone, StoppleEquipment, AFRNeeded) ";
                    comm.CommandText += "Values (@reportID, @reportName, @userName, GETDATE(), @reportTypeID, @plm_ID, @sleeveUsed, @weldingDone, @stoppleEquip, @afr);";
                }
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                comm.Parameters.AddWithValue("@reportName", PLM_Helpers.nullOrStringFromString(this.reportName));
                comm.Parameters.AddWithValue("@userName", PLM_Helpers.nullOrStringFromString(this.username));
                comm.Parameters.AddWithValue("@reportTypeID", this.reportType);
                comm.Parameters.AddWithValue("@plm_ID", this.plmFacility);
                comm.Parameters.AddWithValue("@sleeveUsed", PLM_Helpers.nullOneOrZeroFromNullableBool(this.sleeveUsed));
                comm.Parameters.AddWithValue("@weldingDone", PLM_Helpers.nullOneOrZeroFromNullableBool(this.weldingDone));
                comm.Parameters.AddWithValue("@stoppleEquip", PLM_Helpers.nullOneOrZeroFromNullableBool(this.stoppleEquipmentUsed));
                comm.Parameters.AddWithValue("@afr", PLM_Helpers.nullOneOrZeroFromNullableBool(this.afrNeeded));

                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    errors = ex.Message;
                }
                finally
                {
                    comm.Dispose();
                    conn.Close();
                }
            }
            return errors;


        }
        
    }
}
