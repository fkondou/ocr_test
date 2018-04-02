using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TEST_API
{
    class propOpe
    {
        public propOpe()
        {

        }
        const String SETTING_FILE_NAME = "settings.xml";
        public Boolean saveProp(prop item)
        {
            try
            {
                // XmlSerializerを使ってファイルに保存（オブジェクトの内容を書き込む）
                XmlSerializer serializer = new XmlSerializer(typeof(prop));

                // カレントディレクトリにファイルで書き出す
                using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + SETTING_FILE_NAME, FileMode.Create))
                {
                    // オブジェクトをシリアル化してXMLファイルに書き込む
                    serializer.Serialize(fs, item);
                    fs.Close();
                }
            }
            catch (Exception _e)
            {
                item.Log = _e.StackTrace;
                System.Windows.Forms.Clipboard.SetText(item.Log);
            }
            Console.WriteLine("file:"+ Directory.GetCurrentDirectory() + "\\" + SETTING_FILE_NAME);
            return true;
        }
        public prop getProp()
        {
            // XMLを読み込む
            prop item = new prop();
            XmlSerializer serializer = new XmlSerializer(typeof(prop));
            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\" + SETTING_FILE_NAME, FileMode.Open))
            {
                // XMLファイルを読み込み、逆シリアル化（復元）する
                item = (prop)serializer.Deserialize(fs);
                fs.Close();
            }
            return item;
        }
    }
}
