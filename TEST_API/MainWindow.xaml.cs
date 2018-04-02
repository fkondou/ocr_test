using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
// 追加
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using OpenCvSharp;
using System.Linq;

namespace TEST_API
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public Dictionary<string, string> _mapResult = null;
        public Dictionary<string, Bitmap> _mapView = null;

        const string CRLF = "\r\n";

        // ---------------------------------------------- 
        // アクセスキー
        // ---------------------------------------------- 
        //        public const string accessKey = "ea22e8d199e64950a36f08b999b6b215";

        public MainWindow()
        {
            InitializeComponent();


        }
        // ---------------------------------------------- 
        // 画像ファイルを選択ボタン押下
        // ---------------------------------------------- 
        private void btnSelectImageFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.CheckFileExists = true;
                openFileDialog.FileName = "";
                openFileDialog.Filter = "ImageFile|*.jpg;*jpeg;*.png;*.gif;*.bmp";
                openFileDialog.FilterIndex = 0;
                if (openFileDialog.ShowDialog() == true)
                {
                    BitmapImage _bmp = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute));
                    imgOCR.Source = _bmp;
                }
                else
                {
                    imgOCR.Source = null;
                }
            }
            catch (Exception exception)
            {
                tboxResult.Text = exception.Message;
            }
            finally
            {
            }
        }
        // ---------------------------------------------- 
        // OKボタン押下
        // ---------------------------------------------- 
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Wait;
            string _lang = ((ComboBoxItem)cmbLanguage.SelectedItem).Content.ToString();
            if (_lang.Equals(string.Empty) || _lang == null) _lang = "jpn";
            if (_lang.Equals("jp")) _lang = "jpn";
            if (_lang.Equals("en")) _lang = "eng";
            string _dir = this.lbl_Dir.Content.ToString();
            float _constract = float.Parse(txt_constract.Text);
            float _sharpe = float.Parse(txt_sharpe.Text);
            try
            {
                if (!_dir.Equals(string.Empty))
                {
                    string[] _file = getFilePath(_dir);

                    if (_mapResult == null) _mapResult = new Dictionary<string, string>();
                    if (_mapView == null) _mapView = new Dictionary<string, Bitmap>();

                    foreach (string nm in _file)
                    {
                        dtoOCRResult result = OCR.doOCRRestApiExecute(_lang: _lang, _filePath: nm, _constract, _sharpe);
                        string _resultText = OCR.excDtoOcrResultAnalysis(result);

                        tboxResult.Text += "";
                        //entry hashmap
                        if (_mapResult.ContainsKey(nm))
                        {
                            _mapResult[nm] = _resultText;
                            _mapView[nm] = OCR.ChangeBitmap;
                        }
                        else
                        {
                            _mapResult.Add(nm, _resultText);
                            _mapView.Add(nm, OCR.ChangeBitmap);
                            //ListBox entry
                            lst_Result.Items.Add(nm);
                        }
                    }
                }
                else
                {
                    this.IsEnabled = false;
                    tboxResult.Text = "";
                    string language = ((ComboBoxItem)cmbLanguage.SelectedItem).Content.ToString();
                    string filePath = ((BitmapImage)imgOCR.Source).UriSource.LocalPath;
                    dtoOCRResult result = OCR.doOCRRestApiExecute(language, filePath, _constract, _sharpe);
                    tboxResult.Text = OCR.excDtoOcrResultAnalysis(result);
                }
            }
            catch (Exception exception)
            {
                tboxResult.Text = exception.Message;
            }
            finally
            {
                this.IsEnabled = true;
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                Cursor = null;
            }
        }
        /// <summary>
        /// select on target to the folder for the directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDir_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog _fbd = new FolderBrowserDialog();
            if (_mapResult == null) _mapResult = new Dictionary<string, string>();
            _fbd.Description = "読取画像があるフォルダを選択してください";
            _fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            //            _fbd.SelectedPath = @"c:\inetpub\wwwroot";
            //［新しいフォルダ］ボタン非表示
            _fbd.ShowNewFolderButton = false;
            DialogResult _dir = _fbd.ShowDialog();
            if (_fbd.SelectedPath == null || _fbd.SelectedPath.Equals(string.Empty))
            {
                lbl_Dir.Content = "";
            }
            else
            {
                lst_Result.Items.Clear();
                if (_mapResult != null) _mapResult = null;
                _mapResult = new Dictionary<string, string>();
                string[] _file = getFilePath(_fbd.SelectedPath);
                foreach (string nm in _file)
                {
                    _mapResult.Add(nm, "");
                    //ListBox entry
                    lst_Result.Items.Add(nm);
                }
                lbl_Dir.Content = _fbd.SelectedPath;
            }
        }
        /// <summary>
        /// Use System.IO.Directory. Search directory for the get the path of the file. by the way sub Directory also apply.
        /// </summary>
        /// <param name="_DirPath"></param>
        /// <returns></returns>
        private string[] getFilePath(string _DirPath)
        {
            return getFilePath(_DirPath, "*.jpg");
        }
        private string[] getFilePath(string _DirPath, string _optword)
        {
            return System.IO.Directory
                .GetFiles(@_DirPath, _optword, System.IO.SearchOption.AllDirectories);
        }

        private void btnAnalysis_Click(object sender, RoutedEventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Wait;
            if (_mapResult == null) _mapResult = new Dictionary<string, string>();
            if (_mapView == null) _mapView = new Dictionary<string, Bitmap>();
            string _lan = ((ComboBoxItem)cmbLanguage.SelectedItem).Content.ToString();
            float _constract = float.Parse(txt_constract.Text);
            float _sharpe = float.Parse(txt_sharpe.Text);
            //言語jpn:eng
            if (_lan.Equals(string.Empty) || _lan == null) _lan = "jpn";
            if (_lan.Equals("jp")) _lan = "jpn";
            if (_lan.Equals("en")) _lan = "eng";
            string _dir = this.lbl_Dir.Content.ToString();
            if (!_dir.Equals(string.Empty))
            {
                string[] _file = getFilePath(_dir);


                foreach (string nm in _file)
                {
                    string _resultText = OCR.doOcr(nm, _lan, _constract, _sharpe);
                    //entry hashmap
                    if (_mapResult.ContainsKey(nm))
                    {
                        _mapResult[nm] = _resultText;
                        _mapView[nm] = OCR.ChangeBitmap;
                    }
                    else
                    {
                        _mapResult.Add(nm, _resultText);
                        _mapView.Add(nm, OCR.ChangeBitmap);
                        //ListBox entry
                        lst_Result.Items.Add(nm);
                    }
                }

            }
            else
            {
                string _selFilePath = ((BitmapImage)imgOCR.Source).UriSource.LocalPath;
                if (_selFilePath != null && !_selFilePath.Equals(string.Empty))
                {
                    this.tboxResult.Text
                        += "【" + System.IO.Path.GetFileName(_selFilePath) + "】"
                    //                        + CRLF + "result:" + getOCRResult(nm) + CRLF;
                    + CRLF + "result:" + OCR.doOcr(_selFilePath, _lan, _constract, _sharpe) + CRLF;
                    string _newfile = string.Format(@"c:\tool\{0}_{1}.bmp", System.IO.Path.GetFileName(_selFilePath), _constract);
                    if (OCR.ChangeBitmap != null) OCR.ChangeBitmap.Save(_newfile, ImageFormat.Bmp);
                    imgOCR.Source = new BitmapImage(new Uri(_newfile, UriKind.RelativeOrAbsolute));
                    OCR.ChangeBitmap.Dispose();
                }

            }
            Cursor = null;
        }

        private void lst_Result_Select(object sender, SelectionChangedEventArgs e)
        {
            int _idx = lst_Result.SelectedIndex;
            if (_idx < 0) return;
            string _key = lst_Result.Items[_idx].ToString();
            string _txt = "";
            if (_mapResult == null) return;
            if (_mapView == null) return;
            // reload resul, view from Map.
            if (_mapResult.ContainsKey(_key))
            {
                _txt = _mapResult[_key];
            }
            imgOCR.Source = new BitmapImage(new Uri(_key, UriKind.RelativeOrAbsolute));
            imgOCR.ToolTip = _txt;
            tboxResult.Text = _txt;
            tboxResult.ToolTip = _key;
            // setting for Basic View from Map
            Bitmap _bmp = null;
            if (_mapView.ContainsKey(_key))
            {
                _bmp = _mapView[_key];
            }
            if (_bmp == null) return;
            //ToolTip
            System.Windows.Controls.ToolTip toolTip = new System.Windows.Controls.ToolTip();

            //stackPanel
            StackPanel stackPanel = new StackPanel();

            stackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

            // Save the change to after image in memory. and the ssetting for tooltip
            BitmapImage _afterImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                _bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                _afterImage.BeginInit();
                _afterImage.StreamSource = memory;
                _afterImage.CacheOption = BitmapCacheOption.OnLoad;
                _afterImage.EndInit();
            }

            // Setting for Image to ToolTip.
            System.Windows.Controls.Image img_renew = new System.Windows.Controls.Image();
            img_renew.Source = _afterImage;
            stackPanel.Children.Add(img_renew);
            toolTip.Content = stackPanel;
            imgOCR.ToolTip = toolTip;
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearItemValue();
        }

        private void tboxResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tboxResult.Text.Equals(string.Empty))
            {
                imgOCR.ToolTip = null;
                imgOCR.Source = null;
                this.lbl_Dir.Content=string.Empty;
            }
        }
        private void ClearItemValue()
        {
            if (lst_Result.Items.Count >= 0) lst_Result.Items.Clear();
            if (imgOCR.ToolTip!=null) imgOCR.ToolTip = null;
            if (imgOCR.Source != null) imgOCR.Source = null;
            if (_mapResult != null) _mapResult = null;
            if (_mapView != null) _mapView = null;
            this.lbl_Dir.Content = string.Empty;
            tboxResult.Text = "";
        }

        private void Btn_capture_Click(object sender, RoutedEventArgs e)
        {
            VideoCapture capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                System.Windows.MessageBox.Show("cannot open camera");
                capture.Dispose();
                capture = null;
                return;
            }
            else
            {
                System.Windows.Forms.Form frm = new Form1(capture);
                frm.Show();
            }
        }
    }
}
