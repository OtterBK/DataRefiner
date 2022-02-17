using DataRefinerModule.Refiner;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataRefinerModule.AddressModule {
    public class AddressLoadExplorer : AddressExplorer{

        public AddressLoadExplorer() {
            _maxDepth = 4;

            _schemaName = "refine_info";
            _tableName = "address_load_search";
            _relation = $"{_schemaName}.{_tableName}";

            _dataType = DataType.ADDRESS_LOAD;
        }

    }
}
