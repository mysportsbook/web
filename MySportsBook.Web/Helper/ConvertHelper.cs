using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Globalization;

namespace MySportsBook.Web.Helper
{
    public class ConvertHelper
    {

        static readonly string lineBreak = "\r\n";
        static readonly string htmlBreak = "<br>";
        static readonly string space = " ";
        static readonly string htmlSpace = "&nbsp;";
        static readonly string tab = "\t";
        static readonly string htmlTab = "&nbsp;&nbsp;&nbsp;&nbsp;";
        private ConvertHelper() { }

        public static string ConvertToString(object objectValue)
        {
            return ConvertToString(objectValue, null);
        }
        public static string ConvertToString(object objectValue, string defaultValue)
        {
            return ConvertToHTMLDecodedString(objectValue, false, defaultValue);
        }

        public static string ConvertToHTMLDecodedString(object objectValue, bool htmlDecode, string defaultValue)
        {
            return ConvertToHTMLDecodedString(objectValue, htmlDecode, false, defaultValue);
        }
        public static string ConvertToHTMLDecodedString(object objectValue, bool htmlDecode)
        {
            return ConvertToHTMLDecodedString(objectValue, htmlDecode, false);
        }
        public static string ConvertToHTMLDecodedString(object objectValue, bool htmlDecode, bool preserveFormates)
        {
            return ConvertToHTMLDecodedString(objectValue, htmlDecode, preserveFormates);
        }
        public static string ConvertToHTMLDecodedString(object objectValue, bool htmlDecode, bool preserveFormates, string defaultValue)
        {
            //If the object value is null, return null or default value
            if (objectValue == null)
                return defaultValue;

            //Convert to string and check the strig value's length
            string strVal = Convert.ToString(objectValue);
            if (strVal.Trim().Length == 0)
                return defaultValue;

            // If htmlDecode is true, Preforms HTMLDecode else returns strVal
            if (htmlDecode)
            {
                strVal = Decode(strVal.Trim());
                //To replace the string "<script>" to "&lt;script&gt;" Regular expression is used to check for all character cases, spaces and / with the text "script".
                strVal = Regex.Replace(strVal, @"<\s*/*?\s*[Ss][Cs][Rr][Ii][Pp][Tt]\s*/*?\s*>", delegate (Match m) { string s = null; foreach (Capture c in m.Captures) { s = Regex.Replace(c.Value, @"<", "&lt;"); s = Regex.Replace(s, @">", "&gt;"); } return s; });
            }
            //Replace the line breaks with the HTML break tag to maintain the line breaks in the HTML pages
            if (preserveFormates)
            {
                strVal = strVal.Replace(lineBreak, htmlBreak);
                strVal = strVal.Replace(space, htmlSpace);
                strVal = strVal.Replace(tab, htmlTab);
            }
            return strVal.Trim();
        }
        public static string ConvertToHTMLEncodeString(object objectValue, bool htmlDecode)
        {
            return ConvertToHTMLEncodeString(objectValue, htmlDecode, null);
        }
        public static string ConvertToHTMLEncodeString(object objectValue, bool htmlDecode, string defaultValue)
        {
            //If the object value is null, return null or default value
            if (objectValue == null)
                return defaultValue;

            //Convert to string and check the strig value's length
            string strVal = Convert.ToString(objectValue);
            if (strVal.Trim().Length == 0)
                return defaultValue;

            // If htmlDecode is true, Preforms HTMLDecode else returns strVal
            if (htmlDecode)
                return Encode(strVal.Trim());
            else
                return strVal.Trim();
        }
        public static int ConvertToInteger(object objectValue)
        {
            return ConvertToInteger(objectValue, 0);
        }
        public static int ConvertToInteger(object objectValue, int defaultValue)
        {
            //Declare the output value
            int integerValue;

            //If the object value is Null, return default value
            if (objectValue == null)
                return defaultValue;

            //Check the whether the object value can be converted to an integer, and if it is, return the out integer value
            if (int.TryParse(objectValue.ToString(), out integerValue))
                return integerValue;
            //If the object value cannot be converted to an integer value return defaultValue
            return defaultValue;
        }
        public static double ConvertToDouble(object objectValue)
        {
            return ConvertToDouble(objectValue, 0);
        }
        public static double ConvertToDouble(object objectValue, double defaultValue)
        {
            //Declare the output value
            double doubleValue;

            //If te output value is Null, return default value
            if (objectValue == null)
                return defaultValue;

            //Check the whether the object value can be converted to an double, and if it is, returh the out double value
            if (double.TryParse(objectValue.ToString(), out doubleValue))
                return doubleValue;
            //If the object value cannot be converted to an double value return defaultValue
            return defaultValue;
        }
        public static bool ConvertToBoolean(object objectValue)
        {
            return ConvertToBoolean(objectValue, false);
        }
        public static bool ConvertToBoolean(object objectValue, bool defaultValue)
        {
            //If te output value is Null, return default value
            if (objectValue == null)
                return defaultValue;

            //Declare the output value
            bool boolValue;
            if (objectValue.ToString() == "1") objectValue = true;
            if (objectValue.ToString() == "0") objectValue = false;

            //Check the whether the object value can be converted to an bool, and if it is, returh the out bool value
            if (bool.TryParse(objectValue.ToString(), out boolValue))
                return boolValue;

            //If the object value cannot be converted to an bool value return defaultValue
            return defaultValue;
        }
        public static DateTime ConvertDateFormatParseExact(string date)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime ResultDate = DateTime.ParseExact(date, "dd-MM-yyyy", provider);
            return ResultDate;
        }
        public static DateTime ConvertToDateTime(object objectValue)
        {
            return ConvertToDateTime(objectValue, DateTime.MinValue);
        }
        public static DateTime ConvertToDateTime(object objectValue, DateTime defaultValue)
        {
            //Declare the output value
            DateTime DateTimeValue = new DateTime();

            //If te output value is Null, return default value
            if (objectValue == null)
                return defaultValue;

            //Check the whether the object value can be converted to an DateTime, and if it is, returh the out DateTime value
            if (DateTime.TryParse(objectValue.ToString(), out DateTimeValue))
                return DateTimeValue;
            //If the object value cannot be converted to an DateTime value return defaultValue
            return defaultValue;
        }
        public static decimal ConvertToDecimal(object objectValue)
        {
            return ConvertToDecimal(objectValue, 0);
        }
        public static decimal ConvertToDecimal(object objectValue, decimal defaultValue)
        {
            //Declare the output value
            decimal decimalValue;

            //If te output value is Null, return default value
            if (objectValue == null)
                return defaultValue;

            //Check the whether the object value can be converted to an decimal, and if it is, returh the out decimal value
            if (decimal.TryParse(objectValue.ToString(), out decimalValue))
                return decimalValue;
            //If the object value cannot be converted to an decimal value return defaultValue
            return defaultValue;
        }
        private static string Decode(string strValue)
        {
            return HttpUtility.HtmlDecode(strValue);
        }
        private static string Encode(string strValue)
        {
            return HttpUtility.HtmlEncode(strValue);
        }
        // Convert List to DataTable
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        //Convert DataTable to Json 
        public static string DataTableToJSON(DataTable dt)
        {
            if (dt != null)
            {
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                Dictionary<string, object> row;
                foreach (DataRow dr in dt.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                        row.Add(col.ColumnName, dr[col]);
                    rows.Add(row);
                }
                return serializer.Serialize(rows);
            }
            else
            {
                return "";
            }
        }
        public static string ToConvertINRFormat(decimal amount)//ToString("N2")
        {
            decimal parsed = decimal.Parse(amount.ToString(), CultureInfo.InvariantCulture);
            CultureInfo hindi = new CultureInfo("hi-IN");
            return string.Format(hindi, "{0:c}", parsed);
        }
        public static DateTime ConvertDateTime(string date)
        {
            return DateTime.Parse(date, new CultureInfo("en-US"));
        }

        public static string GetCurrencySymbol(string ISOCurrencySymbol)
        {
            string symbol = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(ri => ri != null && ri.ISOCurrencySymbol == ISOCurrencySymbol)
                .Select(ri => ri.CurrencySymbol)
                .FirstOrDefault();
            return symbol;
        }
    }
}