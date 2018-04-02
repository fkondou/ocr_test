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
    class dtoOCRResult
    {
        [DataMember]
        public string language { get; set; }
        [DataMember]
        public float textAngle { get; set; }
        [DataMember]
        public string orientation { get; set; }
        [DataMember]
        public Region[] regions { get; set; }

        [DataContract]
        public class Region
        {
            [DataMember]
            public string boundingBox { get; set; }
            [DataMember]
            public Line[] lines { get; set; }
        }

        [DataContract]
        public class Line
        {
            [DataMember]
            public string boundingBox { get; set; }
            [DataMember]
            public Word[] words { get; set; }
        }

        [DataContract]
        public class Word
        {
            [DataMember]
            public string boundingBox { get; set; }
            [DataMember]
            public string text { get; set; }
        }
    }
}
