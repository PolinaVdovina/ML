using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace obrRasprOsh
{
    public partial class Form1 : Form
    {
        int m = 20;  //количество нейронов скрытого слоя
        int n = 10000;  //размер входного вектора
        int p = 5;   //количество нейронов выходного слоя
        int k;
        double a = .5;  //альфа
        int[] X;   //входной вектор
        double[,] W;   //веса перед срытым слоем
        double[,] V;   //веса перед выходным слоем
        double[] Y;  //вектор выхода сети
        double[] Yc;  //вектор выхода скрытого слоя
        int N = 20;  //количество образов одного вида
        int[] d;     //ожидаемый выход нейронов сети
        char[] letters;  //вспомогательный массив из распознаваемых букв 

        Random rand = new Random();
        private Point start;
        private bool drawing = false;
        private Bitmap bm = new Bitmap(100, 100);
        private Bitmap bm2 = new Bitmap(100, 100);


        public Form1()
        {
            InitializeComponent();
            k = p;
            X = new int[n];
            W = new double[n, m]; 
            V = new double[m, p];
            Y = new double[k];
            Yc = new double[m];
            d = new int[k];
            letters = new char[p];      
        }

        private void enterX(Bitmap b)
        {
            Color black = Color.FromArgb(255, 0, 0, 0);
            int i = 0;
            for (int x = 0; x < b.Height; x++)
                for (int y = 0; y < b.Width; y++)
                {
                    if (b.GetPixel(x, y) == black) X[i] = 1; else X[i] = 0;
                    i++;
                }
        }   //формирование входного вектора Х

        private void Randomization()
        {
            for (int i = 0; i < n; i++)                 //матрица W 
                for (int j = 0; j < m; j++)
                    W[i, j] = (rand.Next(7) - 3) / 10.0;

            for (int i = 0; i < m; i++)                 //матрица V 
                for (int j = 0; j < p; j++)
                    V[i, j] = (rand.Next(7) - 3) / 10.0;
        }  //инициализация матриц весов случайными числами

        private void Result()
        {
            double[] S1 = new double[m];  //суммы нейронов скрытого слоя
            double[] S2 = new double[p];  //суммы нейронов выходного слоя

            for (int i = 0; i < m; i++)  //обнуление промежуточных сумм
                S1[i] = 0;
            for (int i = 0; i < p; i++)
                S2[i] = 0;
            

            /*............вычисление выходного вектора*/


            for (int i = 0; i < m; i++)                 //вычисление сумм нейронов скрытого слоя
                for (int j = 0; j < n; j++)
                    if (X[j] != 0) S1[i] = S1[i] + X[j] * W[j, i];

            for (int i = 0; i < m; i++)   //выход скрытого слоя
                Yc[i] = 1d / (1d + Math.Pow(Math.E, -a * S1[i]));

            for (int i = 0; i < p; i++)                 //вычисление сумм нейронов выходного слоя
                for (int j = 0; j < m; j++)
                    S2[i] = S2[i] + Yc[j] * V[j, i];

            for (int i = 0; i < k; i++)   //выход выходного слоя
                Y[i] = 1d / (1d + Math.Pow(Math.E, -a * S2[i]));

            /*....................................*/
        }  //вычисление выходного вектора

        private void Train()
        {
            double[] S = new double[m];   //сумма для ошибки скрытого слоя

            for (int i = 0; i < m; i++)  //обнуление промежуточной суммы
                S[i] = 0;

            /*..........изменение весов...............*/

            for (int i = 0; i < p; i++)
            {
                double sigma = (Y[i] - d[i]) * Y[i] * (1d - Y[i]);  //ошибка на выходном слое
                for (int j = 0; j < m; j++)
                {
                    V[j, i] = V[j, i] - a * sigma * Yc[j];
                    S[j] = S[j] + sigma * V[j, i];
                }
            } //вычисление новых весов на выходном слое

            for (int i = 0; i < m; i++)                 //вычисление новых весов на скрытом слое
                for (int j = 0; j < n; j++)
                    if (X[j] != 0)
                        W[j, i] = W[j, i] - a * (S[i] * Yc[i] * (1d - Yc[i]) * X[j]);

            /*........................................*/
        }   //изменение весов

        private void set_dk(int result)
        {
            for (int i = 0; i < k; i++)
            {
                if (i == result) d[i] = 1;
                else d[i] = 0;
            }
        }  //ожидаемый выход каждого нейрона

        private void button1_Click(object sender, EventArgs e)
        {
            double eps = 0;               //ошибка сети (эпсилон)
            bool truth;                   //флаг для проверки размера ошибки на каждом образе
            int epochs = 0;               //счетчик эпох
            int h;

            Randomization();

            string[] trSet = new string[k * N];  //массив имен образов обучающей выборки
            for (int x = 0; x < k; x++)
                for (int y = 0; y < N; y++)
                    trSet[(x * N) + y] = letters[x] + Convert.ToString(y);

            do   //начало новой эпохи
            {
                trSet = trSet.OrderBy(x => rand.Next()).ToArray();  //перемешивание массива в случайном порядке
                truth = true;  //флаг, фиксирующий превышение допустимой ошибки
                epochs++;  //счетчик эпох
                h = 0;  //счетчик количества ошибочно распознанных образов в эпоху

                for (int t = 0; t < k * N; t++)  //подача обучающей выборки
                {
                    Bitmap bmp = new Bitmap(Image.FromFile(@"Pictures\" + trSet[t] + ".png"));  //образ из папки

                    char letter = trSet[t][0];  //ожидаемый результат (буква)
                    int result = -1;  //какой нейрон должен выдать единицу
                    switch (letter)   
                    {
                        case 'A': { result = 0; break; }
                        case 'B': { result = 1; break; }
                        case 'C': { result = 2; break; }
                        case 'D': { result = 3; break; }
                        case 'E': { result = 4; break; }
                    }

                    set_dk(result);
                    enterX(bmp);  //формирование входного вектора
                    Result();  //вычисление выходного вектора

                    for (int i = 0; i < k; i++) //сумма ошибок нейронов внешнего слоя  
                        eps = eps + Math.Pow(Y[i] - d[i], 2);
                    eps = 0.5 * eps;   //эпсилон
                    if (eps > 0.01) { truth = false; h++; }  //удовлетворяет ли ошибка для одного образа условию

                    Train();
                }
                if (checkBox1.Checked && (epochs % 10 == 0)) a = a * 0.99;
            }
            while (!truth && epochs <= 300);  //пока большая ошибка или количество эпох не превысило 300
            label6.Text = "Количество эпох: " + epochs.ToString();
            MessageBox.Show("конец "+eps);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Randomization();

            


        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                bmp = Cut(bmp);
                enterX(bmp);   //считывание входных данных
                Result();   //вычисление выходного вектора

                int f = 0; int maxi = 0;
                for (int i = 0; i < p; i++)
                {
                    if (Y[i] > 0.8) { f++; if (f == 1) maxi = i; } 
                }

                if (f == 1) textBox4.Text = "Это буква " + letters[maxi];
                else textBox4.Text = "Неизвестная буква";

                progressBar1.Value = Convert.ToInt32(Math.Round(Y[0], 2) * 100);
                progressBar2.Value = Convert.ToInt32(Math.Round(Y[1], 2) * 100);
                progressBar3.Value = Convert.ToInt32(Math.Round(Y[2], 2) * 100);
                progressBar4.Value = Convert.ToInt32(Math.Round(Y[3], 2) * 100);
                progressBar5.Value = Convert.ToInt32(Math.Round(Y[4], 2) * 100);    
            }
            catch
            {
                MessageBox.Show("Отсутствует рисунок");
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            var finish = new Point(e.X, e.Y);
            var g = Graphics.FromImage(bm);
            var pen = new Pen(Color.Black, 1f);
            g.DrawLine(pen, start, finish);
            g.Save();
            drawing = false;
            g.Dispose();
            pictureBox1.Invalidate();
        }  //функции

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            start = new Point(e.X, e.Y);
            Bitmap orig = bm;
            drawing = true;
        }  //для рисования

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!drawing) return;
            start = new Point(e.X, e.Y);
            var finish = new Point(e.X, e.Y);
            Bitmap bm2 = new Bitmap(bm);
            pictureBox1.Image = bm2;
            var g = Graphics.FromImage(bm);
            var pen = new Pen(Color.Black, 2f);
            g.DrawEllipse(pen, finish.X, finish.Y, 2, 2);
            g.Dispose();
            pictureBox1.Invalidate();
        }  //на pictureBox

        private void button4_Click(object sender, EventArgs e)
        {
            Bitmap bb = new Bitmap(100, 100);
            pictureBox1.Image = bb;
            bm = bb; bm2 = bb;
        }  //очистка области рисования

        private Bitmap Cut(Bitmap b)
        {
            Color black = Color.FromArgb(255, 0, 0, 0);
            bool ok = false;
            int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            for (int x = 0; x < b.Height; x++)
            {
                for (int y = 0; y < b.Width; y++)
                {
                    if (b.GetPixel(x, y) == black)
                    {
                        x1 = x;
                        ok = true;
                        break;
                    }
                }
                if (ok) break;
            }
            ok = false;

            for (int x = x1 + 1; x < b.Height; x++)
            {
                ok = false;
                for (int y = 0; y < b.Width; y++)
                    if (b.GetPixel(x, y) == black) { ok = true; break; }
                if (!ok)
                {
                    x2 = x;
                    break;
                }
            }
            ok = false;

            for (int y = 0; y < b.Width; y++)
            {
                for (int x = 0; x < b.Height; x++)
                {
                    if (b.GetPixel(x, y) == black)
                    {
                        y1 = y;
                        ok = true;
                        break;
                    }
                }
                if (ok) break;
            }
            ok = false;

            for (int y = y1 + 1; y < b.Width; y++)
            {
                ok = false;
                for (int x = 0; x < b.Height; x++)
                    if (b.GetPixel(x, y) == black) { ok = true; break; }
                if (!ok)
                {
                    y2 = y;
                    break;
                }
            }

            Rectangle cropRect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            Bitmap src = b;
            Bitmap target = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, 100, 100), cropRect, GraphicsUnit.Pixel);
            }

            return target;
        }   //обрезка рисунка

        private void Form1_Load(object sender, EventArgs e)
        {
            letters[0] = 'A';
            letters[1] = 'B';
            letters[2] = 'C';
            letters[3] = 'D';
            letters[4] = 'E';
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int result = -1;
            if (radioButton1.Checked) result = 0;
            else if (radioButton2.Checked) result = 1;
            else if (radioButton3.Checked) result = 2;
            else if (radioButton4.Checked) result = 3;
            else if (radioButton5.Checked) result = 4;

            set_dk(result);
            Train();
        }    //доучить

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                a = Convert.ToDouble(textBox1.Text);
                if (/*a > 1 || */a <= 0) { MessageBox.Show("Недопустимое значение альфа"); textBox1.Text = " "; }
            }
            catch
            {
                MessageBox.Show("Недопустимое значение");
            };
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                n = Convert.ToInt32(textBox2.Text);
                if (n <= 0) { MessageBox.Show("Недопустимое количество нейронов"); textBox2.Text = " "; }
            }
            catch
            {
                MessageBox.Show("Недопустимое значение");
            };
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.StreamWriter srW = new System.IO.StreamWriter("fileW.txt");
                System.IO.StreamWriter srV = new System.IO.StreamWriter("fileV.txt");
                for (int i = 0; i < n; i++)                 //матрица W 
                    for (int j = 0; j < m; j++)
                        srW.Write(W[i, j] + " ");
                for (int i = 0; i < m; i++)                 //матрица V 
                    for (int j = 0; j < p; j++)
                        srV.Write(V[i,j] + " ");
                srW.Close();
                srV.Close();
                MessageBox.Show("Успешно");
            }
            catch
            {
                MessageBox.Show("Не удалось выполнить запись в файл");
            }; 
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.StreamReader srW = new System.IO.StreamReader("fileW.txt");
                System.IO.StreamReader srV = new System.IO.StreamReader("fileV.txt");
                String[] masW = srW.ReadToEnd().Split(' ');
                String[] masV = srV.ReadToEnd().Split(' ');
                for (int i = 0; i < n; i++)                 //матрица W 
                    for (int j = 0; j < m; j++)
                        W[i, j] = Convert.ToDouble(masW[i * m + j]); 
                for (int i = 0; i < m; i++)                 //матрица V 
                    for (int j = 0; j < p; j++)
                        V[i, j] = Convert.ToDouble(masV[i * p + j]);
 
                srW.Close();
                srV.Close();
                MessageBox.Show("Успешно");
            }
            catch
            {
                MessageBox.Show("Не удалось выполнить загрузку");
            };       
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int h = 0;
            string[] trSet = new string[k * 6];
            for (int x = 0; x < k; x++)
                for (int y = 0; y < 6; y++)
                    trSet[(x * 6) + y] = letters[x] + Convert.ToString(y);
            for (int t = 0; t < k * 6; t++)
            {
                Bitmap bmp = new Bitmap(Image.FromFile(@"test\" + trSet[t] + ".png"));
                enterX(bmp);  
                Result();   

                int f = 0; int maxi = 0;
                for (int i = 0; i < p; i++)
                {
                    if (Y[i] > 0.8) { f++; if (f == 1) maxi = i; }
                }

                if (!(f == 1 && trSet[t][0] == letters[maxi])) h++;

            }
            MessageBox.Show("" + (h / 30.0));
        }
    }
}
