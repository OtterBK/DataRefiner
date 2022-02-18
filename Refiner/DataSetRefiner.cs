using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataRefinerModule.Refiner {
    public class DataSetRefiner {

        //데이터 정제기는 데이터셋 정제기마다 1개씩, 각종 패턴 저장을 위해
        private DataRefiner _dataRefiner; 
        private List<FieldProperty> _fields = new List<FieldProperty>();
        private List<string[]> _rows = new List<string[]>();
        private bool _isFieldSampled = false;
        private int _threadAmount = 4;
        private int _threadDoneWorkerCount = 0;

        public int ProgressCount = 0;

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

        public int ThreadAmount {
            get { return _threadAmount; }
            set { _threadAmount = value; }
        }

        public DataSetRefiner() {
            _dataRefiner = new DataRefiner();
            //패턴 재분석 모드 off, 데이터셋 정제기에서는 정제 과정에서 패턴이 바뀌면 안됨
            _dataRefiner.Mode_PatternOverride = false; 
        }

        public class ThreadCallbackInfo {
            public List<string[]> RefinedStore;
            public bool IsDone = false;
        }

        private class RefineThreadInfo {
            public static int DoneWokertCount = 0;
            public int StartIndex = 0;
            public int Count = 0;
            public List<string[]> RefinedStore;
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

        public bool RefineDataSetWithThread(ThreadCallbackInfo threadCallbackInfo) {

            if (threadCallbackInfo != null) {

                Thread thread = new Thread(() => {
                    Console.WriteLine("start dataset refine as thread");
                    List<string[]> refinedRows = RefineDataSet();
                    threadCallbackInfo.RefinedStore = refinedRows;
                    threadCallbackInfo.IsDone = true;
                });
                thread.Start();

                return true;
            }
            return false;

        }

        public List<string[]> RefineDataSet() {
            Console.WriteLine("start dataset refine threadCount: " + _threadAmount);

            DataRefiner dataRefiner = _dataRefiner;

            //필드 샘플링 안됐으면 샘플링 먼저 진행
            if (!IsFieldSampled()) {
                FieldSampling();
            }

            List<string[]> refinedRows = new List<string[]>();

            int rowAmountPerThread = _rows.Count / _threadAmount + 1;

            List<List<string[]>> fixPositionStore = new List<List<string[]>>();

            _threadDoneWorkerCount = 0;
            for(var i = 0; i < _threadAmount; i++) {
                int start = i * rowAmountPerThread;
                int rangeCount = rowAmountPerThread;
                if (start + rangeCount > _rows.Count) rangeCount = _rows.Count - start;

                List<string[]> refinedStore = new List<string[]>();
                fixPositionStore.Add(refinedStore);

                RefineThreadInfo refineThreadInfo = new RefineThreadInfo();
                refineThreadInfo.RefinedStore = refinedStore;
                refineThreadInfo.StartIndex = start;
                refineThreadInfo.Count = rangeCount;

                ThreadPool.QueueUserWorkItem(RefineRowsThreadWorker, refineThreadInfo);
            }

            int lastThreadDoneWorkerCount = 0;
            ProgressCount = 0;
            while(_threadDoneWorkerCount < _threadAmount) {
                Thread.Sleep(100);
                Console.WriteLine($"Waiting for refiner thread... done worker: {_threadDoneWorkerCount}/{_threadAmount}");
                if(_threadDoneWorkerCount > lastThreadDoneWorkerCount) {
                    lastThreadDoneWorkerCount = _threadDoneWorkerCount;
                }
            }

            Console.WriteLine($"Concating RefinedRows...");
            foreach(List<string[]> refinedStorePart in fixPositionStore) {
                refinedRows.AddRange(refinedStorePart);
            }

            return refinedRows;
        }

        public void RefineRowsThreadWorker(object refineThreadInfoObj) {
            RefineThreadInfo refineThreadInfo = refineThreadInfoObj as RefineThreadInfo;
            if(refineThreadInfo != null) {
                List<string[]> refinedStrore = refineThreadInfo.RefinedStore;
                int startIndex = refineThreadInfo.StartIndex;
                int count = refineThreadInfo.Count;

                List<string[]> refineTarget = _rows.GetRange(startIndex, count);

                List<string[]> refinedRows = RefineRows(refineTarget);
                refinedStrore.AddRange(refinedRows);
            }
            _threadDoneWorkerCount++;
        }

        /// <summary>
        /// rows 를 정제하여 새 리스트 반환
        /// </summary>
        /// <param name="rows">정제할 rows</param>
        /// <returns></returns>
        public List<string[]> RefineRows(List<string[]> rows) {
            List<string[]> refinedRows = new List<string[]>();

            foreach(string[] row in rows) {
                string[] refinedRow = RefineRow(row);
                refinedRows.Add(refinedRow);
                Console.WriteLine($"refine row done... {refinedRows.Count}/{rows.Count}");
                ProgressCount++;
            }

            return refinedRows;
        }

        /// <summary>
        /// 단일 row 정제하여 반환
        /// </summary>
        /// <param name="row">정제할 row</param>
        /// <returns></returns>
        public string[] RefineRow(string[] row) {
            string[] refiendRow = new string[row.Length];
            for (var i = 0; i < row.Length; i++) {
                if (i >= _fields.Count) break;

                var field = _fields[i];
                var value = row[i];

                DataType fieldType = field.FieldType;
                var refiendData = _dataRefiner.DataRefine(value, fieldType);
                refiendRow[i] = refiendData.Refined; ;
            }
            return refiendRow;
        }

        /// <summary>
        /// 각 필드의 데이터타입, 패턴 알아냄, 패턴은 데이터 정제기에 기록됨
        /// </summary>
        /// <returns></returns>
        public int FieldSampling(bool forceReset=false) {
            Console.WriteLine("start fields sampling");

            _dataRefiner.Mode_PatternOverride = true; //패턴 재분석 모드 on

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

            _dataRefiner.Mode_PatternOverride = false; //패턴 재분석 모드 off

            return sampledCount;
        }

    }
}
