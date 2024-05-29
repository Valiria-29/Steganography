using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWatermarkLib
{
    public class MethodKochAndZhao
    {
        public Point p1 = new Point(6, 3);
        public Point p2 = new Point(3, 6);

        public byte[] DWM;
        public List<double[,]> DCT;
        public int coefDif;

        public MethodKochAndZhao(byte[] dmw, List<double[,]> dct, int coef) 
        {
            DWM = dmw;
            DCT = dct;
            coefDif = coef;
        }
        public MethodKochAndZhao(List<double[,]> dct, int coef)
        {
            DCT = dct;
            coefDif = coef;
        }

        public  void SetDWM()
        {
            List<int> freePos = new List<int>(); // свободные позиции в соответствии с размером списка коэффициентов ДКП
            for (int i = 0; i < DCT.Count; i++)
                freePos.Add(i);

            for (int i = 0; i < DWM.Length; i++)
            {
                    int currentBit = DWM[i];
                    int pos = freePos[0]; // позиция
                    freePos.RemoveAt(0);
                    // берем значения коэффициентов ДКП по модулю
                    double AbsP1 = Math.Abs(DCT[pos][p1.X, p1.Y]);
                    double AbsP2 = Math.Abs(DCT[pos][p2.X, p2.Y]);
                    int z1 = 1, z2 = 1; // переменные для сохранения знака первичных значений коэффициентов ДКП по модулю
                    if (DCT[pos][p1.X, p1.Y] < 0)
                        z1 = -1;
                    if (DCT[pos][p2.X, p2.Y] < 0)
                        z2 = -1;
                    if (currentBit==1) //для передачи бита "1" стремяться, чтобы разница абсолютных значений коэффициентов ДКП была меньше по сравнению с некоторой отрицательной величиной
                    {
                        if (AbsP1 - AbsP2 >= -coefDif)
                            AbsP2 = coefDif + AbsP1 + 1;
                    }
                    else //для передачи бита "0" стремятся, чтобы разница абсолютных значений коэффициентов ДКП превышала некоторую положительную величину
                    {
                        if (AbsP1 - AbsP2 <= coefDif)
                            AbsP1 = coefDif + AbsP2 + 1;
                    }
                    // присваиваем коэффициентам ДКП новые значения
                    DCT[pos][p1.X, p1.Y] = z1 * AbsP1;
                    DCT[pos][p2.X, p2.Y] = z2 * AbsP2;
            }
        }
        public int[,] GetDWM()
        {
            List<int> freePos = new List<int>(); // свободные позиции в соответствии с размером списка коэффициентов ДКП
            for (int i = 0; i < DCT.Count; i++)
                freePos.Add(i);
            //выделяем метку присутствия сообщения в контейнере
            var MarkBits = new byte[8]; 
            //получаем ее двоичное значение
            for (int j = 0; j < 8; j++)
            {
                int pos = freePos[0]; // позиция 
                freePos.RemoveAt(0);
                double AbsPoint1 = Math.Abs(DCT[pos][p1.X, p1.Y]);
                double AbsPoint2 = Math.Abs(DCT[pos][p2.X, p2.Y]);
                // бит выделяем в соответсвии с тем, какое абслютное значение больше
                if (AbsPoint1 > AbsPoint2)
                    MarkBits[j] = 0;
                else if (AbsPoint1 < AbsPoint2)
                    MarkBits[j] = 1;
            }
            //переводим ее в двоичное число
            Array.Reverse(MarkBits);
            int Markvalue = 0;
            for (int i = 7; i >=0; i--)
            {
                var temp = (byte) MarkBits[i] *  (byte)Math.Pow(2, i);
                Markvalue += temp;
            }
            if (Markvalue != Convert.ToByte('V'))
            {
                throw new Exception("сообщение отсутствует!");
            }
            //выделяем длину сообщения в контейнере
            var LenBits = new byte[16];
            //получаем ее двоичное значение
            for (int j = 0; j < 15; j++)
            {
                int pos = freePos[0]; // позиция 
                freePos.RemoveAt(0);
                double AbsPoint1 = Math.Abs(DCT[pos][p1.X, p1.Y]);
                double AbsPoint2 = Math.Abs(DCT[pos][p2.X, p2.Y]);
                // бит выделяем в соответсвии с тем, какое абслютное значение больше
                if (AbsPoint1 > AbsPoint2)
                    LenBits[j] = 0;
                else if (AbsPoint1 < AbsPoint2)
                    LenBits[j] = 1;
            }
            //переводим ее в двоичное число
            Array.Reverse(LenBits);
            int  Lenvalue = 0;
            for (int i = 15; i >= 0; i--)
            {
                var temp = (byte)LenBits[i] * (short)Math.Pow(2, i);
                Lenvalue += temp;
            }
            //выделяем биты скрываемого сообщения
            int[,] extractionDWM = new int[(int)Math.Sqrt(Lenvalue), (int)Math.Sqrt(Lenvalue)];
            for (int i=0;i < extractionDWM.GetLength(0);i++)
            {
                for (int j=0;j < extractionDWM.GetLength(1);j++)
                {
                    int pos = freePos[0]; // позиция 
                    freePos.RemoveAt(0);
                    double AbsPoint1 = Math.Abs(DCT[pos][p1.X, p1.Y]);
                    double AbsPoint2 = Math.Abs(DCT[pos][p2.X, p2.Y]);
                    // бит выделяем в соответсвии с тем, какое абслютное значение больше
                    if (AbsPoint1 > AbsPoint2)
                        extractionDWM[i,j] = 0;
                    else if (AbsPoint1 < AbsPoint2)
                        extractionDWM[i,j] = 1 *225;
                }
            }
            return extractionDWM;
        }
    }
}
