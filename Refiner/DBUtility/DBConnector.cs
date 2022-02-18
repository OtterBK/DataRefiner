using Newtonsoft.Json.Linq;
using NLog;
using Npgsql;
using System;
using System.Collections.Generic;

namespace DataRefinerModule.DBUtility {

    public class DBConnector : IDisposable{


        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private const string defaultConnectionString = "HOST=192.168.0.30;"
                                                    + "PORT=4432;"
                                                    + "USERNAME=postgres;"
                                                    + "PASSWORD=dusrnth#12;"
                                                    + "DATABASE=data_refine;";

        private static DBConnector instance = new DBConnector(defaultConnectionString);

        public static DBConnector GetDefaultConnector() {
            return instance;
        }

        public static string GetSelectString(List<string> selectList) {

            string selectString = string.Empty;

            for (int i = 0; i < selectList.Count; i++) {
                string key = selectList[i];
                selectString += key;
                if (i != selectList.Count - 1)
                    selectString += ",";
            }

            return selectString;

        }

        public static string GetValueString(List<string> valueList) {

            string valueString = string.Empty;

            for (int i = 0; i < valueList.Count; i++) {
                string value = valueList[i];
                if(value != null) value = value.Replace("\'", "");
                value = $"'{value}'";
                //value = $"{value}";
                valueString += value;
                if (i != valueList.Count - 1)
                    valueString += ",";
            }

            return valueString;

        }

        public static string GetUpdateString(List<string> selectList, List<string> valueList) {

            string updateString = string.Empty;

            for (int i = 0; i < selectList.Count; i++) {
                string key = selectList[i];
                string value = i < valueList.Count ? valueList[i] : "";
                if(value != null) value = value.Replace("\'", "");

                string setString = $"{key}='{value}'";

                updateString += setString;
                if (i != selectList.Count - 1)
                    updateString += ",";
            }

            return updateString;

        }

        public static string GetConditionString(List<string> selectList, List<string> valueList, string condition, string compSymbol = "=") {

            string conditionString = string.Empty;

            for (int i = 0; i < selectList.Count; i++) {
                string key = selectList[i];
                string value = i < valueList.Count ? valueList[i] : "";
                if (value != null) value = value.Replace("\'", "");

                string condtion = $"{key} {compSymbol} '{value}'";

                conditionString += condtion;
                if (i != selectList.Count - 1)
                    conditionString += $" {condition} ";

            }

            return conditionString;

        }

        public static JArray GetJArrayFromResultSet(List<string[]> rows, List<string> keyList) {

            JArray resultArray = new JArray();

            foreach (string[] row in rows) {
                JObject tmpToken = GetJObjectFromResult(row, keyList);
                resultArray.Add(tmpToken);
            }

            return resultArray;
        }

        public static JObject GetJObjectFromResult(string[] row, List<string> keyList) {
            JObject resultJObject = new JObject();
            for (int i = 0; i < keyList.Count; i++) {
                string key = keyList[i];
                string value = row[i];

                resultJObject.Add(key, value);
            }

            return resultJObject;
        }

        public NpgsqlConnection conn;

        public DBConnector(string connectionString) {

            try {
                conn = new NpgsqlConnection(connectionString);
                conn.Open();
            } catch (Exception exc) {
                conn = null;
                _logger.Error(exc, "db connecting error");
            }

        }

        public DBConnector() : this(defaultConnectionString) {

        }

        public int InsertPostgreSql(string sql) {

            int affectedCount = 0;

            try {
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;

                    cmd.CommandText = sql;

                    affectedCount = cmd.ExecuteNonQuery();
                }
            } catch (Exception exc) {
                _logger.Error(exc, "failed sql: " + sql);
            }

            return affectedCount;
        }

        public int UpdatePostgreSql(string sql) {

            int affectedCount = 0;

            try {
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;

                    cmd.CommandText = sql;

                    affectedCount = cmd.ExecuteNonQuery();
                }
            } catch (Exception exc) {
                _logger.Error(exc, "failed sql: " + sql);
            }

            return affectedCount;
        }

        public int DeletePostgreSql(string sql) {

            int affectedCount = 0;

            try {
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;

                    cmd.CommandText = sql;

                    affectedCount = cmd.ExecuteNonQuery();
                }
            } catch (Exception exc) {
                _logger.Error(exc, "failed sql: " + sql);
            }

            return affectedCount;
        }

        public List<string[]> SelectPostgreSql(string sql) {
            try {
                var data = new List<string[]>();

                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;

                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            int fieldSize = reader.FieldCount;

                            string[] row = new string[fieldSize];
                            for (int i = 0; i < reader.FieldCount; i++) {
                                var tmpValue = reader.GetValue(i);
                                if (tmpValue != null)
                                    row[i] = tmpValue.ToString();
                                else row[i] = null;
                            }
                            data.Add(row);
                        }
                    }
                }

                return data;
            } catch (Exception exc) {
                _logger.Warn(exc, "failed select sql: " + sql);
                return null;
            }
        }

        public string SelectSingleObjectPostgreSql(string sql) {
            string singleObject = string.Empty;
            try {
                using (var cmd = new NpgsqlCommand()) {
                    cmd.Connection = conn;

                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader()) {

                        if (reader.Read()) {
                            var tmpValue = reader.GetValue(0);
                            if(tmpValue != null) 
                                singleObject = tmpValue.ToString();
                            else singleObject = null;
                        }
                    }
                }
            } catch (Exception) {

            }

            return singleObject;
        }

        public void Dispose() {
            if (conn != null) conn.Close();
        }
    }
}
