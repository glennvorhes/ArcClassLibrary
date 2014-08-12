using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enbridge.PLM
{
    /// <summary>
    /// Object to hold foreign crossing properties
    /// </summary>
    [Serializable]
    public class ForeignCrossing
    {
        /// <summary>
        /// ID of the crossing Type
        /// </summary>
        public string crossingType = null;
        /// <summary>
        /// The method used to identify pipe location
        /// </summary>
        public string methodToIdentify = null;
        /// <summary>
        /// Method used to confirm pipe location
        /// </summary>
        public string methodToConfirm = null;
        /// <summary>
        /// If the corrosion tech was notified, null enabled
        /// </summary>
        public bool? corrosionTechNotified = null;
        /// <summary>
        /// If the pipe was damaged during installation, null enabled
        /// </summary>
        public bool? pipeDamageDuringInstall = null;
        /// <summary>
        /// Was the pipe exposed during installation, null enabled
        /// </summary>
        public bool? pipeWasExposed = null;
        /// <summary>
        /// Was a test station or bond installed, null enabled
        /// </summary>
        public bool? testStationOrBondInstalled = null;
        /// <summary>
        /// Date the changed was recorded on atlas
        /// </summary>
        public DateTime dateRecordedOnAtlas = DateTime.MinValue;
        /// <summary>
        /// Name of the inpsector
        /// </summary>
        public string inspectorName = null;
        /// <summary>
        /// Dig ticket
        /// </summary>
        public string digTicket = null;
        /// <summary>
        /// Other info, comments
        /// </summary>
        public string otherInfo = null;

        public bool hasValuesSet {get; private set;}


        public ForeignCrossing()
        {
            this.hasValuesSet = false;
        }


        public ForeignCrossing(string reportId)
        {
            this.hasValuesSet = false;
            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT * FROM sde.FOREIGNCROSSING_EVW WHERE ReportID = @ReportID";

                comm.Parameters.AddWithValue("@ReportID", reportId);

                try
                {
                    SqlDataReader reader = comm.ExecuteReader();
                    if (reader.Read())
                    {
                        this.hasValuesSet = true;
                        this.crossingType = PLM_Helpers.processResultToString(reader["CrossingType"]);
                        this.methodToIdentify = PLM_Helpers.processResultToString(reader["MethodToIdentify"]);
                        this.methodToConfirm = PLM_Helpers.processResultToString(reader["MethodToConfirm"]);
                        this.corrosionTechNotified = PLM_Helpers.processOneZeroToNullBool(reader["CorrosionTechNotified"]);
                        this.pipeDamageDuringInstall = PLM_Helpers.processOneZeroToNullBool(reader["PipeDamageDuringInstall"]);
                        this.pipeWasExposed = PLM_Helpers.processOneZeroToNullBool(reader["PipeWasExposed"]);
                        this.testStationOrBondInstalled = PLM_Helpers.processOneZeroToNullBool(reader["TestStationOrBondInstalled"]);
                        this.dateRecordedOnAtlas = PLM_Helpers.processResultToDate(reader["DateRecordedOnAtlas"]);
                        this.inspectorName = PLM_Helpers.processResultToString(reader["InspectorName"]);
                        this.digTicket = PLM_Helpers.processResultToString(reader["DigTicket"]);
                        this.otherInfo = PLM_Helpers.processResultToString(reader["OtherInfo"]);
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
        /// Foreign crossing constructor
        /// </summary>
        public void setForeignCrossingValues(Object crossType, Object identifyMethod, Object confirmMethod, Object corrosionTechNotified,
            Object pipeDamageDuringInstall, Object wasPipeExposed, Object testStationOrBondInstalled, DateTime dateOnAtlas,
            string inspectorName, string digTicket, string otherInfo)
        {
            this.hasValuesSet = true;

            this.crossingType = PLM_Helpers.getComboBoxSelectedValue(crossType);

            this.methodToIdentify = PLM_Helpers.getComboBoxSelectedValue(identifyMethod);
            this.methodToConfirm = PLM_Helpers.getComboBoxSelectedValue(confirmMethod);

            //Set corrosion tech notified property
            this.corrosionTechNotified = PLM_Helpers.trueFalseValue(corrosionTechNotified);
            

            //Set pipe damage during install property
            this.pipeDamageDuringInstall = PLM_Helpers.trueFalseValue(pipeDamageDuringInstall);
            

            //Set pipe exposed during installation property
            this.pipeWasExposed = PLM_Helpers.trueFalseValue(wasPipeExposed);
           

            //Set test station or bond installed property
            this.testStationOrBondInstalled = PLM_Helpers.trueFalseValue(testStationOrBondInstalled);
            

            this.dateRecordedOnAtlas = dateOnAtlas;
            this.inspectorName = inspectorName;
            this.digTicket = digTicket;
            this.otherInfo = otherInfo;


        }

        /// <summary>
        /// string representation of foreign crossing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string returnString = "";
            returnString += string.Format("Type ID: {0}\n", this.crossingType);
            returnString += string.Format("Identify method: {0}\n", this.methodToIdentify);
            returnString += string.Format("Confirm method: {0}\n", this.methodToConfirm);
            returnString += string.Format("Corrosion tech notified: {0}\n", 
                this.corrosionTechNotified == null ? "Unknown or NA" : this.corrosionTechNotified.ToString());
            returnString += string.Format("Pipe damage during installation: {0}\n", 
                this.pipeDamageDuringInstall == null ? "Unknown or NA" : this.pipeDamageDuringInstall.ToString());
            returnString += string.Format("Pipe exposed: {0}\n", 
                this.pipeWasExposed == null ? "Unknown or NA" : this.pipeWasExposed.ToString());
            returnString += string.Format("Test station / bond installed: {0}\n",
                this.testStationOrBondInstalled == null ? "Unknown or NA" : this.testStationOrBondInstalled.ToString());
            returnString += string.Format("Date recorded on atlas: {0}\n", this.dateRecordedOnAtlas);
            returnString += string.Format("Inspector name: {0}\n", this.inspectorName);
            returnString += string.Format("Dig ticket: {0}\n", this.digTicket);
            returnString += string.Format("Other info: {0}", this.otherInfo);
            return returnString;
        }

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
                comm.CommandText += "SELECT COUNT(*) FROM sde.FOREIGNCROSSING_EVW WHERE ReportID = @reportID;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                try
                {
                    int rowCount = (int)comm.ExecuteScalar();
                    updateExisting = (rowCount > 0 ? true : false);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    errors += ex.Message;
                    conn.Close();
                    return errors;
                }

                comm.Parameters.Clear();

                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                comm.CommandText += "BEGIN TRANSACTION;";
                if (updateExisting)
                {
                    comm.CommandText += "UPDATE sde.FOREIGNCROSSING_EVW SET ";
                    comm.CommandText += "CrossingType=@CrossingType, MethodToIdentify=@MethodToIdentify, MethodToConfirm=@MethodToConfirm, ";
                    comm.CommandText += "CorrosionTechNotified=@CorrosionTechNotified, PipeDamageDuringInstall=@PipeDamageDuringInstall, ";
                    comm.CommandText += "PipeWasExposed=@PipeWasExposed, TestStationOrBondInstalled=@TestStationOrBondInstalled, ";
                    comm.CommandText += "DateRecordedOnAtlas=@DateRecordedOnAtlas, InspectorName=@InspectorName, DigTicket=@DigTicket, ";
                    comm.CommandText += "OtherInfo=@OtherInfo ";
                    comm.CommandText += "WHERE ReportID = @reportID; ";
                }
                else
                {
                    comm.CommandText += "INSERT into sde.FOREIGNCROSSING_EVW (";
                    comm.CommandText += "ID, ReportID, ";
                    comm.CommandText += "CrossingType, MethodToIdentify, MethodToConfirm, CorrosionTechNotified, PipeDamageDuringInstall, ";
                    comm.CommandText += "PipeWasExposed, TestStationOrBondInstalled, DateRecordedOnAtlas, InspectorName, DigTicket, OtherInfo";
                    comm.CommandText += ") Values (";
                    comm.CommandText += "NEWID(), @ReportID, ";
                    comm.CommandText += "@CrossingType, @MethodToIdentify, @MethodToConfirm, @CorrosionTechNotified, @PipeDamageDuringInstall, ";
                    comm.CommandText += "@PipeWasExposed, @TestStationOrBondInstalled, @DateRecordedOnAtlas, @InspectorName, @DigTicket, @OtherInfo";
                    comm.CommandText += ");";
                }
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                comm.Parameters.AddWithValue("@CrossingType", PLM_Helpers.nullOrStringFromString(this.crossingType));
                comm.Parameters.AddWithValue("@MethodToIdentify", PLM_Helpers.nullOrStringFromString(this.methodToIdentify));
                comm.Parameters.AddWithValue("@MethodToConfirm", PLM_Helpers.nullOrStringFromString(this.methodToConfirm));
                comm.Parameters.AddWithValue("@CorrosionTechNotified", PLM_Helpers.nullOneOrZeroFromNullableBool(this.corrosionTechNotified));
                comm.Parameters.AddWithValue("@PipeDamageDuringInstall", PLM_Helpers.nullOneOrZeroFromNullableBool(this.pipeDamageDuringInstall));
                comm.Parameters.AddWithValue("@PipeWasExposed", PLM_Helpers.nullOneOrZeroFromNullableBool(this.pipeWasExposed));
                comm.Parameters.AddWithValue("@TestStationOrBondInstalled", PLM_Helpers.nullOneOrZeroFromNullableBool(this.testStationOrBondInstalled));
                comm.Parameters.AddWithValue("@DateRecordedOnAtlas", PLM_Helpers.nullOrDateFromDate(this.dateRecordedOnAtlas));
                comm.Parameters.AddWithValue("@InspectorName", PLM_Helpers.nullOrStringFromString(this.inspectorName));
                comm.Parameters.AddWithValue("@DigTicket", PLM_Helpers.nullOrStringFromString(this.digTicket));
                comm.Parameters.AddWithValue("@OtherInfo", PLM_Helpers.nullOrStringFromString(this.otherInfo, 255));
                
                try
                {
                    comm.ExecuteNonQuery();

                }
                catch (SqlException ex)
                {
                    errors += ex.Message;
                    Console.WriteLine(ex.Message);
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
