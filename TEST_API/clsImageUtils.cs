using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TEST_API
{
    class clsImageUtils
    {
        /// <summary>
        /// change constract
        /// </summary>
        /// <param name="img"></param>
        /// <param name="contrast"></param>
        /// <returns></returns>
        public static Image chContrast(Image _img, float contrast)
        {
            if (contrast <= 0) return _img;
            Bitmap newImg = new Bitmap(_img.Width, _img.Height);
            Graphics g = Graphics.FromImage(newImg);
            try
            {
                //ColorMatrix
                float _s = (100f + contrast) / 100f;
                _s *= _s;
                float append = 0.5f * (1f - _s);
                System.Drawing.Imaging.ColorMatrix _cm =
                    new System.Drawing.Imaging.ColorMatrix(
                    new float[][] {
                    new float[] {_s, 0, 0, 0, 0},
                    new float[] {0, _s, 0, 0, 0},
                    new float[] {0, 0, _s, 0, 0},
                    new float[] {0, 0, 0, _s, 0},
                    new float[] {append, append, append, 0, 1}});

                //ImageAttributes
                System.Drawing.Imaging.ImageAttributes _ia =
                    new System.Drawing.Imaging.ImageAttributes();
                _ia.SetColorMatrix(_cm);

                //ImageAttributes
                g.DrawImage(_img,
                    new Rectangle(0, 0, _img.Width, _img.Height),
                    0, 0, _img.Width, _img.Height, GraphicsUnit.Pixel, _ia);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return _img;
            }
            finally
            {
                g.Dispose();
            }

            return newImg;
        }

        static public Bitmap AdjustSharpness(Bitmap srcImg, float level)
        {
            Bitmap dstImg = new Bitmap(srcImg);

            BitmapData srcDat = srcImg.LockBits(
                new Rectangle(0, 0, srcImg.Width, srcImg.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            byte[] srcBuf = new byte[srcImg.Width * srcImg.Height * 4];
            Marshal.Copy(srcDat.Scan0, srcBuf, 0, srcBuf.Length);

            BitmapData dstDat = dstImg.LockBits(
                new Rectangle(0, 0, dstImg.Width, dstImg.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            byte[] dstBuf = new byte[dstImg.Width * dstImg.Height * 4];
            Marshal.Copy(dstDat.Scan0, dstBuf, 0, dstBuf.Length);

            for (int i = 1; i < srcImg.Height - 1; i++)
            {

                int dy1 = (i - 1) * (srcImg.Width * 4);
                int dy = i * (srcImg.Width * 4);
                int dy2 = (i + 1) * (srcImg.Width * 4);

                for (int j = 1; j < srcImg.Width - 1; j++)
                {
                    int dx1 = (j - 1) * 4;
                    int dx = j * 4;
                    int dx2 = (j + 1) * 4;

                    for (int k = 0; k < 3; k++)
                    {
                        int value = (int)((float)srcBuf[dy + dx + k] * (1f + level * 4))
                                  - (int)((float)srcBuf[dy1 + dx + k] * level)
                                  - (int)((float)srcBuf[dy + dx1 + k] * level)
                                  - (int)((float)srcBuf[dy + dx2 + k] * level)
                                  - (int)((float)srcBuf[dy2 + dx + k] * level);
                        value = Math.Min(value, 255);
                        value = Math.Max(value, 0);

                        dstBuf[dy + dx + k] = (byte)value;
                    }
                }
            }
            Marshal.Copy(dstBuf, 0, dstDat.Scan0, dstBuf.Length);
            dstImg.UnlockBits(dstDat);
            srcImg.UnlockBits(srcDat);
            return dstImg;
        }
        public static Image chGrayscaleImage(Image img)
        {
            Bitmap ret = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(ret);

            System.Drawing.Imaging.ColorMatrix _cm =
                        new System.Drawing.Imaging.ColorMatrix(
                        new float[][] {
                    new float[] { 0.299F, 0.299F, 0.299F, 0, 0},
                    new float[] { 0.587F, 0.587F, 0.587F, 0, 0},
                    new float[] { 0.114F, 0.114F, 0.114F, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}});
            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();

            ia.SetColorMatrix(_cm);
            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height),
                        0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

            g.Dispose();
            ia.Dispose();
            return ret;
        }
    }
}
