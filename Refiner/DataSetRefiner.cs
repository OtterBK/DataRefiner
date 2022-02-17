using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataRefinerModule.Refiner {
    public class DataSetRefiner {

        private DataRefiner _dataRefiner = new DataRefiner();
        private List<FieldProperty> _fields = new List<FieldProperty>();
        private List<string[]> _rows = new List<string[]>();
        private bool _isFieldSampled = false;

        public List<FieldProperty> Fields {
            get { return _fields; }
            set { _fields = value; }
        }

        public List<string[]> Rows {
            get { return _rows; }
            set { _rows = value; }
        }

        public DataRefiner DataRefiner {
            get { return _dataRefiner; }
        }

        public DataSetRefiner() {

        }

        public int AddFields(string fieldName) {
            int fieldsCount = AddFields(fieldName, DataType.UNKNOWN);
            _isFieldSampled = false;

            return fieldsCount;
        }

        public int AddFields(string fieldName, DataType fieldType) {
            FieldProperty field = new FieldProperty(fieldName, fieldType);
            _isFieldSampled = false;

            _fields.Add(field);

            return _fields.Count;
        }

        public int AddRows(string[] row) {
            _rows.Add(row);

            return _rows.Count;
        }

        public int AddRows(List<string[]> rows) {
            _rows.AddRange(rows);

            return _rows.Count;
        }

        public bool IsFieldSampled() {
            return _isFieldSampled;
        }

        public List<string[]> RefineDataSet() {
            DataRefiner dataRefiner = _dataRefiner;

            //필드 샘플링 안됐으면 샘플링 먼저 진행
            if (!IsFieldSampled()) {
                FieldSampling();
            }

            List<string[]> refinedRows = new List<string[]>();
            int refiendCount = 0;

            foreach(string[] row in _rows) {
                string[] refiendRow = new string[row.Length];
                for(var i = 0; i < row.Length; i++) {
                    if (i >= _fields.Count) break;

                    var field = _fields[i];
                    var value = row[i];

                    DataType fieldType = field.FieldType;
                    var refiendData = dataRefiner.DataRefine(value, fieldType);
                    refiendRow[i] = refiendData.Refined; ;

                    if (refiendData.DataType != DataType.UNKNOWN && refiendData.DataType != DataType.TEXT) {
                        refiendCount++;
                    }
                       
                }
                refinedRows.Add(refiendRow);
            }

            return refinedRows;
        }

        /// <summary>
        /// 각 필드의 데이터타입 알아내기
        /// </summary>
        /// <returns></returns>
        public int FieldSampling(bool forceReset=false) {
            int sampledCount = 0;

            if(_rows.Count > 0) {
                string[] row = _rows[0];

                for (var i = 0; i < _fields.Count; i++) {
                    if (i >= row.Length) break;

                    var field = _fields[i];
                    var value = row[i];

                    if(!forceReset && field.FieldType == DataType.UNKNOWN) {
                        var refinedData = _dataRefiner.DataRefine(value);
                        field.FieldType = refinedData.DataType;
                    }
                }
            }
            _isFieldSampled = true;

            return sampledCount;
        }

    }
}
