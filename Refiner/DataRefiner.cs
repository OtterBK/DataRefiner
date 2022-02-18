using DataRefinerModule.AddressModule;
using DataRefinerModule.DBUtility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataRefinerModule.Refiner {
    public class DataRefiner {

        private CultureInfo _cultureInfo;
        private DateTimeStyles _dateTimeStyles;

        private string _stdDate = "yyyy-MM-dd";
        private string _stdTime = "HH:mm";

        private string[] _numberComma = new string[] {
            ",",
            "\"",
            "\'",
        };

        private List<string> _addressPattern;

        private char _addressSeparator = ' ';
        private string _dbWildcardSymbol = "%";

        public bool Mode_PatternOverride = true;

        public DataRefiner() {

            //문화권은 현지
            _cultureInfo = CultureInfo.CurrentCulture; ;

            //dateTime parsing style 지정
            //참고: https://docs.microsoft.com/ko-kr/dotnet/api/system.globalization.datetimestyles?view=net-6.0
            _dateTimeStyles = DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal;

        }

        public RefinedData DataRefine(string baseData) {

            RefinedData refinedData = new RefinedData();
            refinedData.BaseData = baseData;

            if(baseData != null && baseData != string.Empty) {
                //Datetime 형식 시도
                if (ExtractDateTime(refinedData)) return refinedData;

                //일반 숫자형식 시도
                if (ExtractNumeric(refinedData)) return refinedData;

                //주소 시도
                if (ExtractAddress(refinedData)) return refinedData;

                //텍스트 시도
                if (ExtractText(refinedData)) return refinedData;
            }

            return refinedData;

        }

        public RefinedData DataRefine(string baseData, DataType dataType) {

            RefinedData refinedData = new RefinedData();
            refinedData.BaseData = baseData;

            if(baseData != null && baseData != string.Empty) {
                switch (dataType) {

                    case DataType.TIME: ExtractDateTime(refinedData); break;
                    case DataType.DATE: ExtractDateTime(refinedData); break;
                    case DataType.DATETIME: ExtractDateTime(refinedData); break;

                    case DataType.NUMERIC: ExtractNumeric(refinedData); break;

                    case DataType.ADDRESS: ExtractAddress(refinedData); break;
                    case DataType.ADDRESS_LOAD: ExtractAddress(refinedData); break;
                    case DataType.ADDRESS_REG: ExtractAddress(refinedData); break;

                    case DataType.TEXT: ExtractText(refinedData); break;

                    default: refinedData = DataRefine(baseData); break;
                }
            }

            return refinedData;

        }

        private static bool IsNumeric(string input) {
            return int.TryParse(input, out int number);
        }

        /// <summary>
        /// 날짜 및 시간 변환 시도 후 성공 시 날짜, 시간, 날짜+시간 으로 정제, 정제 기준은 _stdDate, _stdTime
        /// CultureInfo 는 표준화할 문화권, _dateTimeStyles 는 NoCurrentDateDefault 필수
        /// </summary>
        /// <param name="refinedData">정제 대상 데이터 baseData 기준으로 추출</param>
        /// <returns></returns>
        public bool ExtractDateTime(RefinedData refinedData) {
            string baseData = refinedData.BaseData;
            bool isFind = false;

            DateTime dtDateTime;
            bool isDateTime = DateTime.TryParse(baseData, _cultureInfo, _dateTimeStyles, out dtDateTime);
            if (isDateTime) {
                bool isIncludeTimeData = (dtDateTime.Hour != 0) | (dtDateTime.Minute != 0) | (dtDateTime.Second != 0);
                if (isIncludeTimeData) {
                    bool isOnlyTimeData = (dtDateTime.Year == 1) & (dtDateTime.Month == 1) & (dtDateTime.Day == 1);
                    if (isOnlyTimeData) {
                        isFind = true;
                        refinedData.Refined = dtDateTime.ToString($"{_stdTime}");
                        refinedData.DataType = DataType.TIME;
                    } else {
                        isFind = true;
                        refinedData.Refined = dtDateTime.ToString($"{_stdDate} {_stdTime}");
                        refinedData.DataType = DataType.DATETIME;
                        CalculateAdditionalData(refinedData, dtDateTime);
                    }
                } else {
                    isFind = true;
                    refinedData.Refined = dtDateTime.ToString($"{_stdDate}");
                    refinedData.DataType = DataType.DATE;
                    CalculateAdditionalData(refinedData, dtDateTime);
                }
            }

            return isFind;
        }

        /// <summary>
        /// dtDateTime 에서 날짜 관련 추가 정보 추출, 예) 반기, 분기
        /// </summary>
        /// <param name="refinedData">추가 정보를 삽입할 정제된 데이터 객체</param>
        /// <param name="dtDateTime">추가 정보를 추출할 dateTime</param>
        private void CalculateAdditionalData(RefinedData refinedData, DateTime dtDateTime) {

            string yearHalf;
            if(dtDateTime.Month > 6) {
                yearHalf = "하반기";
            } else {
                yearHalf = "상반기";
            }

            string yearQuarter = (dtDateTime.Month % 3 == 0 ? dtDateTime.Month / 3 : dtDateTime.Month / 3 + 1).ToString();

            refinedData.AddAdditionalData(yearHalf, DataType.YEARHALF);
            refinedData.AddAdditionalData(yearQuarter, DataType.YEARQUARTER);

        }

        /// <summary>
        /// refinedData 의 baseData에서 숫자 형태로 변환, 무시할 특수기호는 _comma 에 지정 예) 1,000 -> 1000
        /// </summary>
        /// <param name="refinedData">정제 대상 데이터 baseData 기준으로 추출</param>
        /// <returns></returns>
        private bool ExtractNumeric(RefinedData refinedData) {
            string baseData = refinedData.BaseData;
            bool isFind = false;

            string tmpBaseData = baseData;
            foreach (string comma in _numberComma) {
                tmpBaseData = tmpBaseData.Replace(comma, "");
            }
            bool isNumericData = int.TryParse(tmpBaseData, out int numericData);
            if (isNumericData) {
                refinedData.DataType = DataType.NUMERIC;
                refinedData.Refined = numericData.ToString();
                isFind = true;
            }

            return isFind;
        }


        public bool ExtractAddress(RefinedData refinedData) {

            string baseData = refinedData.BaseData;
            bool isFind = false;

            AddressExplorer addressExplorer = null;

            if (DataRefinerForm.addressType == 0) addressExplorer = new AddressRegExplorer();
            else if (DataRefinerForm.addressType == 1) addressExplorer = new AddressLoadExplorer();
            else if (DataRefinerForm.addressType == 2) addressExplorer = new AddressSggExplorer();

            string refinedAddress = string.Empty;

            string[] depths = baseData.Split(_addressSeparator);

            bool isFindPattern = false;
            //패턴 기반 탐색(여러 데이터 정제시 유효)
            if (_addressPattern != null) { //이미 분석한 addressPattern 이 있다면
                addressExplorer.SetAddressPattern(_addressPattern); //address 패턴 적용
                addressExplorer.SetDepths(depths); //패턴에 맞춰 depths 적용
                string tmpAddress = addressExplorer.GetAddress(fullAddress: true); //주소 있는지 찾아봄
                if (tmpAddress != string.Empty) {//있다면
                    isFindPattern = true;
                    refinedAddress = tmpAddress;
                }
            }

            //기존 패턴이 없다면
            if (!isFindPattern) {
                isFindPattern = addressExplorer.FindPattern(depths); //패턴 찾기
            }
            
            //찾은 패턴이 있다면
            if (isFindPattern) {
                if(Mode_PatternOverride) _addressPattern = addressExplorer.AddressPattern; //패턴 저장
                int validDepthCount = addressExplorer.GetValidDepthCount();

                if(refinedAddress == string.Empty) 
                    refinedAddress = addressExplorer.GetAddress(fullAddress:true);

                //남은 주소들 붙이기
                while (validDepthCount < depths.Length) {
                    refinedAddress += _addressSeparator + depths[validDepthCount++];
                }

                refinedData.Refined = refinedAddress;
                refinedData.DataType = addressExplorer.DataType;
                isFind = true;
            }

            return isFind;
        }

        /// <summary>
        /// 일반 문자 정제
        /// </summary>
        /// <param name="refinedData">정제 대상 데이터 baseData 기준으로 추출</param>
        /// <returns></returns>
        public bool ExtractText(RefinedData refinedData) {
            string baseData = refinedData.BaseData;
            bool isFind = false;

            refinedData.Refined = baseData;
            refinedData.DataType = DataType.TEXT;
            isFind = true;

            return isFind;
        }

    }
}
