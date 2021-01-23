using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MySportsBook.Web.Helper
{
    public class ConnectionDataControl
    {
        public string con = ConfigurationManager.ConnectionStrings["MySportsBook"].ConnectionString;
        public SqlCommand comm = new SqlCommand();
        public Dictionary<string, object> DynamicParameters { get; set; }
        public Boolean IsSp = true;

        public ConnectionDataControl()
        {
            DynamicParameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Get Data from database
        /// </summary>
        /// <param name="argstrQuery"></param>
        /// <param name="IsSp"></param>
        /// <param name="IsParameter"></param>
        /// <param name="isCombo"></param>
        /// <param name="argstrIntialValue"></param>
        /// <returns></returns>
        public void GetDetailswithoutputParam(out int OutParam,out DataTable Dt, string argstrQuery, Boolean IsParameter = false, Boolean isCombo = false, string argstrIntialValue = "--Select--",string OutputParameter="")
        {
            DataTable dtData = null;
            OutParam = 0;
            using (SqlConnection connection = new SqlConnection(con))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = argstrQuery;
                    cmd.Connection = connection;
                    if (IsSp)
                    {
                        if (IsParameter)
                        {
                            if (DynamicParameters.Count > 0)
                            {
                                foreach (KeyValuePair<string, object> item in DynamicParameters)
                                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }
                        if(!string.IsNullOrEmpty(OutputParameter))
                        {
                            cmd.Parameters.Add("@totalrow", SqlDbType.Int,4).Direction= ParameterDirection.Output;
                            //cmd.Parameters[OutputParameter].Direction = ParameterDirection.Output;
                        }
                        cmd.CommandType = CommandType.StoredProcedure;
                    }
                    else
                    {
                        if (IsParameter)
                        {
                            if (DynamicParameters.Count > 0)
                            {
                                foreach (KeyValuePair<string, object> item in DynamicParameters)
                                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                            }
                        }
                        cmd.CommandType = CommandType.Text;
                    }

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (!string.IsNullOrEmpty(OutputParameter))
                    {
                        OutParam=ConvertHelper.ConvertToInteger(cmd.Parameters["@totalrow"].Value,0);
                    }
                        if (dr.HasRows)
                    {
                        dtData = new DataTable();
                        dtData.Load(dr);
                        if (isCombo)
                        {
                            DataRow drs = dtData.NewRow();
                            drs[1] = "0";
                            drs[0] = argstrIntialValue;
                            dtData.Rows.InsertAt(drs, 0);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    if (connection != null)
                        ((IDisposable)connection).Dispose();
                }
            }
            Dt= dtData;
        }

        public DataTable GetDetails(string argstrQuery, Boolean IsParameter = false, Boolean isCombo = false, string argstrIntialValue = "--Select--")
        {
            int outparam;
            DataTable Dt;
            GetDetailswithoutputParam(out outparam, out Dt, argstrQuery, IsParameter, isCombo, argstrIntialValue, "");
            return Dt;
        }
        public bool InsertOrUpdateData(string argstrQuery, Boolean IsSp = true, Boolean IsParameter = true)
        {
            Boolean IsSaved = false;
            using (SqlConnection connection = new SqlConnection(con))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = connection;
                    cmd.CommandText = argstrQuery;
                    if (IsSp)
                        cmd.CommandType = CommandType.StoredProcedure;
                    else
                        cmd.CommandType = CommandType.Text;
                    if (IsParameter)
                    {
                        if (DynamicParameters.Count > 0)
                        {
                            foreach (KeyValuePair<string, object> item in DynamicParameters)
                                cmd.Parameters.AddWithValue(item.Key, item.Value);
                            int i = cmd.ExecuteNonQuery();
                            if (i > 0)
                                IsSaved = true;
                        }
                    }
                    else
                    {
                        int i = cmd.ExecuteNonQuery();
                        if (i > 0)
                            IsSaved = true;
                    }
                }
                catch (SqlException ex)
                {
                    IsSaved = false;
                    if (connection != null) ((IDisposable)connection).Dispose();
                }
            }
            return IsSaved;
        }
    }

}