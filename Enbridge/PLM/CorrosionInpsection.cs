using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enbridge.PLM
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CorrosionInpsection
    {
        public string soilType = null;
        public string terrainType = null;
        public string trenchCondition = null;
        public string inspectorName = null;
        public DateTime inspectionDate = DateTime.MinValue;
        public string externalCondition = null;
        public string internalCondition = null;
        public string coatingCondition = null;
        public string pipeToSoilPotential = null;

        public string existingCoating = null;
        public string newCoating = null;
        public double? coatingLength = null;

        public string typeOfPig = null;
        public string indicationNumber = null;
        public string wheelCount = null;
        public string gradeDepthOrRPR = null;
        public string corrosionType = null;
        public string maxPitDepth = null;
        public string anomalySize = null;


        public bool hasValuesSet { get; private set; }


        public CorrosionInpsection()
        {
            this.hasValuesSet = false;
        }

        public CorrosionInpsection(string reportId)
        {
            this.hasValuesSet = false;
            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT * FROM sde.CORROSIONINSPECTION_EVW WHERE ReportID = @ReportID";

                comm.Parameters.AddWithValue("@ReportID", reportId);

                try
                {
                    SqlDataReader reader = comm.ExecuteReader();
                    if (reader.Read())
                    {

                        this.hasValuesSet = true;
                        this.soilType = PLM_Helpers.processResultToString(reader["SoilType"]);
                        this.terrainType = PLM_Helpers.processResultToString(reader["TerrainType"]);
                        this.trenchCondition = PLM_Helpers.processResultToString(reader["TrenchCondition"]);
                        this.inspectorName = PLM_Helpers.processResultToString(reader["InspectorName"]);
                        this.inspectionDate = PLM_Helpers.processResultToDate(reader["InspectedDate"]);
                        this.externalCondition = PLM_Helpers.processResultToString(reader["ExternalCondition"]);
                        this.internalCondition = PLM_Helpers.processResultToString(reader["InternalCondition"]);
                        this.coatingCondition = PLM_Helpers.processResultToString(reader["CoatingCondition"]);
                        this.pipeToSoilPotential = PLM_Helpers.processResultToString(reader["PipeToSoilPotential"]);

                        this.existingCoating = PLM_Helpers.processResultToString(reader["ExistingCoating"]);
                        this.newCoating = PLM_Helpers.processResultToString(reader["NewCoating"]);
                        this.coatingLength = PLM_Helpers.processResultToNullableDouble(reader["CoatingLength"]);

                        this.typeOfPig = PLM_Helpers.processResultToString(reader["TypeOfPig"]);
                        this.indicationNumber = PLM_Helpers.processResultToString(reader["IndicationNumber"]);
                        this.wheelCount = PLM_Helpers.processResultToString(reader["WheelCount"]);
                        this.gradeDepthOrRPR = PLM_Helpers.processResultToString(reader["GradeDepthOrRPR"]);
                        this.corrosionType = PLM_Helpers.processResultToString(reader["CorrosionType"]);
                        this.maxPitDepth = PLM_Helpers.processResultToString(reader["MaxPitDepth"]);
                        this.anomalySize = PLM_Helpers.processResultToString(reader["AnomalySize"]);
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

        public void setTab1Values(Object soilType, Object terrainType, string trenchCondition, string inspectorName, DateTime inspectionDate, Object externalCondition, Object internalCondition,
            Object coatingCondition, string pipeToSoilPotential)
        {
            this.hasValuesSet = true;
            this.soilType = PLM_Helpers.getComboBoxSelectedValue(soilType);
            this.terrainType = PLM_Helpers.getComboBoxSelectedValue(terrainType);

            this.trenchCondition = trenchCondition;
            this.inspectorName = inspectorName;
            this.inspectionDate = inspectionDate;

            this.externalCondition = PLM_Helpers.getComboBoxSelectedValue(externalCondition);
            this.internalCondition = PLM_Helpers.getComboBoxSelectedValue(internalCondition);
            this.coatingCondition = PLM_Helpers.getComboBoxSelectedValue(coatingCondition);

            this.pipeToSoilPotential = pipeToSoilPotential;
        }

        public void setTab2Values(Object existingCoating, Object newCoating, string coatingLength)
        {
            this.hasValuesSet = true;
            this.existingCoating = PLM_Helpers.getComboBoxSelectedValue(existingCoating);
            this.newCoating = PLM_Helpers.getComboBoxSelectedValue(newCoating);

            this.coatingLength = PLM_Helpers.convertStringToNullableDouble(coatingLength);


        }

        public void setTab3Values(Object typeOfPig, string indicationNumber, string wheelCount, string gradeDepthOrRPR, Object corrosionType,
            string maxPitDepth, string anomalySize)
        {
            this.hasValuesSet = true;
            this.typeOfPig = PLM_Helpers.getComboBoxSelectedValue(typeOfPig);
            this.indicationNumber = indicationNumber;
            this.wheelCount = wheelCount;
            this.gradeDepthOrRPR = gradeDepthOrRPR;
            this.corrosionType = PLM_Helpers.getComboBoxSelectedValue(corrosionType);
            this.maxPitDepth = maxPitDepth;
            this.anomalySize = anomalySize;
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
                comm.CommandText += "SELECT COUNT(*) FROM sde.CORROSIONINSPECTION_EVW WHERE ReportID = @reportID;";

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
                    comm.CommandText += "UPDATE sde.CORROSIONINSPECTION_EVW SET ";
                    comm.CommandText += "SoilType=@SoilType, TerrainType=@TerrainType, TrenchCondition=@TrenchCondition, ";
                    comm.CommandText += "InspectorName=@InspectorName, InspectedDate=@InspectedDate, ExternalCondition=@ExternalCondition, ";
                    comm.CommandText += "InternalCondition=@InternalCondition, CoatingCondition=@CoatingCondition, PipeToSoilPotential=@PipeToSoilPotential, ";
                    comm.CommandText += "ExistingCoating=@ExistingCoating, NewCoating=@NewCoating, CoatingLength=@CoatingLength, ";
                    comm.CommandText += "TypeOfPig=@TypeOfPig, IndicationNumber=@IndicationNumber, WheelCount=@WheelCount, ";
                    comm.CommandText += "GradeDepthOrRPR=@GradeDepthOrRPR, CorrosionType=@CorrosionType, MaxPitDepth=@MaxPitDepth, ";
                    comm.CommandText += "AnomalySize=@AnomalySize ";
                    comm.CommandText += "WHERE ReportID = @reportID; ";
                }
                else
                {
                    comm.CommandText += "INSERT into sde.CORROSIONINSPECTION_EVW (";
                    comm.CommandText += "ID, ReportID, SoilType, TerrainType, TrenchCondition, InspectorName, InspectedDate, ";
                    comm.CommandText += "ExternalCondition, InternalCondition, CoatingCondition, PipeToSoilPotential, ExistingCoating, ";
                    comm.CommandText += "NewCoating, CoatingLength, TypeOfPig, IndicationNumber, WheelCount, ";
                    comm.CommandText += "GradeDepthOrRPR, CorrosionType, MaxPitDepth, AnomalySize";
                    comm.CommandText += ") Values (";
                    comm.CommandText += "NEWID(), @ReportID, @SoilType, @TerrainType, @TrenchCondition, @InspectorName, @InspectedDate, ";
                    comm.CommandText += "@ExternalCondition, @InternalCondition, @CoatingCondition, @PipeToSoilPotential, @ExistingCoating, ";
                    comm.CommandText += "@NewCoating, @CoatingLength, @TypeOfPig, @IndicationNumber, @WheelCount, ";
                    comm.CommandText += "@GradeDepthOrRPR, @CorrosionType, @MaxPitDepth, @AnomalySize";
                    comm.CommandText += ");";
                }
                comm.CommandText += "COMMIT;";
                comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                comm.Parameters.AddWithValue("@reportID", reportID);
                comm.Parameters.AddWithValue("@SoilType", PLM_Helpers.nullOrStringFromString(this.soilType));
                comm.Parameters.AddWithValue("@TerrainType", PLM_Helpers.nullOrStringFromString(this.terrainType));
                comm.Parameters.AddWithValue("@TrenchCondition", PLM_Helpers.nullOrStringFromString(this.trenchCondition));
                comm.Parameters.AddWithValue("@InspectorName", PLM_Helpers.nullOrStringFromString(this.inspectorName));
                comm.Parameters.AddWithValue("@InspectedDate", PLM_Helpers.nullOrDateFromDate(this.inspectionDate));
                comm.Parameters.AddWithValue("@ExternalCondition", PLM_Helpers.nullOrStringFromString(this.externalCondition));
                comm.Parameters.AddWithValue("@InternalCondition", PLM_Helpers.nullOrStringFromString(this.internalCondition));
                comm.Parameters.AddWithValue("@CoatingCondition", PLM_Helpers.nullOrStringFromString(this.coatingCondition));
                comm.Parameters.AddWithValue("@PipeToSoilPotential", PLM_Helpers.nullOrStringFromString(this.pipeToSoilPotential, 255));
                comm.Parameters.AddWithValue("@ExistingCoating", PLM_Helpers.nullOrStringFromString(this.existingCoating));
                comm.Parameters.AddWithValue("@NewCoating", PLM_Helpers.nullOrStringFromString(this.newCoating));
                comm.Parameters.AddWithValue("@CoatingLength", PLM_Helpers.nullOrNumberFromNullableDouble(this.coatingLength));
                comm.Parameters.AddWithValue("@TypeOfPig", PLM_Helpers.nullOrStringFromString(this.typeOfPig));
                comm.Parameters.AddWithValue("@IndicationNumber", PLM_Helpers.nullOrStringFromString(this.indicationNumber));
                comm.Parameters.AddWithValue("@WheelCount", PLM_Helpers.nullOrStringFromString(this.wheelCount));
                comm.Parameters.AddWithValue("@GradeDepthOrRPR", PLM_Helpers.nullOrStringFromString(this.gradeDepthOrRPR));
                comm.Parameters.AddWithValue("@CorrosionType", PLM_Helpers.nullOrStringFromString(this.corrosionType));
                comm.Parameters.AddWithValue("@MaxPitDepth", PLM_Helpers.nullOrStringFromString(this.maxPitDepth));
                comm.Parameters.AddWithValue("@AnomalySize", PLM_Helpers.nullOrStringFromString(this.anomalySize));

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
