using DataRefinerModule.DBUtility;
using DataRefinerModule.Refiner;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataRefinerModule.AddressModule {
    public class AddressExplorer {

        protected List<string> _depthList = new List<string>();
        protected int _maxDepth = 5;

        protected string fieldNm_depthBase = "depth_";
        protected string _schemaName;
        protected string _tableName;
        protected string _relation;

        private string _tableNameSidoDict;
        private string _relationSidoDict;
        private string fieldNm_sido_std_name = "sido_std_name";
        private string fieldNm_sido_name = "sido_name";

        protected char _addressSeparator = ' ';
        protected string _dbWildcardSymbol = "%";

        protected string _symbolValue = "value";
        protected string _symbolNull = "null";

        protected DataType _dataType = DataType.ADDRESS;

        protected List<string> _addressPattern = new List<string>();

        public List<string> DepthList {
            get { return _depthList; }
        }

        public List<string> AddressPattern {
            get { return _addressPattern; }
        }

        public DataType DataType {
            get { return _dataType; }
        }

        public AddressExplorer() {
            _schemaName = "refine_info";
            _tableName = "address_reg_search";
            _relation = $"{_schemaName}.{_tableName}";

            _tableNameSidoDict = "address_sido_dict";
            _relationSidoDict = $"{_schemaName}.{_tableNameSidoDict}";
        }

        public string Pop(bool affectPattern = true) {
            if (_depthList.Count > 0) {
                string target = _depthList[_depthList.Count - 1];
                _depthList.RemoveAt(_depthList.Count -1);

                if(affectPattern)
                    _addressPattern.RemoveAt(_addressPattern.Count - 1);

                return target;
            } else
                return null;
            
        }

        public bool Push(string newDepth, bool affectPattern = true) {
            if(_depthList.Count >= _maxDepth) {
                return false;
            } 
            
            _depthList.Add(newDepth);

            if (affectPattern) {
                if (newDepth == string.Empty) _addressPattern.Add(_symbolNull);
                else _addressPattern.Add(_symbolValue);
            }
            
            return true;
        }

        public bool Clear() {
            _depthList.Clear();
            _addressPattern.Clear();

            return true;
        }

        /// <summary>
        /// return 이 0개면 잘못된 depth, return 이 있는데 결과값이 null 이면 endOfBrach
        /// </summary>
        /// <returns></returns>
        public List<string> GetNextNodes() {
            List<string> resultList = NextNodeSearch();

            return resultList;
        }

        public List<string> GetSiblingNodes() {
            List<string> resultList = SiblingNodeSearch();

            return resultList;
        }

        public string GetNextDepthFieldName() {
            return fieldNm_depthBase + (_depthList.Count).ToString();
        }

        public string GetSiblingDepthFieldName() {
            return fieldNm_depthBase + (_depthList.Count - 1).ToString();
        }

        public List<string> GetAllDepthFieldName() {
            List<string> resultSet = new List<string>();

            for(var idx = 0; idx < _depthList.Count; idx++) {
                string fieldNm = fieldNm_depthBase + (idx);
                resultSet.Add(fieldNm);
            }

            return resultSet;

        }

        public List<string> GetPatternDepthFieldName() {
            List<string> resultSet = new List<string>();

            for (var idx = 0; idx < _depthList.Count; idx++) {
                string depthValue = _depthList[idx];
                if (depthValue == string.Empty) continue;
                string fieldNm = fieldNm_depthBase + (idx);
                resultSet.Add(fieldNm);
            }

            return resultSet;

        }

        public List<string> GetDepthFieldList() {
            List<string> fieldList = new List<string>();
            for (var i = 0; i < _depthList.Count; i++) {
                string depthValue = _depthList[i];
                if (depthValue != string.Empty) {
                    string fieldName = (fieldNm_depthBase + i).ToString();
                    fieldList.Add(fieldName);
                }
            }
            return fieldList;
        }

        public List<string> GetDepthValueList() {
            List<string> fieldList = new List<string>();
            for (var i = 0; i < _depthList.Count; i++) {
                string depthValue = _depthList[i];
                if (depthValue != string.Empty) {
                    fieldList.Add(depthValue);
                }
            }
            return fieldList;
        }

        public int GetValidDepthCount() {
            int validDepthCount = 0;

            foreach (string depthValue in _depthList) {
                if (depthValue != string.Empty) validDepthCount++;
            }

            return validDepthCount;
        }

        public void TrimList(List<string> list) {
            string[] trimSymbol = new string[] {
                string.Empty,
                "null",
            };

            int startIndex = -1;
            for(var i = 0; i < list.Count; i++) {
                string value = list[i];

                if (trimSymbol.Contains(value)) {
                    if(startIndex == -1)
                        startIndex = i;
                } else {
                    if (startIndex != -1)
                        startIndex = -1;
                }
            }

            if(startIndex != -1)
                list.RemoveRange(startIndex, list.Count - startIndex);
        }


        /// <summary>
        /// _depthList 에 유효한 value 가 있는지 체크
        /// </summary>
        /// <returns></returns>
        public bool IsExistValidDepth() {

            bool isEixst = false;
            foreach (string depthValue in _depthList) {
                if (depthValue != string.Empty) return true;
            }
            return isEixst;

        }

        /// <summary>
        /// 패턴을 갖고 있는지 확인
        /// </summary>
        /// <returns></returns>
        public bool HasPattern() {
            return _addressPattern.Count > 0;
        }

        /// <summary>
        /// 시/도 명 표준명으로 정제
        /// </summary>
        /// <param name="baseSido">정제할 시/도명</param>
        /// <returns></returns>
        public string GetRefinedSidoName(string baseSido) {
            DBConnector dbConn = DBConnector.GetDefaultConnector();

            List<string> selectList = new List<string> {
                fieldNm_sido_std_name,
            };
            string selectString = DBConnector.GetSelectString(selectList);

            List<string> idFields = new List<string>() {
                fieldNm_sido_name,
            };
            List<string> idValues = new List<string>() {
                baseSido,
            };
            string conditionString = DBConnector.GetConditionString(idFields, idValues, "AND");

            string result = dbConn.SelectSingleObjectPostgreSql($"SELECT {selectString} FROM {_relationSidoDict} WHERE {conditionString}");

            return result;
        }

        /// <summary>
        /// 현재 저장된 depth 값 기준으로 다음 노드들을 가져옴
        /// </summary>
        /// <returns></returns>
        public List<string> NextNodeSearch() {

            DBConnector dbConn = DBConnector.GetDefaultConnector();

            List<string> resultList = new List<string>();
            string nextDepthFieldName = GetNextDepthFieldName();

            List<string> selectList = new List<string> {
                    nextDepthFieldName,
            };
            string selectString = DBConnector.GetSelectString(selectList);

            List<string> idFields = GetDepthFieldList();
            List<string> idValues = GetDepthValueList();
            string conditionString = DBConnector.GetConditionString(idFields, idValues, "AND", "LIKE");

            List<string[]> resultSet = dbConn.SelectPostgreSql($"SELECT DISTINCT {selectString} FROM {_relation} WHERE {conditionString}");
            if (resultSet == null || resultSet.Count == 0) return resultList;

            foreach (string[] row in resultSet) {
                string value = row[0];
                resultList.Add(value);
            }

            return resultList;

        }

        /// <summary>
        /// 현재 저장된 depth 값 기준으로 형제 노드들을 가져옴
        /// </summary>
        /// <returns></returns>
        public List<string> SiblingNodeSearch() {

            DBConnector dbConn = DBConnector.GetDefaultConnector();

            List<string> resultList = new List<string>();
            string siblingDepthFieldName = GetNextDepthFieldName();

            List<string> selectList = new List<string> {
                    siblingDepthFieldName,
            };
            string selectString = DBConnector.GetSelectString(selectList);

            List<string> idFields = GetDepthFieldList();
            List<string> idValues = GetDepthValueList();
            string conditionString = DBConnector.GetConditionString(idFields, idValues, "AND", "LIKE");

            List<string[]> resultSet = dbConn.SelectPostgreSql($"SELECT DISTINCT {selectString} FROM {_relation} WHERE {conditionString}");
            if (resultSet == null || resultSet.Count == 0) return resultList;

            foreach (string[] row in resultSet) {
                string value = row[0];
                resultList.Add(value);
            }

            return resultList;

        }

        /// <summary>
        /// 현재 입력된 depth 에 따른 address 가져오기
        /// </summary>
        /// <returns></returns>
        public string GetAddress(bool fullAddress = false) {

            DBConnector dbConn = DBConnector.GetDefaultConnector();

            List<string> selectList;
            if (fullAddress) {
                selectList = GetAllDepthFieldName();
            } else {
                selectList = GetPatternDepthFieldName();
            }
            string selectString = DBConnector.GetSelectString(selectList);

            List<string> idFields = GetDepthFieldList();
            List<string> idValues = GetDepthValueList();
            string conditionString = DBConnector.GetConditionString(idFields, idValues, "AND", "LIKE");

            List<string[]> resultSet = dbConn.SelectPostgreSql($"SELECT DISTINCT {selectString} FROM {_relation} WHERE {conditionString}");
            if (resultSet == null || resultSet.Count == 0) return string.Empty;

            string[] valueSet = resultSet[0];

            string address = string.Empty;

            int validValueCount = GetValidDepthCount();

            for (var i = 0; i < _addressPattern.Count; i++) {
                string symbol = _addressPattern[i];
                if (!fullAddress && symbol == _symbolNull) continue;

                if (i >= valueSet.Length) break;

                address += valueSet[i] + _addressSeparator;
            }

            return address.Trim();

        }


        /// <summary>
        /// 주소 패턴을 찾아서 찾으면 _depthList 와 _addressPattern 에 저장
        /// </summary>
        /// <param name="depths">주소 형태소 목록</param>
        /// <returns></returns>
        public bool FindPattern(string[] depths) {

            Clear();

            //사실 상 root 라고도 할 수 있는 시/도 push
            int nowIdx = 0;

            string firstDepth = depths[nowIdx];
            string refinedSidoName = GetRefinedSidoName(firstDepth);
            bool isExistSido = (refinedSidoName != string.Empty);

            if (isExistSido) {
                Push(refinedSidoName);
                nowIdx++;
            } else {
                Push(string.Empty);
            }

            //이후 depth 에 대해 최종 root 찾기
            while (nowIdx < depths.Length) {
                string nowDepth = depths[nowIdx] + _dbWildcardSymbol;
                bool isCanPush = Push(nowDepth);
                if (!isCanPush) break;

                List<string> nextNodes = GetNextNodes();
                if (nextNodes.Count == 0) { //잘못된 depth 값일 시
                    Pop();
                    Push(string.Empty);
                    continue;
                } else { //현재 depth 가 leaf 라면
                    if (nextNodes.Count == 1 && nextNodes[0] == string.Empty) {
                        nowIdx++;
                        break;
                    }
                }
                nowIdx++;
            }

            TrimList(_depthList);
            TrimList(_addressPattern);

            if (!IsExistValidDepth()) {
                return false;
            } return true;

        }

        /// <summary>
        /// _addressPattern 에 pattern 적용
        /// </summary>
        /// <param name="pattern">적용할 패턴</param>
        /// <returns></returns>
        public bool ApplyPattern(List<string> pattern) {
            _addressPattern = pattern;

            return true;
        }

        /// <summary>
        /// _addresspattern 에 맞춰 depths 를 depthList 에 주입
        /// </summary>
        /// <returns></returns>
        public bool SetDepths(string[] depths) {

            _depthList.Clear();
            for(var i = 0; i < _addressPattern.Count; i++) {
                string symbol = _addressPattern[i];
                if (i >= depths.Length) break;

                if (symbol == _symbolValue) {
                    string depthValue = depths[i];

                    if (i == 0) {
                        string refinedSidoName = GetRefinedSidoName(depthValue);
                        if (refinedSidoName != string.Empty) {
                            depthValue = refinedSidoName;
                        } else {
                            depthValue += _dbWildcardSymbol;
                        }
                    } else {
                        depthValue += _dbWildcardSymbol;
                    }

                    Push(depthValue, false);
                } else if(symbol == _symbolNull) {
                    Push(string.Empty, false);
                }
            }

            return true;

        }

    }
}
