using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enbridge.PLM
{
    /// <summary>
    /// RowInfo properties
    /// </summary>
    [Serializable]
    public class RowInfo
    {
        #region class properties
        /// <summary>
        /// 
        /// </summary>
        public string locationTract = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationDistrict = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationLineSection = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationState = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationCountry = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationLegalDescription = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationSection = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationTownship = null;
        /// <summary>
        /// 
        /// </summary>
        public string locationRange = null;

        /// <summary>
        /// 
        /// </summary>
        public string tenantName = null;
        /// <summary>
        /// 
        /// </summary>
        public string tenantMainPhone = null;
        /// <summary>
        /// 
        /// </summary>
        public string tenantSecondaryPhone = null;
        /// <summary>
        /// 
        /// </summary>
        public string tenantCountry = null;
        /// <summary>
        /// 
        /// </summary>
        public string tenantRowComments = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? tenantContactMade = null;

        /// <summary>
        /// 
        /// </summary>
        public string accessHowGained = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? accessRoutePreapproved = null;
        /// <summary>
        /// 
        /// </summary>
        public double? accessLength = null;
        /// <summary>
        /// 
        /// </summary>
        public double? accessWidth = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? accessDamagedWhileAcessing = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? accessRestorationCompleted = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? accessRestorationApprovedByOwner = null;

        /// <summary>
        /// 
        /// </summary>
        public double? workAreaLength = null;
        /// <summary>
        /// 
        /// </summary>
        public double? workAreaWidth = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? workAreaRestorationCompleted = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? workAreaRestorationApproved = null;
        /// <summary>
        /// 
        /// </summary>
        public bool? workAreaPesticidesInLast72Hours = null;

        /// <summary>
        /// 
        /// </summary>
        public bool hasValuesSet { get; private set; }

        #endregion class properties

        /// <summary>
        /// Constructor no parameters
        /// </summary>
        public RowInfo()
        {
            this.hasValuesSet = false;
        }

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="reportId">Report ID</param>
        public RowInfo(string reportId)
        {
            this.hasValuesSet = false;
            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT * FROM sde.ROWINFO_EVW WHERE ReportID = @ReportID";

                comm.Parameters.AddWithValue("@ReportID", reportId);

                try
                {
                    SqlDataReader reader = comm.ExecuteReader();
                    if (reader.Read())
                    {
                        this.hasValuesSet = true;
                        this.locationTract = PLM_Helpers.processResultToString(reader["Tract"]);
                        this.locationDistrict = PLM_Helpers.processResultToString(reader["District"]);
                        this.locationLineSection = PLM_Helpers.processResultToString(reader["LineSection"]);
                        this.locationState = PLM_Helpers.processResultToString(reader["State"]);
                        this.locationCountry = PLM_Helpers.processResultToString(reader["Country"]);
                        this.locationLegalDescription = PLM_Helpers.processResultToString(reader["LegalDescription"]);
                        this.locationSection = PLM_Helpers.processResultToString(reader["Section"]);
                        this.locationTownship = PLM_Helpers.processResultToString(reader["Township"]);
                        this.locationRange = PLM_Helpers.processResultToString(reader["Range"]);

                        this.tenantName = PLM_Helpers.processResultToString(reader["TenantName"]);
                        this.tenantMainPhone = PLM_Helpers.processResultToString(reader["TenantPhone"]);
                        this.tenantSecondaryPhone = PLM_Helpers.processResultToString(reader["TenantPhone2"]);
                        this.tenantCountry = PLM_Helpers.processResultToString(reader["TenantCountry"]);
                        this.tenantRowComments = PLM_Helpers.processResultToString(reader["TenantRowComments"]);
                        this.tenantContactMade = PLM_Helpers.processOneZeroToNullBool(reader["TenantContactMade"]);

                        this.accessHowGained = PLM_Helpers.processResultToString(reader["AccessGainedMethod"]);
                        this.accessRoutePreapproved = PLM_Helpers.processOneZeroToNullBool(reader["AccessRoutePreapproved"]);
                        this.accessLength = PLM_Helpers.processResultToNullableDouble(reader["AccessLength"]);
                        this.accessWidth = PLM_Helpers.processResultToNullableDouble(reader["AccessWidth"]);
                        this.accessDamagedWhileAcessing = PLM_Helpers.processOneZeroToNullBool(reader["AccessDamaged"]);
                        this.accessRestorationCompleted = PLM_Helpers.processOneZeroToNullBool(reader["AccessRestored"]);
                        this.accessRestorationApprovedByOwner = PLM_Helpers.processOneZeroToNullBool(reader["AccessRestorationApprovedOwner"]);

                        this.workAreaLength = PLM_Helpers.processResultToNullableDouble(reader["WorkAreaLength"]);
                        this.workAreaWidth = PLM_Helpers.processResultToNullableDouble(reader["WorkAreaWidth"]);
                        this.workAreaRestorationCompleted = PLM_Helpers.processOneZeroToNullBool(reader["WorkAreaRestored"]);
                        this.workAreaRestorationApproved = PLM_Helpers.processOneZeroToNullBool(reader["WorkAreaRestorationApproved"]);
                        this.workAreaPesticidesInLast72Hours = PLM_Helpers.processOneZeroToNullBool(reader["WorkAreaPesticideInLast72Hours"]);

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
        /// Set the location tab values
        /// </summary>
        /// <param name="tract"></param>
        /// <param name="district"></param>
        /// <param name="lineSection"></param>
        /// <param name="state"></param>
        /// <param name="country"></param>
        /// <param name="legalDescription"></param>
        /// <param name="section"></param>
        /// <param name="township"></param>
        /// <param name="range"></param>
        public void setLocationTabValues(string tract, string district, string lineSection, Object state, Object country, string legalDescription,
            string section, string township, string range)
        {
            this.hasValuesSet = true;
            this.locationTract = tract;
            this.locationDistrict = district;
            this.locationLineSection = lineSection;
            this.locationState = PLM_Helpers.getComboBoxSelectedValue(state);
            this.locationCountry = PLM_Helpers.getComboBoxSelectedValue(country);
            this.locationLegalDescription = legalDescription;
            this.locationSection = section;
            this.locationTownship = township;
            this.locationRange = range;
        }

        /// <summary>
        /// Set tenant tab values
        /// </summary>
        /// <param name="tenantName"></param>
        /// <param name="tenantMainPhone"></param>
        /// <param name="tenantSecondaryPhone"></param>
        /// <param name="tenantCountry"></param>
        /// <param name="tenantRowComments"></param>
        /// <param name="tenantContactMade"></param>
        public void setTenantTabValues(string tenantName, string tenantMainPhone, string tenantSecondaryPhone, Object tenantCountry,
            string tenantRowComments, Object tenantContactMade)
        {
            this.hasValuesSet = true;
            this.tenantName = tenantName;
            this.tenantMainPhone = tenantMainPhone;
            this.tenantSecondaryPhone = tenantSecondaryPhone;
            this.tenantCountry = PLM_Helpers.getComboBoxSelectedValue(tenantCountry);
            this.tenantRowComments = tenantRowComments;
            this.tenantContactMade = PLM_Helpers.trueFalseValue(tenantContactMade);
        }

        /// <summary>
        /// Set access tab values
        /// </summary>
        /// <param name="accessHowGained"></param>
        /// <param name="accessRoutePreapproved"></param>
        /// <param name="accessLength"></param>
        /// <param name="accessWidth"></param>
        /// <param name="accessDamagedWhileAcessing"></param>
        /// <param name="accessRestorationCompleted"></param>
        /// <param name="accessRestorationApprovedByOwner"></param>
        public void setAccessTabValues(Object accessHowGained, Object accessRoutePreapproved, string accessLength, string accessWidth,
            Object accessDamagedWhileAcessing, Object accessRestorationCompleted, Object accessRestorationApprovedByOwner)
        {
            this.hasValuesSet = true;
            this.accessHowGained = PLM_Helpers.getComboBoxSelectedValue(accessHowGained);
            this.accessRoutePreapproved = PLM_Helpers.trueFalseValue(accessRoutePreapproved);
            this.accessLength = PLM_Helpers.convertStringToNullableDouble(accessLength);
            this.accessWidth = PLM_Helpers.convertStringToNullableDouble(accessWidth);
            this.accessDamagedWhileAcessing = PLM_Helpers.trueFalseValue(accessDamagedWhileAcessing);
            this.accessRestorationCompleted = PLM_Helpers.trueFalseValue(accessRestorationCompleted);
            this.accessRestorationApprovedByOwner = PLM_Helpers.trueFalseValue(accessRestorationApprovedByOwner);
        }

        /// <summary>
        /// Set work area tab values
        /// </summary>
        /// <param name="workAreaLength"></param>
        /// <param name="workAreaWidth"></param>
        /// <param name="workAreaRestorationCompleted"></param>
        /// <param name="workAreaRestorationApproved"></param>
        /// <param name="workAreaPesticidesInLast72Hours"></param>
        public void setWorkAreaTabValues(string workAreaLength, string workAreaWidth, Object workAreaRestorationCompleted,
            Object workAreaRestorationApproved, Object workAreaPesticidesInLast72Hours)
        {
            this.hasValuesSet = true;
            this.workAreaLength = PLM_Helpers.convertStringToNullableDouble(workAreaLength);
            this.workAreaWidth = PLM_Helpers.convertStringToNullableDouble(workAreaWidth);
            this.workAreaRestorationCompleted = PLM_Helpers.trueFalseValue(workAreaRestorationCompleted);
            this.workAreaRestorationApproved = PLM_Helpers.trueFalseValue(workAreaRestorationApproved);
            this.workAreaPesticidesInLast72Hours = PLM_Helpers.trueFalseValue(workAreaPesticidesInLast72Hours);
        }

        /// <summary>
        /// Save to database
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
                comm.CommandText += "SELECT COUNT(*) FROM sde.ROWINFO_EVW WHERE ReportID = @reportID;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                try
                {
                    int rowCount = (int)comm.ExecuteScalar();
                    updateExisting = (rowCount > 0 ? true : false);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    errors += "ROW Info: " + ex.Message;
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
                    comm.CommandText += "UPDATE sde.ROWINFO_EVW SET ";
                    comm.CommandText += "District=@District, LineSection=@LineSection, State=@State, ";
                    comm.CommandText += "Country=@Country, LegalDescription=@LegalDescription, Section=@Section, ";
                    comm.CommandText += "Township=@Township, Range=@Range, TenantName=@TenantName, ";
                    comm.CommandText += "TenantPhone=@TenantPhone, TenantPhone2=@TenantPhone2, TenantCountry=@TenantCountry, ";
                    comm.CommandText += "TenantRowComments=@TenantRowComments, TenantContactMade=@TenantContactMade, AccessGainedMethod=@AccessGainedMethod, ";
                    comm.CommandText += "AccessRoutePreapproved=@AccessRoutePreapproved, AccessLength=@AccessLength, AccessWidth=@AccessWidth, ";
                    comm.CommandText += "AccessDamaged=@AccessDamaged, AccessRestored=@AccessRestored, ";
                    comm.CommandText += "AccessRestorationApprovedOwner=@AccessRestorationApprovedOwner, ";
                    comm.CommandText += "WorkAreaLength=@WorkAreaLength, WorkAreaWidth=@WorkAreaWidth, WorkAreaRestored=@WorkAreaRestored, ";
                    comm.CommandText += "WorkAreaRestorationApproved=@WorkAreaRestorationApproved, ";
                    comm.CommandText += "WorkAreaPesticideInLast72Hours=@WorkAreaPesticideInLast72Hours ";
                    comm.CommandText += "WHERE ReportID = @reportID; ";
                }
                else
                {
                    comm.CommandText += "INSERT into sde.ROWINFO_EVW (";
                    comm.CommandText += "ID, ReportID, ";
                    comm.CommandText += "Tract, District, LineSection, State, Country, LegalDescription, Section, Township, Range, ";
                    comm.CommandText += "TenantName, TenantPhone, TenantPhone2, TenantCountry, TenantRowComments, TenantContactMade, ";
                    comm.CommandText += "AccessGainedMethod, AccessRoutePreapproved, AccessLength, AccessWidth, AccessDamaged, ";
                    comm.CommandText += "AccessRestored, AccessRestorationApprovedOwner, ";
                    comm.CommandText += "WorkAreaLength, WorkAreaWidth, WorkAreaRestored, WorkAreaRestorationApproved, WorkAreaPesticideInLast72Hours";
                    comm.CommandText += ") Values (";
                    comm.CommandText += "NEWID(), @ReportID, ";
                    comm.CommandText += "@Tract, @District, @LineSection, @State, @Country, @LegalDescription, @Section, @Township, @Range, ";
                    comm.CommandText += "@TenantName, @TenantPhone, @TenantPhone2, @TenantCountry, @TenantRowComments, @TenantContactMade, ";
                    comm.CommandText += "@AccessGainedMethod, @AccessRoutePreapproved, @AccessLength, @AccessWidth, @AccessDamaged, ";
                    comm.CommandText += "@AccessRestored, @AccessRestorationApprovedOwner, ";
                    comm.CommandText += "@WorkAreaLength, @WorkAreaWidth, @WorkAreaRestored, @WorkAreaRestorationApproved, @WorkAreaPesticideInLast72Hours";
                    comm.CommandText += ");";
                }
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                comm.Parameters.AddWithValue("@Tract", PLM_Helpers.nullOrStringFromString(this.locationTract));
                comm.Parameters.AddWithValue("@District", PLM_Helpers.nullOrStringFromString(this.locationDistrict));
                comm.Parameters.AddWithValue("@LineSection", PLM_Helpers.nullOrStringFromString(this.locationLineSection));
                comm.Parameters.AddWithValue("@State", PLM_Helpers.nullOrStringFromString(this.locationState));
                comm.Parameters.AddWithValue("@Country", PLM_Helpers.nullOrStringFromString(this.locationCountry));
                comm.Parameters.AddWithValue("@LegalDescription", PLM_Helpers.nullOrStringFromString(this.locationLegalDescription, 255));
                comm.Parameters.AddWithValue("@Section", PLM_Helpers.nullOrStringFromString(this.locationSection));
                comm.Parameters.AddWithValue("@Township", PLM_Helpers.nullOrStringFromString(this.locationTownship));
                comm.Parameters.AddWithValue("@Range", PLM_Helpers.nullOrStringFromString(this.locationRange));
                comm.Parameters.AddWithValue("@TenantName", PLM_Helpers.nullOrStringFromString(this.tenantName));
                comm.Parameters.AddWithValue("@TenantPhone", PLM_Helpers.nullOrStringFromString(this.tenantMainPhone));
                comm.Parameters.AddWithValue("@TenantPhone2", PLM_Helpers.nullOrStringFromString(this.tenantSecondaryPhone));
                comm.Parameters.AddWithValue("@TenantCountry", PLM_Helpers.nullOrStringFromString(this.tenantCountry));
                comm.Parameters.AddWithValue("@TenantRowComments", PLM_Helpers.nullOrStringFromString(this.tenantRowComments, 255));
                comm.Parameters.AddWithValue("@TenantContactMade", PLM_Helpers.nullOneOrZeroFromNullableBool(this.tenantContactMade));
                comm.Parameters.AddWithValue("@AccessGainedMethod", PLM_Helpers.nullOrStringFromString(this.accessHowGained));
                comm.Parameters.AddWithValue("@AccessRoutePreapproved", PLM_Helpers.nullOneOrZeroFromNullableBool(this.accessRoutePreapproved));
                comm.Parameters.AddWithValue("@AccessLength", PLM_Helpers.nullOrNumberFromNullableDouble(this.accessLength));
                comm.Parameters.AddWithValue("@AccessWidth", PLM_Helpers.nullOrNumberFromNullableDouble(this.accessWidth));
                comm.Parameters.AddWithValue("@AccessDamaged", PLM_Helpers.nullOneOrZeroFromNullableBool(this.accessDamagedWhileAcessing));
                comm.Parameters.AddWithValue("@AccessRestored", PLM_Helpers.nullOneOrZeroFromNullableBool(this.accessRestorationCompleted));
                comm.Parameters.AddWithValue("@AccessRestorationApprovedOwner", PLM_Helpers.nullOneOrZeroFromNullableBool(this.accessRestorationApprovedByOwner));
                comm.Parameters.AddWithValue("@WorkAreaLength", PLM_Helpers.nullOrNumberFromNullableDouble(this.workAreaLength));
                comm.Parameters.AddWithValue("@WorkAreaWidth", PLM_Helpers.nullOrNumberFromNullableDouble(this.workAreaWidth));
                comm.Parameters.AddWithValue("@WorkAreaRestored", PLM_Helpers.nullOneOrZeroFromNullableBool(this.workAreaRestorationCompleted));
                comm.Parameters.AddWithValue("@WorkAreaRestorationApproved", PLM_Helpers.nullOneOrZeroFromNullableBool(this.workAreaRestorationApproved));
                comm.Parameters.AddWithValue("@WorkAreaPesticideInLast72Hours", PLM_Helpers.nullOneOrZeroFromNullableBool(this.workAreaPesticidesInLast72Hours));

                try
                {
                    comm.ExecuteNonQuery();

                }
                catch (SqlException ex)
                {
                    errors += "Row Info: " + ex.Message;
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
