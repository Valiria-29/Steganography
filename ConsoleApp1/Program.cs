using DigitalWatermarkLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mainclass = new MainClass();
            int coeff = 25;
            Stopwatch timer = new Stopwatch();
            for (int i = 0; i < 5; i++)
            {
                timer.Start();
                string temp = coeff.ToString();
                string newPathToFullContainer = "FullContainer/Nature1full" + temp + ".tiff";
                string newPathToInsertedDWM = "InsertedDWM/Nature12DWM" + temp + ".png";
                mainclass.InsertDWM("EmptyContainer/Nature1.jpg", newPathToFullContainer, coeff);
                mainclass.ExtractionDWM(newPathToFullContainer, newPathToInsertedDWM, coeff);
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds);
                timer.Reset();
                coeff += 25;
            }
            Console.ReadLine();
        }
    } 
}
