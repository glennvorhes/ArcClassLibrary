using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geocortex.Forms.Client;
using System.Data.SqlClient;

namespace Enbridge.PLM
{
    [Serializable]
    public class FileAttachments
    {
        private Dictionary<string, FileItem> uploadedFilesFileItemDict = new Dictionary<string, FileItem>();
        private Dictionary<string, FileItem> pendingFilesFileItemDict = new Dictionary<string, FileItem>();
        public Dictionary<string, DataItem> uploadedFilesDataItemDict = new Dictionary<string, DataItem>();
        public Dictionary<string, DataItem> pendingFilesDataItemDict = new Dictionary<string, DataItem>();

        public FileAttachments()
        {

        }

        public FileAttachments(string reportId)
        {
            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();
                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT ID, FileName, Loaded_Date FROM sde.ATTACHMENTS_EVW WHERE Report_ID = @ReportID";

                comm.Parameters.AddWithValue("@ReportID", reportId);

                try
                {
                    SqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        string fileId = PLM_Helpers.processResultToString(reader["ID"]);
                        string fileName = PLM_Helpers.processResultToString(reader["FileName"]);

                        this.uploadedFilesFileItemDict.Add(fileId, null);
                        this.uploadedFilesDataItemDict.Add(fileId, new DataItem(fileName, fileId));
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


        public void addFiles(IList<FileItem> uploadList)
        {

            foreach (FileItem fileItem in uploadList)
            {
                string uniqueId = Guid.NewGuid().ToString();
                this.pendingFilesFileItemDict.Add(uniqueId, fileItem);
                this.pendingFilesDataItemDict.Add(uniqueId, new DataItem(fileItem.FileName, uniqueId));
            }
        }

        public void removeFile(string id)
        {
            this.pendingFilesDataItemDict.Remove(id);
            this.pendingFilesFileItemDict.Remove(id);
        }


        /// <summary>
        /// Return a list of Geocortex DataItems representing the items in the FileItem dictionary
        /// </summary>
        /// <param name="existingOrPending">existing or pending files</param>
        /// <returns></returns>
        public List<DataItem> getFileDataItemList(string existingOrPending)
        {
            List<DataItem> dataItemList = new List<DataItem>();

            switch (existingOrPending)
            {
                case "existing":
                    foreach (KeyValuePair<string, DataItem> entry in this.uploadedFilesDataItemDict)
                    {
                        dataItemList.Add(entry.Value);
                    }
                    break;
                case "pending":
                    foreach (KeyValuePair<string, DataItem> entry in this.pendingFilesDataItemDict)
                    {
                        dataItemList.Add(entry.Value);
                    }
                    break;
                default:
                    throw new Exception("existing or pending not set");
            }

            return dataItemList;
        }


        public string savePendingFilesToDatabase(string reportID)
        {
            string errors = "";

            if (this.pendingFilesFileItemDict.Count == 0)
            {
                return errors;
            }

            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();

                foreach (KeyValuePair<string, FileItem> entry in this.pendingFilesFileItemDict)
                {
                    comm.Parameters.Clear();

                    comm.CommandText = "";
                    comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                    comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 1;";
                    comm.CommandText += "BEGIN TRANSACTION;";
                    comm.CommandText += "INSERT into sde.ATTACHMENTS_EVW (";
                    comm.CommandText += "ID, Report_ID, ";
                    comm.CommandText += "Data, FileName, Loaded_Date";
                    comm.CommandText += ") Values (";
                    comm.CommandText += "@ID, @Report_ID, ";
                    comm.CommandText += "@Data, @FileName, GETDATE()";
                    comm.CommandText += ");";
                    comm.CommandText += "COMMIT;";
                    comm.CommandText += "EXEC sde.edit_version 'SDE.Working', 2;";

                    comm.Parameters.AddWithValue("@Report_ID", reportID);
                    comm.Parameters.AddWithValue("@ID", entry.Key);
                    comm.Parameters.AddWithValue("@Data", entry.Value.FileData);
                    comm.Parameters.AddWithValue("@FileName", entry.Value.FileName);
                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.Message);
                        errors += "Attachments: " + ex.Message;
                    }
                }

                comm.Dispose();
                conn.Close();

            }
            return errors;
        }

        public static byte[] get_file(string fileId, out string fileName)
        {
            byte[] data = null;
            fileName = null;

            using (SqlConnection conn = new SqlConnection(AppConstants.CONN_STRING_PLM_REPORTS))
            {
                conn.Open();
                SqlCommand comm = conn.CreateCommand();

                comm.CommandText = "";
                comm.CommandText += "EXEC sde.set_current_version 'SDE.Working';";
                comm.CommandText += "SELECT * FROM sde.ATTACHMENTS_EVW ";
                comm.CommandText += "WHERE ID = @ID;";

                comm.Parameters.AddWithValue("@ID", fileId);

                try
                {
                    SqlDataReader reader = comm.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        Console.WriteLine(reader);
                        fileName = reader["FileName"].ToString();
                        data = reader["Data"] as byte[];
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
            return data;
        }
    }
}
