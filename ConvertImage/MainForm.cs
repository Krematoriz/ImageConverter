/*
 * Программа для конвертации фото в 16-ный цвет
 * Для работы с программой загрузите в ресурсы проекта (через окно свойств) и поменяйте переменную sourceImage под ваше фото.
 * Для вывода на дисплей размером 50 на 50 исходное фото должно быть размером 50 на 50 пикселей
 * 
 * By Markus Romanov
 */
using ConvertImage.Properties;
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace ConvertImage
{
    public partial class MainForm : Form
    {
        //Загрузка изначальной фотографии (размер должен совпадать с размером дисплея)
        private Bitmap sourceImage = Resources.Romanov;
        //Глобальная переменная для трансформированной фотки
        private Bitmap convertedImage = null;
        //Ширина фото для тестового показа в программе
        private int imageWidth = 256;
        //Разрешение рисования тестовых фото
        private bool allowToDraw = false;
        //Порог яркости пикселя для его осветления на дисплее
        private int brightnessTreshold = 160; //От 0 до 255
        //Порог считывания цвета (чем ниже - тем светлее будут фото на дисплее)
        private int colorTreshold = 100; //От 0 до 255

        public MainForm()
        {
            InitializeComponent();
            //Конвертированные байты
            Byte[] pixelBytes = null;
            convertedImage = ConvertImage(sourceImage, out pixelBytes);
            allowToDraw = true;
            Invalidate();
            //Переменная для рассчета размера тестового фото
            int x = imageWidth / sourceImage.Width;

            //Установка размера формы под фото
            this.Size = new Size(sourceImage.Width * x * 2+10, sourceImage.Height * x * 2);

            //Размещение и программирование текстовой коробочки для вывода байтов
            RichTextBox TextBox = new RichTextBox();
            TextBox.Location = new Point(0, sourceImage.Height * x);
            TextBox.Size = new Size(Width - SystemInformation.VerticalScrollBarWidth, Height - (sourceImage.Height * x) - SystemInformation.HorizontalScrollBarHeight*2);
            TextBox.ReadOnly = true;
            TextBox.Text = StringFromByteArray(pixelBytes);
            Controls.Add(TextBox);
        }
        //Метод конвертации фото в 16-ти цветный формат
        private Bitmap ConvertImage(Bitmap SourceImage, out Byte[] Pixels)
        {
            Bitmap convertedImage = new Bitmap(SourceImage.Width, SourceImage.Height);
            Pixels = new Byte[SourceImage.Height * SourceImage.Width + SourceImage.Height];
            int counter = 0;
            int secCount = 0;

            for (int i = 0; i < SourceImage.Height; i++)
            {
                for (int a = 0; a < SourceImage.Width; a++)
                {
                    Color Pixel = SourceImage.GetPixel(a, i);

                    Byte[] aBArr = new Byte[1];
                    Byte[] rBArr = new Byte[1];
                    Byte[] gBArr = new Byte[1];
                    Byte[] bBArr = new Byte[1];

                    aBArr[0] = Pixel.A;
                    rBArr[0] = Pixel.R;
                    gBArr[0] = Pixel.G;
                    bBArr[0] = Pixel.B;
                    
                    BitArray rArr = new BitArray(rBArr);
                    BitArray gArr = new BitArray(gBArr);
                    BitArray bArr = new BitArray(bBArr);
                    BitArray newByte = new BitArray(8, false);

                    int iR = BitsToInt(rArr);
                    int iG = BitsToInt(gArr);
                    int iB = BitsToInt(bArr);



                    if (iR >= colorTreshold)
                    {
                        newByte[2] = true;
                        rArr[7] = true;
                    }
                    else
                        rArr[7] = false;

                    if (iG >= colorTreshold)
                    {
                        newByte[1] = true;
                        gArr[7] = true;
                    }
                    else
                        gArr[7] = false;

                    if (iB >= colorTreshold)
                    {
                        newByte[0] = true;
                        bArr[7] = true;
                    }
                    else
                        bArr[7] = false;

                    if (iR < colorTreshold)
                        iR = colorTreshold;
                    if (iG < colorTreshold)
                        iG = colorTreshold;
                    if (iB < colorTreshold)
                        iB = colorTreshold;

                    if ((iR+iG+iB)/3 >= brightnessTreshold)
                    {
                        newByte[3] = true;
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        if (!j.Equals(7) && !j.Equals(5) && !j.Equals(6))
                        {
                            rArr[j] = true;
                            gArr[j] = true;
                            bArr[j] = true;
                        }
                        else if (j.Equals(6))
                        {
                            rArr[j] = false;
                            gArr[j] = false;
                            bArr[j] = false;
                        }
                        else if (j.Equals(5))
                        {
                            rArr[j] = newByte[3];
                            gArr[j] = newByte[3];
                            bArr[j] = newByte[3];
                        }
                    }

                    if (!counter.Equals(0) && (counter % sourceImage.Height).Equals(0))
                    {
                        Pixels[counter + secCount] = Pixels[counter + secCount - 1];
                        Pixels[counter + secCount + 1] = BitsToByte(newByte);
                        secCount++;
                    }
                    else
                        Pixels[counter + secCount] = BitsToByte(newByte);

                    convertedImage.SetPixel(a, i, Color.FromArgb(255, BitsToByte(rArr), BitsToByte(gArr), BitsToByte(bArr)));
                    counter++;
                }
            }

            return convertedImage;
        }
        //Событие рисования фото в форме
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            int x = imageWidth / sourceImage.Width;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            if (allowToDraw)
            {
                e.Graphics.DrawImage(sourceImage, 0, 0, sourceImage.Width * x, sourceImage.Height * x);

                if (!convertedImage.Equals(null))
                    e.Graphics.DrawImage(convertedImage, sourceImage.Width * x, 0, convertedImage.Width * x, convertedImage.Height * x);
            }
        }
        //Конвертация BitArray в Byte
        private Byte BitsToByte(BitArray bits)

        {

            if (bits.Count != 8)

            {

                throw new ArgumentException("bits");

            }

            Byte[] bytes = new Byte[1];

            bits.CopyTo(bytes, 0);

            return bytes[0];

        }
        //Получение красивой строки из массива байтов
        private string StringFromByteArray(Byte[] Data)
        {

            string strFromBytes = "";

            for (int i = 0; i < Data.Length; i++)
            {
                if (i.Equals(Data.Length - 1))
                {
                    strFromBytes += Data[i].ToString("X2");
                }

                else
                {
                    strFromBytes += Data[i].ToString("X2") + ",";
                }
            }

            return strFromBytes;
        }
        //Перевод из массива битов в целочисленное число
        private int BitsToInt(BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }
    }
}
