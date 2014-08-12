using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enbridge.PLM
{
    [Serializable]
    public class PermanentRepair
    {
        public DateTime dateOfDiscovery = DateTime.MinValue;
        public DateTime dateOfRepair = DateTime.MinValue;
        public bool? repairDueToLeak = null;
        public string causeOfBreakLeak = null;
        public double? oilOut = null;
        public double? oilRecovered = null;
        public string crudeType = null;
        public string batchNumber = null;
        public string dispositionOfOil = null;
        public string nameAndAddressOfReporter = null;
        public bool? reportFeePaid = null;

        public bool? stockPipeInstalled = null;
        public string hydrotestNumber = null;
        public string remarks = null;
        public bool? govAgencyContacted = null;
        public DateTime contactTime = DateTime.MinValue;
        public double? lat = null;
        public double? lon = null;
        public bool? gpsReading = null;
        public string locationDescription = null;
        public string fittingsAddedToMainline = null;
        public bool hasValuesSet { get; private set; }

        public PermanentRepair()
        {
            this.hasValuesSet = false;

        }

        public PermanentRepair(string reportId)
        {
            this.hasValuesSet = false;
            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT * FROM sde.PERMANENT_REPAIR_EVW WHERE ReportID = @ReportID";

                comm.Parameters.AddWithValue("@ReportID", reportId);

                try
                {
                    SqlDataReader reader = comm.ExecuteReader();
                    if (reader.Read())
                    {

                        this.hasValuesSet = true;
                        this.dateOfDiscovery = PLM_Helpers.processResultToDate(reader["DateOfDiscovery"]);
                        this.dateOfRepair = PLM_Helpers.processResultToDate(reader["DateOfRepair"]);
                        this.repairDueToLeak = PLM_Helpers.processOneZeroToNullBool(reader["RepairDueToLeak"]);
                        this.causeOfBreakLeak = PLM_Helpers.processResultToString(reader["CauseOfBreak"]);
                        this.oilOut = PLM_Helpers.processResultToNullableDouble(reader["OilOut"]);
                        this.oilRecovered = PLM_Helpers.processResultToNullableDouble(reader["OilRecovered"]);
                        this.crudeType = PLM_Helpers.processResultToString(reader["CrudeType"]);
                        this.batchNumber = PLM_Helpers.processResultToString(reader["BatchNumber"]);
                        this.dispositionOfOil = PLM_Helpers.processResultToString(reader["DispositionOfOil"]);
                        this.nameAndAddressOfReporter = PLM_Helpers.processResultToString(reader["NameAndAddressOfReporter"]);
                        this.reportFeePaid = PLM_Helpers.processOneZeroToNullBool(reader["ReportFeePaid"]);

                        this.stockPipeInstalled = PLM_Helpers.processOneZeroToNullBool(reader["StockPipeInstalled"]);
                        this.hydrotestNumber = PLM_Helpers.processResultToString(reader["HydrotestNumber"]);
                        this.remarks = PLM_Helpers.processResultToString(reader["Remarks"]);
                        this.govAgencyContacted = PLM_Helpers.processOneZeroToNullBool(reader["GovAgencyContacted"]);
                        this.contactTime = PLM_Helpers.processResultToDate(reader["GovContactTime"]);
                        this.lat = PLM_Helpers.processResultToNullableDouble(reader["Latitude"]);
                        this.lon = PLM_Helpers.processResultToNullableDouble(reader["Longitude"]);
                        this.gpsReading = PLM_Helpers.processOneZeroToNullBool(reader["GPSReading"]);
                        this.locationDescription = PLM_Helpers.processResultToString(reader["LocationDescription"]);
                        this.fittingsAddedToMainline = PLM_Helpers.processResultToString(reader["FittingsAdded"]);
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

        public void setTab1Values(DateTime dateOfDiscovery, DateTime dateOfRepair, Object repairDueToLeak, string causeOfBreakLeak, string oilOut, 
            string oilRecovered, string crudeType, string batchNumber, string dispositionOfOil, string nameAndAddressOfReporter, Object reportFeePaid)
        {
            this.hasValuesSet = true;
            this.dateOfDiscovery = dateOfDiscovery;
            this.dateOfRepair = dateOfRepair;
            this.repairDueToLeak = PLM_Helpers.trueFalseValue(repairDueToLeak);
            this.causeOfBreakLeak = causeOfBreakLeak;
            this.oilOut = PLM_Helpers.convertStringToNullableDouble(oilOut);
            this.oilRecovered = PLM_Helpers.convertStringToNullableDouble(oilRecovered);
            this.crudeType = crudeType;
            this.batchNumber = batchNumber;
            this.dispositionOfOil = dispositionOfOil;
            this.nameAndAddressOfReporter = nameAndAddressOfReporter;
            this.reportFeePaid = PLM_Helpers.trueFalseValue(reportFeePaid);
        }


        public void setTab2Values(Object stockPipeInstalled, string hydrotestNumber, string remarks, Object govAgencyContacted, DateTime contactTime,
            string lat, string lon, Object gpsReading, string locationDescription, string fittingsAddedToMainline)
        {
            this.hasValuesSet = true;
            this.stockPipeInstalled = PLM_Helpers.trueFalseValue(stockPipeInstalled);
            this.hydrotestNumber = hydrotestNumber;
            this.remarks = remarks;
            this.govAgencyContacted = PLM_Helpers.trueFalseValue(govAgencyContacted);
            this.contactTime = contactTime;
            this.lat = PLM_Helpers.convertStringToNullableDouble(lat);
            this.lon = PLM_Helpers.convertStringToNullableDouble(lon);
            this.gpsReading = PLM_Helpers.trueFalseValue(gpsReading);
            this.locationDescription = locationDescription;
            this.fittingsAddedToMainline = fittingsAddedToMainline;
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
                comm.CommandText += "SELECT COUNT(*) FROM sde.PERMANENT_REPAIR_EVW WHERE ReportID = @reportID;";

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
                    comm.CommandText += "UPDATE sde.PERMANENT_REPAIR_EVW SET ";
                    comm.CommandText += "DateOfDiscovery=@DateOfDiscovery, DateOfRepair=@DateOfRepair, RepairDueToLeak=@RepairDueToLeak, ";
                    comm.CommandText += "CauseOfBreak=@CauseOfBreak, OilOut=@OilOut, OilRecovered=@OilRecovered, ";
                    comm.CommandText += "CrudeType=@CrudeType, BatchNumber=@BatchNumber, DispositionOfOil=@DispositionOfOil, ";
                    comm.CommandText += "NameAndAddressOfReporter=@NameAndAddressOfReporter, ReportFeePaid=@ReportFeePaid, StockPipeInstalled=@StockPipeInstalled, ";
                    comm.CommandText += "HydrotestNumber=@HydrotestNumber, Remarks=@Remarks, GovAgencyContacted=@GovAgencyContacted, ";
                    comm.CommandText += "GovContactTime=@GovContactTime, Latitude=@Latitude, Longitude=@Longitude, ";
                    comm.CommandText += "GPSReading=@GPSReading, LocationDescription=@LocationDescription, FittingsAdded=@FittingsAdded ";
                    comm.CommandText += "WHERE ReportID = @reportID; ";
                }
                else
                {
                    comm.CommandText += "INSERT into sde.PERMANENT_REPAIR_EVW (";
                    comm.CommandText += "ID, ReportID, DateOfDiscovery, DateOfRepair, RepairDueToLeak, CauseOfBreak, OilOut, ";
                    comm.CommandText += "OilRecovered, CrudeType, BatchNumber, DispositionOfOil, NameAndAddressOfReporter, ";
                    comm.CommandText += "ReportFeePaid, StockPipeInstalled, HydrotestNumber, Remarks, GovAgencyContacted, GovContactTime, ";
                    comm.CommandText += "Latitude, Longitude, GPSReading, LocationDescription, FittingsAdded";
                    comm.CommandText += ") Values (";
                    comm.CommandText += "NEWID(), @ReportID, @DateOfDiscovery, @DateOfRepair, @RepairDueToLeak, @CauseOfBreak, @OilOut, ";
                    comm.CommandText += "@OilRecovered, @CrudeType, @BatchNumber, @DispositionOfOil, @NameAndAddressOfReporter, ";
                    comm.CommandText += "@ReportFeePaid, @StockPipeInstalled, @HydrotestNumber, @Remarks, @GovAgencyContacted, @GovContactTime, ";
                    comm.CommandText += "@Latitude, @Longitude, @GPSReading, @LocationDescription, @FittingsAdded";
                    comm.CommandText += ");";
                }
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                comm.Parameters.AddWithValue("@DateOfDiscovery", PLM_Helpers.nullOrDateFromDate(this.dateOfDiscovery));
                comm.Parameters.AddWithValue("@DateOfRepair", PLM_Helpers.nullOrDateFromDate(this.dateOfRepair));
                comm.Parameters.AddWithValue("@RepairDueToLeak", PLM_Helpers.nullOneOrZeroFromNullableBool(this.repairDueToLeak));
                comm.Parameters.AddWithValue("@CauseOfBreak", PLM_Helpers.nullOrStringFromString(this.causeOfBreakLeak));
                comm.Parameters.AddWithValue("@OilOut", PLM_Helpers.nullOrNumberFromNullableDouble(this.oilOut));
                comm.Parameters.AddWithValue("@OilRecovered", PLM_Helpers.nullOrNumberFromNullableDouble(this.oilRecovered));
                comm.Parameters.AddWithValue("@CrudeType", PLM_Helpers.nullOrStringFromString(this.crudeType));
                comm.Parameters.AddWithValue("@BatchNumber", PLM_Helpers.nullOrStringFromString(this.batchNumber));
                comm.Parameters.AddWithValue("@DispositionOfOil", PLM_Helpers.nullOrStringFromString(this.dispositionOfOil));
                comm.Parameters.AddWithValue("@NameAndAddressOfReporter", PLM_Helpers.nullOrStringFromString(this.dispositionOfOil));
                comm.Parameters.AddWithValue("@ReportFeePaid", PLM_Helpers.nullOneOrZeroFromNullableBool(this.reportFeePaid));
                comm.Parameters.AddWithValue("@StockPipeInstalled", PLM_Helpers.nullOneOrZeroFromNullableBool(this.stockPipeInstalled));
                comm.Parameters.AddWithValue("@HydrotestNumber", PLM_Helpers.nullOrStringFromString(this.hydrotestNumber));
                comm.Parameters.AddWithValue("@Remarks", PLM_Helpers.nullOrStringFromString(this.remarks));
                comm.Parameters.AddWithValue("@GovAgencyContacted", PLM_Helpers.nullOneOrZeroFromNullableBool(this.govAgencyContacted));
                comm.Parameters.AddWithValue("@GovContactTime", PLM_Helpers.nullOrDateFromDate(this.contactTime));
                comm.Parameters.AddWithValue("@Latitude", PLM_Helpers.nullOrNumberFromNullableDouble(this.lat));
                comm.Parameters.AddWithValue("@Longitude", PLM_Helpers.nullOrNumberFromNullableDouble(this.lon));
                comm.Parameters.AddWithValue("@GPSReading", PLM_Helpers.nullOneOrZeroFromNullableBool(this.gpsReading));
                comm.Parameters.AddWithValue("@LocationDescription", PLM_Helpers.nullOrStringFromString(this.locationDescription));
                comm.Parameters.AddWithValue("@FittingsAdded", PLM_Helpers.nullOrStringFromString(this.fittingsAddedToMainline));

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
