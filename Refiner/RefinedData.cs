using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataRefinerModule.Refiner {
    public class RefinedData {

        private List<RefinedData> _additionalDataList = null;

        private string _baseData;

        public string BaseData {
            get { return _baseData; }
            set { _baseData = value; }
        }

        private string _refinedData = null;

        public string Refined {
            get { return _refinedData; }
            set { _refinedData = value; }
        }

        private DataType _dataType = DataType.UNKNOWN;

        public DataType DataType {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public List<RefinedData> AdditionalData {
            get { return _additionalDataList; }
        }

        public void AddAdditionalData(string baseData, string refinedDataString, DataType dataType) {

            RefinedData refinedData = new RefinedData();
            if(_additionalDataList == null) {
                _additionalDataList = new List<RefinedData>();
            }
            refinedData.BaseData = baseData;
            refinedData.Refined = refinedDataString;
            refinedData.DataType = dataType;

            _additionalDataList.Add(refinedData);
        }

        public void AddAdditionalData(string refinedDataString, DataType dataType) {

            string baseData = string.Empty;
            this.AddAdditionalData(baseData, refinedDataString, dataType);

        }

    }
}
