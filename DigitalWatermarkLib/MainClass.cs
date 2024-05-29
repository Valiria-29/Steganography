using DigitalWatermarkLib;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWatermarkLib
{
    public class MainClass
    {
        
        int Coeff;
        public void InsertDWM(string pathToContainer, string newPathToFullContainer, int coeffP)
        {
            Coeff = coeffP;
            //получение изображения контейнера и разложение его на составляющие
            var Container = new StegoBitmap(pathToContainer);
            //создание экземпляра класса для дальнейшего ДКП
            var DCT = new DCT(Container, Container.BlueColor);
            //разделение массива синей составляющей на сегменты 8х8 пикселей
            var SegmentList = DCT.Segmentation();
            //применение ДКП к каждому сегменту
            var SegmentListDCT = new List<double[,]>();
            foreach (var item in SegmentList)
            {
                SegmentListDCT.Add(DCT.DCTChance(item));
            }
            //получение двоичных данных ЦВЗ
            var DWM= new StegoBitmap("DWM/dwmMedium.bmp");
            var binaryDWM = DWM.ReadDWM(DWM);
            //создание экземпляра класса MethodKochAndZhao
            var method = new MethodKochAndZhao(binaryDWM, SegmentListDCT, Coeff);
            //выполнение внедрения ЦВЗ в контейнер
            method.SetDWM();
            //обратное ДКП
            var SegmentListIDCT = new List<double[,]>();
            foreach (var item in SegmentListDCT)
            {
                SegmentListIDCT.Add(DCT.IDCT(item));
            }
            var newArr = new double[Container.originalImgBitmap.Width, Container.originalImgBitmap.Height]; // новый массив значений
            //соединение сегментов
            DCT.Join(newArr, SegmentListIDCT);
            DCT.Normalize(newArr);
            var newContainer =  new StegoBitmap(Container.originalImgBitmap, newArr);
            var c = newContainer.BlueColor[3, 6];
            var b = newContainer.BlueColor[6, 3];
            //сохранение изображения с ЦВЗ
            if (File.Exists(newPathToFullContainer))
                File.Delete(newPathToFullContainer);
            newContainer.originalImgBitmap.Save(newPathToFullContainer, ImageFormat.Tiff);
        }

        public void ExtractionDWM (string PathToFullContainer, string newPathToDWM, int coeffP)
        {
            Coeff = coeffP;
            //получение изображения заполненного контейнера и разложение его на составляющие
            var fullContainer = new StegoBitmap(PathToFullContainer);
            var c = fullContainer.BlueColor[3, 6];
            var b = fullContainer.BlueColor[6, 3];
            //создание экземпляра класса для дальнейшего ДКП
            var DCT = new DCT(fullContainer, fullContainer.BlueColor);
            //разделение массива синей составляющей на сегменты 8х8 пикселей
            var SegmentList = DCT.Segmentation();
            //применение ДКП к каждому сегменту
            var SegmentListDCT = new List<double[,]>();
            foreach (var item in SegmentList)
            {
                SegmentListDCT.Add(DCT.DCTChance(item));
            }
            //создание экземпляра класса MethodKochAndZhao
            var method = new MethodKochAndZhao(SegmentListDCT, Coeff);
            //выполнение извлечения ЦВЗ из контейнера
            var extractedDWM =  method.GetDWM();
            var extrDWMBitmap = new Bitmap(extractedDWM.GetLength(0), extractedDWM.GetLength(1));
            for (int i = 0; i < extrDWMBitmap.Width; i++)
            {
                for (int j = 0; j < extrDWMBitmap.Height; j++)
                {
                    extrDWMBitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(extractedDWM[i, j], extractedDWM[i, j], extractedDWM[i, j]));
                }
            }
            //сохранение изображения  ЦВЗ
            if (File.Exists(newPathToDWM))
                File.Delete(newPathToDWM);
            extrDWMBitmap.Save(newPathToDWM, ImageFormat.Png);
        }
    }

}
