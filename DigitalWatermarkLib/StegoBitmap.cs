using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWatermarkLib
{
    public enum Color { Red, Green, Blue } //перечисление  возможных цветов 
    //класс отвечающий за получение RGB составляющих изображения и работу с ним
    public class StegoBitmap
    {
        public Bitmap originalImgBitmap; // Bitmap исходного файла 

        public string ImgSize { get; } // размер файла в пикселях
        public byte[,] RedColor { get; } // байты красного цвета
        public byte[,] GreenColor { get; } // байты зеленого цвета
        public byte[,] BlueColor { get; } // байты голубого цвета

        // конструктор получающий путь файла в качестве параметра
        public StegoBitmap(string path)
        {
            //originalImgBitmap = (Bitmap)Image.FromFile(path, false);
            originalImgBitmap = (Bitmap)Image.FromFile(path);
            ImgSize = originalImgBitmap.Height.ToString() + " x " + originalImgBitmap.Width.ToString();
            RedColor = ReadColor(originalImgBitmap, Color.Red);
            GreenColor = ReadColor(originalImgBitmap, Color.Green);
            BlueColor = ReadColor(originalImgBitmap, Color.Blue);
        }
        public StegoBitmap(Bitmap bmap, double[,] newArr)
        {
            originalImgBitmap = new Bitmap(bmap.Width, bmap.Height);
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    originalImgBitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(bmap.GetPixel(i, j).R, bmap.GetPixel(i, j).G, (byte)Math.Round(newArr[i, j])));
                }
            }
            RedColor = ReadColor(originalImgBitmap, Color.Red);
            GreenColor = ReadColor(originalImgBitmap, Color.Green);
            BlueColor = ReadColor(originalImgBitmap, Color.Blue);
        }
        //метод получающий r/g/b массивы изображения
        private byte[,] ReadColor(Bitmap bitmap, Color c)
        {
            var arr = new byte[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    if (c == Color.Red)
                        arr[i, j] = bitmap.GetPixel(i, j).R;
                    else if (c == Color.Green)
                        arr[i, j] = bitmap.GetPixel(i, j).G;
                    else if (c == Color.Blue)
                        arr[i, j] = bitmap.GetPixel(i, j).B;
                }
            }
            return arr;
        }
        //метод, получающий массив из 0 и 1 из исходного черно-белого изображения ЦВЗ
        public byte[] ReadDWM  (StegoBitmap bwm)
        {
            var binarydwm = bwm.BlueColor;
            for (var i = 0; i < binarydwm.GetLength(0); i++)
            {
                for (var j = 0; j < binarydwm.GetLength(1); j++)
                {
                    binarydwm[i, j] = (byte)(binarydwm[i, j] / 255);
                }
            }
            var cur = binarydwm.Cast<byte>().ToArray();
            //вставка метки о наличии ЦВЗ и указание его длины
            var sign = Convert.ToByte('V');
            var len = (binarydwm.Length);
            byte[] arrSign = new byte[8]; // размер массива соотвествует 8 битам в одном байте
            byte[] arrLen = new byte[16];
            int res = 0;
            int p = 1;
            while (sign > 0)
            {
                res += p * (sign % 2);
                sign >>= 1; // то же самое что n /= 2;
                arrSign[p] = (byte)(sign % 2);
                p++;
            }
            int res1 = 0;
            int p1 = 1;
            while (len > 0)
            {
                res1 += p1 * (len % 2);
                len >>= 1; // то же самое что n /= 2;
                arrLen[p1] = (byte)(len % 2);
                p1++;
            }
            Array.Reverse(arrSign);
            Array.Reverse(arrLen);
            var finishBinDWM = new byte[8+16+cur.Length];
            arrSign.CopyTo(finishBinDWM, 0);
            arrLen.CopyTo(finishBinDWM, 8);
            cur.CopyTo(finishBinDWM, 23);
            return finishBinDWM;
        }
    }
}

