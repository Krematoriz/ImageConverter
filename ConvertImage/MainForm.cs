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
using System.IO;
using System.Windows.Forms;

namespace ConvertImage
{
    public partial class MainForm : Form
    {
        //Загрузка изначальной фотографии (размер должен совпадать с размером дисплея)
        private Bitmap sourceImage = null;
        //Глобальная переменная для трансформированной фотки
        private Bitmap convertedImage = null;
        //Ширина фото для тестового показа в программе
        private int imageWidth = 256;
        //Разрешение рисования тестовых фото
        private bool allowToDraw = false;
        //Порог яркости пикселя для его осветления на дисплее
        private TrackBar brightnessTrackBar = new TrackBar(); //От 0 до 255
        //Порог считывания цвета (чем ниже - тем светлее будут фото на дисплее)
        private TrackBar colorTrackBar = new TrackBar(); //От 0 до 255
        //
        private OpenFileDialog openFile = new OpenFileDialog();
        //
        private Size buttonSize;
        private Size labelSize;
        private RichTextBox TextBox = new RichTextBox();

        public MainForm()
        {
            InitializeComponent();
            
            openFile.Multiselect = false;
            openFile.RestoreDirectory = true;
            openFile.Filter = "Image Files(*.PNG; *.JPG; *.BMP)| *.PNG; *.JPG; *.BMP | All files(*.*) | *.*";
            openFile.CheckFileExists = true;
            openFile.CheckPathExists = true;
            Button button = new Button();
            button.Click += new EventHandler(this.ButtonClick);
            using (Graphics cg = this.CreateGraphics())
            {
                SizeF size = cg.MeasureString("Open Image...", button.Font);
                button.Width = (int)size.Width+10;
                button.Text = "Open Image...";
                buttonSize = button.Size;
            }
            button.Location = new Point(0,0);
            Controls.Add(button);

            colorTrackBar.Minimum = 1;
            brightnessTrackBar.Minimum = 1;

            colorTrackBar.Maximum = 255;
            brightnessTrackBar.Maximum = 255;

            colorTrackBar.Value = 125;
            brightnessTrackBar.Value = 170;

            colorTrackBar.ValueChanged += new EventHandler(TrackBarValueChanged);
            brightnessTrackBar.ValueChanged += new EventHandler(TrackBarValueChanged);
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



                    if (iR >= colorTrackBar.Value)
                    {
                        newByte[2] = true;
                        rArr[7] = true;
                    }
                    else
                        rArr[7] = false;

                    if (iG >= colorTrackBar.Value)
                    {
                        newByte[1] = true;
                        gArr[7] = true;
                    }
                    else
                        gArr[7] = false;

                    if (iB >= colorTrackBar.Value)
                    {
                        newByte[0] = true;
                        bArr[7] = true;
                    }
                    else
                        bArr[7] = false;

                    if (iR < colorTrackBar.Value)
                        iR = colorTrackBar.Value;
                    if (iG < colorTrackBar.Value)
                        iG = colorTrackBar.Value;
                    if (iB < colorTrackBar.Value)
                        iB = colorTrackBar.Value;

                    if ((iR+iG+iB)/3 >= brightnessTrackBar.Value)
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
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            if (allowToDraw)
            {
                int x = imageWidth / sourceImage.Width;
                e.Graphics.DrawImage(sourceImage, 0, colorTrackBar.Location.Y + colorTrackBar.Height, sourceImage.Width * x, sourceImage.Height * x);

                if (!convertedImage.Equals(null))
                    e.Graphics.DrawImage(convertedImage, brightnessTrackBar.Location.X, brightnessTrackBar.Location.Y + brightnessTrackBar.Height, convertedImage.Width * x, convertedImage.Height * x);
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
        //Событие нажатия кнопки
        private void ButtonClick(object sender, EventArgs e)
        {
            if(openFile.ShowDialog().Equals(DialogResult.OK))
            {
                using (Image image = Image.FromStream(openFile.OpenFile()))
                {
                    //Загрузка и перевод фото в битмэп
                    sourceImage = new Bitmap(image);
                    int x = imageWidth / sourceImage.Width;

                    //Программирование строки размера фото
                    Label label = new Label();
                    label.Text = sourceImage.Width + " x " + sourceImage.Height;
                    using (Graphics cg = this.CreateGraphics())
                    {
                        SizeF size = cg.MeasureString(label.Text, label.Font);
                        label.Width = (int)size.Width + 10;
                        labelSize = label.Size;
                    }
                    label.Location = new Point(buttonSize.Width + 10,5);
                    Controls.Add(label);

                    //Программирование строки порога цвета
                    Label colorLabel = new Label();
                    colorLabel.Text = "Color Treshold";
                    colorLabel.Width = imageWidth;
                    colorLabel.Location = new Point(5, buttonSize.Height+5);
                    Controls.Add(colorLabel);

                    //Программирование трэкбара цвета
                    colorTrackBar.Width = colorLabel.Width;
                    colorTrackBar.Location = new Point(0, colorLabel.Location.Y+colorLabel.Height);
                    Controls.Add(colorTrackBar);
                    
                    //Программирование строки порога яркости
                    Label brightnessLabel = new Label();
                    brightnessLabel.Text = "Brightness Treshold";
                    brightnessLabel.Width = imageWidth;
                    brightnessLabel.Location = new Point(colorLabel.Width+5, buttonSize.Height+5);
                    Controls.Add(brightnessLabel);

                    //Программирование трэкбара яркости
                    brightnessTrackBar.Width = brightnessLabel.Width;
                    brightnessTrackBar.Location = new Point(brightnessLabel.Location.X, brightnessLabel.Location.Y + brightnessLabel.Height);
                    Controls.Add(brightnessTrackBar);

                    //Размещение и программирование текстовой коробочки для вывода байтов
                    TextBox.Location = new Point(0, colorTrackBar.Location.Y + sourceImage.Height * x + colorTrackBar.Height + 10);
                    TextBox.Size = new Size(Width - SystemInformation.VerticalScrollBarWidth, 150);
                    TextBox.ReadOnly = true;
                    Controls.Add(TextBox);

                    DisplayImage();
                }
            }
        }
        //Конвертация фото, получение байтов и вывод
        private void DisplayImage()
        {
            //Конвертированные байты
            Byte[] pixelBytes = null;
            convertedImage = ConvertImage(sourceImage, out pixelBytes);
            allowToDraw = true;
            Invalidate();

            TextBox.Text = StringFromByteArray(pixelBytes);
        }
        //
        private void TrackBarValueChanged(object sender, EventArgs e)
        {
            DisplayImage();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            MinimumSize = new Size(this.Width, this.Height);
            MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }
    }
}
