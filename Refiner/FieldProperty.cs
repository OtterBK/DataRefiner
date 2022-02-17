using DataRefinerModule.Refiner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataRefinerModule.Refiner {

    public class FieldProperty {
        public string FieldName { get; set; }
        public DataType FieldType { get; set; }

        public FieldProperty(string fieldName) {
            FieldName = fieldName;
            FieldType = DataType.UNKNOWN;
        }

        public FieldProperty(string fieldName, DataType fieldType) : this(fieldName) {
            FieldType = fieldType;
        }

    }
}
