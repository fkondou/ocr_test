using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 追加
using System.Runtime.Serialization;

namespace TEST_API
{
    [DataContract]
    class dtoResult
    {
        [DataMember]
        public string fileName { get; set; }
        [DataMember]
        public string filePath { get; set; }
        [DataMember]
        public string OcrText { get; set; }
    }
}
