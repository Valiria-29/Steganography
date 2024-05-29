using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWatermarkLib
{
    public class DCT
    {
        public StegoBitmap originalImgBitmap;
        public byte[,] BlueColor; // байты голубого цвета
        public DCT(StegoBitmap bitmap, byte[,] blue)
        {
            originalImgBitmap = bitmap;
            BlueColor = blue;
        }
        // разбиение массива на сегменты
        public List<byte[,]> Segmentation()
        {
            var array = BlueColor;
            var segmentsList = new List<byte[,]>();
            int numSW = array.GetLength(0) / 8; // количество сегментов по горизонтали
            int numSH = array.GetLength(1) / 8; // количество сегментов по вертикали
            for (int i = 0; i < numSW; i++)
            {
                int firstWPoint = i * 8; //начало каждого сегмента по горизонтали
                int lastWPoint = firstWPoint + 8 - 1; //конец каждого сегмента по горизонтали
                for (int j = 0; j < numSH; j++)
                {
                    int firstHPoint = j * 8; //начало каждого сегмента по вертикали
                    int lastHPoint = firstHPoint + 8 - 1; //конец каждого сегмента по вертикали
                    segmentsList.Add(SegmBytes(array, firstWPoint, lastWPoint, firstHPoint, lastHPoint)); // добавляем в список элементы относительно сегментов
                }
            }
            return segmentsList;
        }

        // получение элементов сегмента
        private static byte[,] SegmBytes(byte[,] arr, int a, int b, int c, int d)
        {
            var sg = new byte[b - a + 1, d - c + 1];
            for (int i = a, x = 0; i <= b; i++, x++)
                for (int j = c, y = 0; j <= d; j++, y++)
                    sg[x, y] = arr[i, j];
            return sg;
        }
        // определение значений коэффициентов(кси в формуле 7.1) для прямого ДКП
        public double GetCoefficient(int argument)
        {
            if (argument == 0)
                return 1.0 / Math.Sqrt(2);
            else
                return 1;
        }
        // прямое дискретное косинусное преобразование
        public double[,] DCTChance(byte[,] segment)
        {
            int len = 8; // получаем размер сегмента
            double[,] segmentDCT = new double[len, len]; // новый массив после дискретного косинусного преобразования
            double temp;
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    temp = 0;
                    for (int x = 0; x < len; x++)
                    {
                        for (int y = 0; y < len; y++)
                            temp += segment[x, y] * Math.Cos(Math.PI * i * (2 * x + 1) / (2 * len)) * Math.Cos(Math.PI * j * (2 * y + 1) / (2 * len)); // вычисление спектральных коэффициентов ДКП для каждого сегмента (без умножения на значения коэффициентов для текущего значения аргумента) 
                    }
                    segmentDCT[i, j] = GetCoefficient(j) * GetCoefficient(i) * temp / Math.Sqrt(2 * len); // вычисление спектральных коэффициентов ДКП для каждого сегмента (домножаем на значения коэффициентов для текущего значения аргумента)
                }
            }
            return segmentDCT;
        }
        //обратное дискретное косинусное преобразование
        public double[,] IDCT(double[,] dct)
        {
            int len = dct.GetLength(0); // получаем размер сегмента ДКП
            double[,] result = new double[len, len]; // новый массив после обратного дискретного косинусного преобразования
            double temp;
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    temp = 0;
                    for (int x = 0; x < len; x++)
                    {
                        for (int y = 0; y < len; y++)
                            temp += GetCoefficient(x) * GetCoefficient(y) * dct[x, y] * Math.Cos(Math.PI * x * (2 * i + 1) / (2 * len)) * Math.Cos(Math.PI * y * (2 * j + 1) / (2 * len));
                    }
                    result[i, j] = (temp / (Math.Sqrt(2 * len)));
                }
            }
            return result;
        }
        // соединяем сегменты
        public void Join(double[,] array, List<double[,]> Idct)
        {
            var temp = Idct.ToArray();
            int numSW = array.GetLength(0) / 8; // количество сегментов по горизонтали
            int numSH = array.GetLength(1) / 8; // количество сегментов по вертикали
            int k = 0;
            for (int i = 0; i < numSW; i++)
            {
                int firstWPoint = i * 8; //начало каждого сегмента по горизонтали
                int lastWPoint = firstWPoint + 8 - 1; //конец каждого сегмента по горизонтали
                for (int j = 0; j < numSH; j++)
                {
                    int firstHPoint = j * 8;//начало каждого сегмента по вертикали
                    int lastHPoint = firstHPoint + 8 - 1;//конец каждого сегмента по вертикали
                    Insert(array, temp[k], firstWPoint, lastWPoint, firstHPoint, lastHPoint);
                    k++;
                }
            }
        }

        // вставка сегмент в массив
        public void Insert(double[,] arr, double[,] temp, int firstWPoint, int lastWPoint, int firstHPoint, int lastHPoint)
        {
            for (int i = firstWPoint, u = 0; i < lastWPoint + 1; i++, u++)
            {
                for (int j = firstHPoint, v = 0; j < lastHPoint + 1; j++, v++)
                {
                    arr[i, j] = temp[u, v];
                }
            }
        }
        public void Normalize(double[,] Idct)
        {
            double min = double.MaxValue, max = double.MinValue;
            for (int i = 0; i < Idct.GetLength(0); i++)
            {
                for (int j = 0; j < Idct.GetLength(1); j++)
                {
                    if (Idct[i, j] > max)
                        max = Idct[i, j]; // находим максиальный элемент
                    if (Idct[i, j] < min)
                        min = Idct[i, j];// находим минимальный элемент
                }
            }
            for (int i = 0; i < Idct.GetLength(0); i++)
            {
                for (int j = 0; j < Idct.GetLength(1); j++)
                    Idct[i, j] = (byte)(255 * (Idct[i, j] + Math.Abs(min)) / (max + Math.Abs(min))); // записываем результат нормировки в массив
            }
        }
    }
}
