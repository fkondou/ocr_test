using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TEST_API
{
    public class prop
    {
        public prop()
        {

        }
        private string _key;
        private string _url;
        private string _tess;
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        public string TessData
        {
            get { return _tess; }
            set { _tess = value; }
        }
        public string ReqUrl
        {
            get { return _url; }
            set { _url = value; }
        }
        public string Log = "";

    }
}
