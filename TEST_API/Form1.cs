using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace TEST_API
{
    public partial class Form1 : Form
    {
        int WIDTH = 640;
        int HEIGHT = 480;
        Mat frame;
        VideoCapture capture;
        Bitmap captBmp;
        Bitmap gray;
        Mat capMat;
        Graphics graphic;

        public Form1(VideoCapture c)
        {
            InitializeComponent();
            //int WIDTH = this.pictureBox.Width;
            //int HEIGHT = this.pictureBox.Height;
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            //カメラ画像取得
            capture = c;
            // capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                MessageBox.Show("cannot open camera");
                this.Close();
                return;
            }
            capture.FrameWidth = WIDTH;
            capture.FrameHeight = HEIGHT;

            //画像取得クラス
            frame = new Mat(HEIGHT, WIDTH, MatType.CV_8UC3);
            capMat = frame;

            //表示用にBitmap変換
            captBmp = new Bitmap(frame.Cols, frame.Rows, (int)frame.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, frame.Data);
            this.Width = WIDTH;
            this.Height = HEIGHT;

            //サイズ調整
            this.pictureBox.Width = frame.Cols;
            this.pictureBox.Height = frame.Rows;

            //Graphicsクラスで描画
            graphic = this.pictureBox.CreateGraphics();

            //setup back ground thread for camera capture
            this.backgroundWorker.RunWorkerAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string _s = getTmpFileNm("gray");
            if (gray != null) {
            Bitmap _b = (Bitmap)gray.Clone();
            _b.Save(_s);
            _b.Dispose();
        }
            if (frame != null) frame.Dispose();
            if (capture != null) capture.Dispose();
            if (captBmp != null) captBmp.Dispose();
//            if (gray != null) gray.Dispose();
            if (capMat != null) capMat.Dispose();
            if (graphic != null) graphic.Dispose();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;
            while (!backgroundWorker.CancellationPending)
            {
                //画像取得
                capture.Read(frame);
                if (!capture.IsOpened())
                {
                    this.backgroundWorker.CancelAsync();
                    while (this.backgroundWorker.IsBusy)
                        Application.DoEvents();
                    return;
                }
                else
                {
                    capture.Grab();
                }
                NativeMethods.videoio_VideoCapture_operatorRightShift_Mat(capture.CvPtr, frame.CvPtr);

                bw.ReportProgress(0);

            }

        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                Bitmap _b = (Bitmap)captBmp.Clone();
                graphic.DrawImage(_b, 0, 0, frame.Cols, frame.Rows);
                _b.Dispose();
                if ("".Equals(this.textBox1.Text))
                {
                    this.textBox1.Text = "start";
                }
            }
            catch (Exception _e)
            {
                Console.WriteLine(_e.Message);
            }
        }

        private void pic_click(object sender, EventArgs e)
        {

            writeFacePoint(capMat.Clone());
//            chGrayscaleImage(capMat.Clone());
//            MessageBox.Show(doOcr());
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        // execute tesseract-OCR
        private string doOcr()
        {
            string _f = getTmpFileNm("base");
            System.IO.File.Delete(_f);
            string _nf= doCorrect(captBmp);
            using (Mat _m = OpenCvSharp.Extensions.BitmapConverter.ToMat(captBmp))
            {
                Mat _mg = new Mat();
                Cv2.CvtColor(_m, _mg, ColorConversionCodes.BGR2GRAY);
                Bitmap _nb = new Bitmap(_mg.Cols, _mg.Rows, (int)_mg.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, _mg.Data);
                gray = (Bitmap)_nb.Clone();
//                Bitmap.UnlockBits(_nb);
            }
            return OCR.doOcr(_nf, "", 100, 10).Replace("\n", "");
        }

        private string getTmpFileNm(string _h)
        {
            string _tmp_path = Path.GetTempPath();
            string _n = _h + "_"+System.DateTime.Now.ToString("yyyyMMddHHmmss");
            return  _tmp_path + _n + ".bmp";
        }

        private void writeFacePoint(Mat _mat)
        {
            using (var haarCascade = new CascadeClassifier("haarcascade_frontalface_default.xml"))

            using (var src = _mat)
            using (var gray = new Mat())
            {
                var result = src.Clone();
                Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                //検出
                Rect[] faces = haarCascade.DetectMultiScale(
                    gray, 1.2, 2, HaarDetectionType.FindBiggestObject, new OpenCvSharp.Size(50, 50));
                Bitmap _nb = new Bitmap(gray.Cols, gray.Rows, (int)gray.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, gray.Data);
                Bitmap _tmp = (Bitmap)_nb.Clone();
//                string _nf = getTmpFileNm("gray0");
//                _tmp.Save(_nf);
                //描画位置
                foreach (Rect face in faces)
                {
                    //楕円用position
                    var center = new OpenCvSharp.Point
                    {
                        X = (int)(face.X + face.Width * 0.5),
                        Y = (int)(face.Y + face.Height * 0.5)
                    };
                    var axes = new OpenCvSharp.Size
                    {
                        Width = (int)(face.Width * 0.5),
                        Height = (int)(face.Height * 0.5)
                    };
                    //長方形用position
                    var center2 = new OpenCvSharp.Point
                    {
                        X = (int)(face.X - (face.Width / face.X)),
                        Y = (int)(face.Y - (face.Height / face.Y))
                    };
                    var center3 = new OpenCvSharp.Point
                    {
                        X = (int)(center2.X + face.Width),
                        Y = (int)(center2.Y + face.Height)
                    };
                    //楕円表示(scalar:BGR)
                    Cv2.Ellipse(result, center, axes, 0, 0, 360, new Scalar(0, 0, 255), 4);
                    //四角表示
                    Cv2.Rectangle(result, center2, center3, new Scalar(255, 0, 0), 4);
                }
                using (new Window("result", result))
                {
                    Cv2.WaitKey();
                }
            }
        }
        private string doCorrect(Bitmap _inB)
            {
                // 入力画像中の四角形の頂点座標
                var srcPoints = new Point2f[] {
                new Point2f(69, 110),
                new Point2f(81, 857),
                new Point2f(1042, 786),
                new Point2f(1038, 147),
            };

                // srcで指定した4点が、出力画像中で位置する座標
                var dstPoints = new Point2f[] {
                new Point2f(0, 0),
                new Point2f(0, 480),
                new Point2f(640, 480),
                new Point2f(640, 0),
            };

            using (var matrix = Cv2.GetPerspectiveTransform(srcPoints, dstPoints))
            using (var src = OpenCvSharp.Extensions.BitmapConverter.ToMat(_inB))
            using (var dst = new Mat(new OpenCvSharp.Size(640, 480), MatType.CV_8UC3))
            {
                // 透視変換
                Cv2.WarpPerspective(src, dst, matrix, dst.Size());
                using (new Window("result", dst))
                {
                    Bitmap bitmap = new Bitmap(dst.Cols, dst.Rows, (int)dst.Step(), System.Drawing.Imaging.PixelFormat.Format24bppRgb, dst.Data);
                    string _f = getTmpFileNm("conv");
                    bitmap.Save(_f, ImageFormat.Bmp);
                    return _f;
//                    Cv2.WaitKey();
                }
            }
        }
    }
}
 