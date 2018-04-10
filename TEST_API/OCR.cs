using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Drawing;
using System.Linq;

namespace TEST_API
{
    static class OCR
    {
        private static Bitmap changeBitmap = null;
        //言語ファイル
//        const string LANG_PATH = @"C:\tessdata\";

        const string REST_SPI_URI = " http://westus.api.cognitive.microsoft.com/vision/v1.0/ocr";

        public static Bitmap ChangeBitmap { get => changeBitmap; set => changeBitmap = value; }

        // ---------------------------------------------- 
        // OCR REST API
        // ---------------------------------------------- 
        public static dtoOCRResult doOCRRestApiExecute(string _lang, string _filePath)
        {
            return doOCRRestApiExecute( _lang, _filePath, 0, 0);
        }
   
        public static dtoOCRResult doOCRRestApiExecute(string _lang, string _filePath,float _constract, float _sharp)
        {
            try
            {
                prop item = getProp();
                string _accessKey = item.Key;

                string url = "".Equals(item.ReqUrl) ? REST_SPI_URI : item.ReqUrl;
                // リクエスト
                HttpWebRequest req = WebRequest.CreateHttp(url + (_lang.Equals(string.Empty) ? "" : "?language=" + _lang));
                req.Method = "POST";
                req.ContentType = "application/octet-stream";
                req.Headers.Add("Ocp-Apim-Subscription-Key", _accessKey);

                // 画像ファイルの読み込み
                Image _img = Image.FromFile(@_filePath);
                string _fileName = System.IO.Path.GetFileName(_filePath);
                //画像調整
                OCR.ChangeBitmap = clsImageUtils.AdjustSharpness(new Bitmap(clsImageUtils.chContrast(_img, _constract)), _sharp);
                //調整後ファイル保存
                string _nf = Path.GetTempPath();
                _nf += @"\pro_" + _fileName;
                Console.WriteLine(_nf);
                File.Delete(_nf);
                OCR.ChangeBitmap.Save(_nf);
                //調整後のファイルで分析
                using (FileStream _fs = new FileStream(_nf, FileMode.Open, FileAccess.Read))
                {
                    byte[] _b = new byte[_fs.Length];
                    _fs.Read(_b, 0, _b.Length);
                    _fs.Close();
                    //request
                    Stream _rs = req.GetRequestStream();
                    _rs.Write(_b, 0, _b.Length);
                }

                //response
                WebResponse _gr = req.GetResponse();
                Stream _stream = _gr.GetResponseStream();

                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(dtoOCRResult));

                dtoOCRResult result = (dtoOCRResult)dcjs.ReadObject(_stream);
                return result;

            }
            catch (WebException _we)
            {
                Console.WriteLine(_we.Message);
                return null;
            }
            catch (Exception _e)
            {
                Console.WriteLine(_e.Message);
                return null;
            }
        }
        /// <summary>
        /// Tesseract(HP=>google)  open source engine
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_lng"></param>
        /// <param name="_constract"></param>
        /// <param name="_sharp"></param>
        /// <returns></returns>
        public static string doOcr(string _path, string _lng,float _constract,float _sharp)
        {
            System.Drawing.Image _img = null;
            try
            {
                prop item = getProp();
                //言語jpn:eng
                switch (_lng)
                    {
                    case "en":
                        _lng = "eng";
                        break;
                    case "ja":
                        _lng = "jpn";
                        break;
                    default:
                        _lng = "jpn";
                        break;
                }
                if (!inSelect(_lng, "eng", "jpn"))
                {
                    return "Lang error";
                }
                //画像
                _img = Image.FromFile(@_path);
                string _fnm = System.IO.Path.GetFileName(@_path);
                // adjast&constract
                //                ChangeBitmap = clsImageUtils.AdjustSharpness(new Bitmap(clsImageUtils.chContrast(_img, _constract)), _sharp);
                //gray scale & shapeness & constract
                ChangeBitmap = clsImageUtils.AdjustSharpness(new Bitmap(clsImageUtils.AdjustSharpness(new Bitmap(clsImageUtils.chGrayscaleImage(_img)), _constract)), _sharp);
                using (var tes = new Tesseract.TesseractEngine(@item.TessData, _lng))
                {
                    // 加工
                    ChangeBitmap.MakeTransparent();
                    ChangeBitmap.Save(Path.GetTempPath() + @"\mt_" + _fnm);
                    //OCRの実行
                    Tesseract.Page _page = tes.Process(ChangeBitmap);
                    Tesseract.Pix _p=_page.GetThresholdedImage();
                    _p.Save(Path.GetTempPath() + @"\ch_"+ _fnm);
                    Console.WriteLine("" + _page.GetHOCRText(0));
//                    string _s = _page.GetText().Replace("\n", "");
                    string _s = _page.GetText();
                    /*
                    Tesseract.Pix _k = _page.GetThresholdedImage();
                    Tesseract.PixData _d= _k.GetData();
                    */
                    return "".Equals(_s.Trim())?"文字が識別できませんでした":_s +"\r\n 信頼度："+ _page.GetMeanConfidence();
                }
            }
            catch (System.ArgumentException)
            {
                return "【" + _path + "】:Unparsable";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return e.Message;
            }
            finally
            {
                if(_img!=null) _img.Dispose();
                _img = null;
            }
        }
        private static prop getProp()
        {
            prop item = new prop();
            propOpe p = new propOpe();
            return p.getProp();
        }
        /// <summary>
        ///  word select in char
        /// </summary>
        /// <param name="self"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool inSelect(this string self, params string[] values) => self!=null?values.Any(c => c == self):false;
        public static string excDtoOcrResultAnalysis(dtoOCRResult _result)
        {
            string _ret= "";
            if (_result == null || _result.regions.Count() == 0)
            {
//                return _ret;
                _ret = "読み取れませんでした。:un analyysis";
                return _ret;
            }
            foreach (dtoOCRResult.Region region in _result.regions)
            {
                foreach (dtoOCRResult.Line line in region.lines)
                {
                    foreach (dtoOCRResult.Word word in line.words)
                    {
                        _ret += word.text;
//                        if (_result.language == "en") _ret += " ";
                    }
                    _ret += "\r\n";
                }
            }
            return _ret;
        }
    }
}
