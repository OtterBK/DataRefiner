using DataRefinerModule.Refiner;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataRefinerModule.AddressModule {
    public class AddressRegExplorer : AddressExplorer{

        
        public AddressRegExplorer() {
            _maxDepth = 5;

            _schemaName = "refine_info";
            _tableName = "address_reg_search";
            _relation = $"{_schemaName}.{_tableName}";

            _dataType = DataType.ADDRESS_REG;
        }

    }
}
