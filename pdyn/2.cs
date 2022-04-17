using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pdyn
{
    
    public class Library
    {

        public List<Man> People = new List<Man>();
        public List<Border> Borders = new List<Border>();
        public double ChannelWidth = 520;
        public double ChannelLength =200;
        public double scale = 50; // pixels/m (масштаб)
        public double draw_minmanradius = 15; // минимальный радиус человека при отрисовке 
        public double draw_meshstep = 10; // линии сетки при отрисовке
       // public double f=28;//количество человек
        public double CurrentTime = 0;
        public static double AngleStep = 0.262;
        public static int MaxTryNum = 6; //90 градусов
        public static int MaxTryNum2 = 10;
        public static int HashN=3500;
      //  public int num_train = 0;
        public int peoplenum = 0;
        public int peoples = 0;
        public int peoples2 = 0;
        public static double HashDT=0.001;
        public double Cellsize = 1;
        public static int[] CellCount = {2000,1000};
        public double destination_x, destinationy_x;
        public double destination_y, destinationy_y;
        public int comfort_p;
        public static int check_insidepoints = 5;
        public static int check_anglesteps = 30;
        public int expeople1 = 0;
        public int expeople2 = 0;
        public double height = 1.8;//185 - 180 - 175 -170 - 165   // xxx
        public double co = 3; //3, 2, 1.5 // коэффициент зависимости длины шага от роста


        public SortedList<double, Event> Queue = new SortedList<double, Event>(new DuplicateKeyComparer<double>());

        public SortedList<double, Event> [] hTable = new SortedList<double, Event>[HashN];

        public Cell[,] cells = new Cell[CellCount[0], CellCount[1]];

        public Bitmap bmpmask;// = new Bitmap("hallprepared.bmp");

        public Bitmap dop_bmpmask;// = new Bitmap("map1.bmp");
        public StreamReader studentsXY;// = new StreamReader("Razmeshenie.txt");
        public StreamWriter ras = new StreamWriter("ras.txt", true);
        public StreamWriter rightpeop = new StreamWriter("right.txt", true);
        //public StreamWriter comfort_people = new StreamWriter("comfort.txt", true);


        public class Cell
        {
            public Cell()
            {
                People.Clear();
            }
            public List<Man> People = new List<Man>();

        }





        public class Event
        {
            public int type;  
            public Man m;
            public double t;
            public Border b;
        }
        public int HashF(double t)
        {   
            return ((int)Math.Truncate(t / HashDT)) % HashN;
        }




        public class Man
        {
            public double[] x = new double[2];
            public double[] v = new double[2];// скорость//
            public int i0, j0;// первоначальные и предыдущие ячейки человека
            public double l; //размер шага//
            public double[] d = new double[2]; //направление//
            public double[] dw = new double[2]; //направление//
            public double f; // частота шага
            public double r;//???
            public double num_train;
            public int Type;
            public int direction;//1 - направо - выход жёлтый, 2 - налево - выход красный
            //public int comfort;
            public Color color;
            public bool area1 = true; //2022
            public bool area2 = true; //2022

            public int DoStep(Library problem)
            {
                
                double[] nx = new double[2];
                double[] dest = new double[2];//направление для точки притяжения
              
                if (1 == 0) // direction == 1, if 2 flows
                {
                    dest[0] = problem.destinationy_x; // 475; //11;//
                    dest[1] = problem.destinationy_y;// ; //7.5;//
                }

                //if (direction == 2)
                //{
                dest[0] = problem.destination_x; 
                dest[1] = problem.destination_y;
                //}

                double dlin = 0;
                double delcolor = 120;// изменить на 100 //120 -- 2020

                Color c = problem.GetMaskColor(x[0], x[1]);

                if (c.G > 250 && c.R > 250 && c.B > 250)// при условии, что фон белый - идем к точке притяжения
                {
                    dw[0] = dest[0] - x[0]; // -(direction - 1.5) * 2; - для 2 потоков
                    dw[1] = dest[1] - x[1]; // 0 - для 2 потоков
                }
                else if (c.G > 180 && c.R > 180 && c.B < 5)// 2022 - недостаточно красный фон, проходим дальше
                {
                    dw[0] = dest[0] - x[0]; // -(direction - 1.5) * 2; - для 2 потоков
                    dw[1] = dest[1] - x[1]; // 0 - для 2 потоков
                }
                else
                {
                    if (direction == 2) // == 1 для 2 потоков
                    {
                        if (c.G < 10 && c.R > 250 && c.B < 10)// красный => не тот выход, идём дальше
                        { dw[0] = dest[0] - x[0]; dw[1] = dest[1] - x[1]; }
                        else
                        {
                            dw[0] = c.B - delcolor;//направление по оси х изменить на 100  //для голубого - 0 120 240
                            dw[1] = c.G - delcolor;//направление по оси у изменить на 100
                        }
                    }
                    if (1 == 0) //(direction == 2)
                    {
                        if (c.G > 250 && c.R > 250 && c.B < 10)// жёлтый => не тот выход, идём дальше
                        { dw[0] = dest[0] - x[0]; dw[1] = dest[1] - x[1]; }
                        else
                        {
                            dw[0] = -c.B + delcolor;//направление по оси х изменить на 100
                            dw[1] = -c.G + delcolor;//направление по оси у изменить на 100
                        }
                    }
                    

                    //MessageBox.Show(x[0] + " " + x[1] + " " + dw[0] + " " + dw[1] + " " + c.R + " " + c.G + " " + c.B + " ");
                }

                for (int i = 0; i < 2; i++)
                {
                    //dw[i] = dest[i] - x[i];
                    dlin += dw[i] * dw[i];
                }

                for (int i = 0; i < 2; i++)
                {
                    dw[i] = dw[i] / Math.Sqrt(dlin);
                }

                for (int i = 0; i < 2; i++)
                {
                    nx[i] = x[i] + l * dw[i];
                    //nx[i] = x[i] + l1 * dw[i];
                }

                for (int i = 0; i < 2; i++)
                {
                    d[i] = dw[i];
                }

                int can = 1;
               // problem.comfort_p = 0;
               // comfort = 0;
                int trynum2 = 0;
                double alpha = 0.9;
                double stepkoef = 1;

                do
                {
                    int trynum = 0;
                    double angle = 0;
                    Random rnd = new Random();
                    int signum = rnd.Next(1, 2);
                    do
                    {
                        can = 1;
                       

                        if (problem.CanGo(nx[0], nx[1], r) == 0) { can = 0; };


                        if (can == 1)
                        {

                            int ibegin = Math.Max(0, i0 - 1);
                            int jbegin = Math.Max(0, j0 - 1);
                            int iend = Math.Min(CellCount[0] - 1, i0 + 1);
                            int jend = Math.Min(CellCount[1] - 1, j0 + 1);
                            double del = 0;//

                            for (int iii = ibegin; iii <= iend; iii++)
                                for (int jjj = jbegin; jjj <= jend; jjj++)
                                    foreach (Man m in problem.cells[iii, jjj].People)
                                        if (m != this)
                                        {

                                            //if ((m != this) && (Math.Pow(m.x[0], 2) + Math.Pow(x[0], 2) < (2 * m.l) * (2 * m.l)) && (Math.Pow(m.x[1], 2) + Math.Pow(x[1], 2) < (2 * m.l) * (2 * m.l)))//разделила на окружности, чтобы не идти по всем
                                            //{
                                            if (Math.Pow(nx[0] - m.x[0], 2) + Math.Pow(nx[1] - m.x[1], 2) <= (m.r + r) * (m.r + r) + del)
                                            { can = 0;  }
                                            //}

                                        }
                        }

                        if (can == 0)
                        {
                            double sig = 0;
                            angle += AngleStep;
                            if (signum % 2 == 0){ sig = 1; signum++; }
                            else{ sig = -1; signum++; }

                            d[0] = Math.Cos(sig * angle) * dw[0] + Math.Sin(sig * angle) * dw[1];
                            d[1] = Math.Cos(sig * angle) * dw[1] - Math.Sin(sig * angle) * dw[0];

                            for (int i = 0; i < 2; i++)
                            {
                                nx[i] = x[i] + l * stepkoef * d[i];
                                //nx[i] = x[i] + l1 * d[i];
                            }

                            trynum++;
                            // comfort = +1;
                        }

                       
                    } while (can == 0 && trynum < MaxTryNum);

                    stepkoef *= alpha;
                    trynum2++; 
                   // comfort = +1; 

                } while (can == 0 && trynum2 < MaxTryNum2 && l * stepkoef > l / 5);


                if (can == 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        x[i] += l * stepkoef * d[i];
                        // x[i] += l1 * d[i];
                    }
                    problem.PutintoCell(this);
                    
                }
                //problem.comfort_p = comfort;

                return 0;
            }


        }


        public void LogWrite(string s)
        {
            StreamWriter log = new StreamWriter("log.txt", true);
            DateTime cur = DateTime.Now;

            log.WriteLine(cur.ToLongDateString()+ cur.ToLongTimeString()+": " + s);
            log.Flush();
            log.Close();

        }

    public Color GetMaskColor(double x, double y)
        {
            double maskscale = scale;

            int cx = (int)Math.Truncate(x * maskscale);
            int cy = (int)Math.Truncate(y * maskscale);


            if (cx <= 0 || cy <= 0 || cx >= bmpmask.Width || cy >= bmpmask.Height) return Color.Black;
            else return bmpmask.GetPixel(cx, cy);

        }
        //public double GetDw(double x, double y)
        //{

//        }


        public int CanGo(double x, double y, double R)
        {
           
            //int maskcan = 1; // нужно ли мне понимать почему именно я не могу сходить? от  этого завися же мои действия: если я сзади цепляюсь, то нужно отклоняться в один угол, спереди - в другой
            double x1, y1;
            int i,j;

            for (j = 0; j <= check_insidepoints; j++)// смотрим на цвет точки и внутри окружности
            {
                for (i = 0; i < check_anglesteps; i++)
                {
                    x1 = j * R / (check_insidepoints) * Math.Cos(i * (2*Math.PI / check_anglesteps)) + x;
                    y1 = j * R / (check_insidepoints) * Math.Sin(i * (2 * Math.PI / check_anglesteps)) + y;

                    Color c = GetMaskColor(x1, y1);
                    if (c.G < 15 && c.R < 110 && c.B < 15) return 0; //шаг сделать  нельзя

                }

            }
            return 1;
        }



        public int ExitCheck(double x, double y, double R)
        {
            double x1, y1;
            int i = 0, j = 0;
            //double r = R;
            for (j = 0; j <= check_insidepoints; j++)// смотрим на цвет точки и внутри окружности
            {
                for (i = 0; i < check_anglesteps; i++)
                {
                    x1 = j * R / (check_insidepoints) * Math.Cos(2 * Math.PI / check_anglesteps) + x;
                    y1 = j * R / (check_insidepoints) * Math.Sin(2 * Math.PI / check_anglesteps) + y;

                    Color c = GetMaskColor(x1, y1);

                    if (c.R > 240 && c.B < 5 && c.G < 5) return 1;// на границе (красная)

                }

            }
            return 0;
        }
        public int ExitCheckArea1(double x, double y, double R) //2022
        {
            double x1, y1;
            int i = 0, j = 0;
            //double r = R;
            for (j = 0; j <= check_insidepoints; j++)// смотрим на цвет точки и внутри окружности
            {
                for (i = 0; i < check_anglesteps; i++)
                {
                    x1 = j * R / (check_insidepoints) * Math.Cos(2 * Math.PI / check_anglesteps) + x;
                    y1 = j * R / (check_insidepoints) * Math.Sin(2 * Math.PI / check_anglesteps) + y;

                    Color c = GetMaskColor(x1, y1);

                    if (c.R < 220 && c.B < 5 && c.G < 220 && c.R > 180 && c.G > 180) return 1;// на границе (красная)

                }

            }
            return 0;
        }

        public int ExitCheckArea2(double x, double y, double R) //2022
        {
            double x1, y1;
            int i = 0, j = 0;
            //double r = R;
            for (j = 0; j <= check_insidepoints; j++)// смотрим на цвет точки и внутри окружности
            {
                for (i = 0; i < check_anglesteps; i++)
                {
                    x1 = j * R / (check_insidepoints) * Math.Cos(2 * Math.PI / check_anglesteps) + x;
                    y1 = j * R / (check_insidepoints) * Math.Sin(2 * Math.PI / check_anglesteps) + y;

                    Color c = GetMaskColor(x1, y1);

                    if (c.R > 220 && c.B < 5 && c.G > 220) return 1;// на границе (красная)

                }

            }
            return 0;
        }
        public int ExitCheck_yellow(double x, double y, double R)// для левых людей, направление==2
        {
            double x1, y1;
            int i = 0, j = 0;
            //double r = R;
            for (j = 0; j <= check_insidepoints; j++)// смотрим на цвет точки и внутри окружности
            {
                for (i = 0; i < check_anglesteps; i++)
                {
                    x1 = j * R / (check_insidepoints) * Math.Cos(2 * Math.PI / check_anglesteps) + x;
                    y1 = j * R / (check_insidepoints) * Math.Sin(2 * Math.PI / check_anglesteps) + y;

                    Color c = GetMaskColor(x1, y1);

                    if (c.R > 200 && c.B < 5 && c.G > 200) return 1;// на границе (жёлтый)

                }

            }
            return 0;
        }

        

        public int FindDestination_Yellow()
        {
            double ex = 0;
            double ey = 0;
            int ec = 0;
            int fst = 5;

            // пробежим по маске и найдем все красные, вычислим центр
            for (int i = 1; i < bmpmask.Width / fst; i++)
                for (int j = 1; j < bmpmask.Height / fst; j++)
                {
                    Color c = bmpmask.GetPixel(i * fst, j * fst);

                    if (c.R > 250 && c.B < 10 && c.G > 250)
                    {
                        ec++;
                        ex += i * fst / scale;
                        ey += j * fst / scale;
                    }

                }
            
            destinationy_x = ex / ec;
            destinationy_y = ey / ec;

            return 0;



        }
        public int FindDestination()
        {
            double ex = 0;
            double ey = 0;
            int ec = 0;
            int fst = 5;

            // пробежим по маске и найдем все красные, вычислим центр
            for (int i = 1; i < bmpmask.Width / fst; i++)
                for (int j = 1; j < bmpmask.Height / fst; j++)
                {
                    Color c = bmpmask.GetPixel(i * fst, j * fst);

                    if (c.R > 250 && c.B < 5 && c.G < 5)
                    {
                        ec++;
                        ex += i * fst / scale;
                        ey += j * fst / scale;
                    }

                }

            destination_x = ex / ec;
            destination_y = ey / ec;

            return 0;



        }
        public double PutintoCell(Man m) // считает ячейку в которой находится человек, сравнивает с предыдущим значением
         {
            int i1 = (int)Math.Truncate(m.x[0] / Cellsize) + 1;
            int j1 = (int)Math.Truncate(m.x[1] / Cellsize) + 1;
            //сравниваю с предыдущими
            if (i1 != m.i0 || j1 != m.j0)
            {
                if(m.i0>=0 && m.j0>=0)
                  RemoveCell(m, m.i0, m.j0);// 
                ChangeCell(m);// добавили в ячейку
            }

            /*if (i1 < 0 || i1 >= CellCount[0] || j1 < 0 || j1 >= CellCount[1]) return 2;// проверяет вдруг какие-то ошибки

            int ibegin = Math.Max(0, i1 - 1);
            int jbegin = Math.Max(0, j1 - 1);
            int iend = Math.Min(CellCount[0] - 1, i1 + 1);
            int jiend = Math.Min(CellCount[1] - 1, j1 + 1);
            
            Man m = new Man();
            for (int i = 0; i < 1; i++)
            {
                m.x[i] = x[i];

            }*/
            //i0 = i1;// i0 и j0 стали предыдущими
           // j0 = j1;
            return 0;


        }


        public void ChangeCell(Man m)//функция добавления человека в ячейку
        {

            int i = (int)Math.Truncate(m.x[0] / Cellsize) + 1;
            int j = (int)Math.Truncate(m.x[1] / Cellsize) + 1;

            if (i >= 0 && i < CellCount[0] && j >= 0 && j < CellCount[1])
            {

                cells[i, j].People.Add(m);
                m.i0 = i;
                m.j0 = j;
            }
            else {
                LogWrite("Man is outside the region! ------------------------------");
            }
           

        }

        
        public void RemoveCell(Man m, int i, int j)
        {
            
            cells[i, j].People.Remove(m);
        }

        
        public class Border
        {
            public int type; // 1-plane, 2-circle
            public double[] n = new double[2];
            public double R;
            //public double f;
            public double[,] limits = new double[3,3];
        }
        
        
        Random rnd = new Random();

        public void MyInit(double bf, double delta_c, double delta_d)
        {

            cells = new Cell[CellCount[0], CellCount[1]];

            for (int i = 0; i < CellCount[0]; i++)
            {
                for (int j = 0; j < CellCount[1]; j++)
                {
                    cells[i, j] = new Cell();
                }

            }


            for (int i = 0; i < HashN; i++)
            {
                hTable[i] = new SortedList<double, Event>(new DuplicateKeyComparer<double>());
            }

            //for(int i=0;i<10;i++)
            //{

            //    Man m = NewMan();
            //    People.Add(m);

            //    NewEvent(1, CurrentTime + 1.0 / m.f, m);

            // круги
            /*b.type = 2;
            b.n[0] = 10;
            b.n[1] = 2 + CellCount[1] / 2 * Cellsize;
            b.R = 1;

            Borders.Add(b);


            b = new Border();//круги
            b.type = 2;
            b.n[0] = 15;
            b.n[1] = 3 + CellCount[1] / 2 * Cellsize;
            b.R = 1;

            Borders.Add(b);*/


            //b = new Border();//вход
            //b.type = 3;
            //b.n[0] = 1;
            //b.n[1] = 0;
            //b.R = 0;
            //b.f = bf;

            //Borders.Add(b);
            //NewEvent(2, 0, null, b);




            /*b = new Border();// вроде как линии
            b.type = 1;
            b.n[0] = -1.0 / Math.Pow(2, 0.5);
            b.n[1] = 1.0 / Math.Pow(2, 0.5);
            b.R = -5;
            b.limits[0, 0] = 0;
            b.limits[1, 0] = ChannelWidth;*/

            /*  StreamReader border = new StreamReader("Borders.txt");
              string line;
              while ((line = border.ReadLine()) != null)
              {

                  Border b = new Border();

                  //double b.type, b.R, b.limits[0, 0], b.limits[1, 0], b.limits[0, 1];
                  //double b.n[0], b.n[1];

                  Char delimiter = ' ';
                  String[] substrings = line.Split(delimiter);
                  b.type = int.Parse(substrings[0].Replace('.', ','));
                  b.n[0] = double.Parse(substrings[1].Replace('.', ','));
                  b.n[1] = double.Parse(substrings[2].Replace('.', ','));
                  b.R = double.Parse(substrings[3].Replace('.', ','));
                  b.limits[0, 0] = double.Parse(substrings[4].Replace('.', ','));
                  b.limits[1, 0] = double.Parse(substrings[5].Replace('.', ','));
                  b.limits[0, 1] = double.Parse(substrings[6].Replace('.', ','));
                  b.limits[1, 1] = double.Parse(substrings[7].Replace('.', ','));

                  Borders.Add(b);

              }*/
            /*
                        b = new Border();//горизонтальная линия
                        b.type = 1;
                        b.n[0] = 0;
                        b.n[1] = 1;
                        b.R = 0;  //CellCount[1] / 2 * Cellsize;
                        //b.R = CellCount[1]/2* Cellsize;
                        b.limits[0, 0] = 0;
                        b.limits[1, 0] = ChannelLength;

                        Borders.Add(b);


                        b = new Border();//горизонтальная линия
                        b.type = 1;
                        b.n[0] = 0;
                        b.n[1] = -1;
                        b.R = - ChannelWidth;
                        //b.R = CellCount[1]/2* Cellsize;
                        b.limits[0, 0] = 0;
                        b.limits[1, 0] = ChannelLength;

                        Borders.Add(b);

                        b = new Border();//левая граница
                        b.type = 3;
                        b.n[0] = 1;
                        b.n[1] = 0;
                        b.R = 0;
                        b.limits[0, 1]=0;
                        b.limits[1, 1]= ChannelWidth;

                        Borders.Add(b);

                        b = new Border();// правая граница, выход
                        b.type = 3;
                        b.n[0] = -1;
                        b.n[1] = 0;
                        b.R = -ChannelLength;
                        b.limits[0, 1] = 0;
                        b.limits[1, 1] = ChannelWidth - 7.5;

                        Borders.Add(b);

                        /*
                        b = new Border();// вроде как линии
                        b.type = 1;
                        b.n[0] = -1.0/Math.Pow(2,0.5);
                        b.n[1] = 1.0 / Math.Pow(2, 0.5);
                        b.R = -5;
                        b.limits[0, 0] = 0;
                        b.limits[1, 0] = 15;


                        Borders.Add(b);
                         * */

            if (1 == 1)
            {
                //StreamReader studentsXY = new StreamReader("Razmeshenie.txt");

                string line1;
                while ((line1 = studentsXY.ReadLine()) != null)
                {
                    Console.WriteLine(line1);
                    double c = 0;
                    double d = 0;

                    d = 0.6;// тут частота длина шага
                                                                 //d = 0.65;

                    c = 2.55 + (rnd.NextDouble() - 0.5) * 0.3;// 2.2 
                                                              // c = 2.2;
                                                              // d = 0.6;
                                                              //c = 1.5;
                    double x, y, numbertrain;
                    int direction;

                    Char delimiter = ' ';
                    x = double.Parse(line1.Split(delimiter)[0].Replace('.', ','));
                    //Console.WriteLine(line1);
                    y = double.Parse(line1.Split(delimiter)[1].Replace('.', ','));
                    //Console.WriteLine(line1);
                    //numbertrain = double.Parse(substrings1[2].Replace('.', ','));
                    numbertrain = 0;
                    direction = int.Parse(line1.Split(delimiter)[2].Replace('.', ',')); //--2021
                    AddManToX(x, y, d, c, numbertrain, direction); //добавляем людей в геометрию

                }
            }




        }


        public Man AddManToX(double x, double y, double l, double f, double numbertrain, int direction)
        {


            Man m = new Man();
            m.Type = 1;
            m.direction = direction; //--2021
            m.x[0] = x;
            m.x[1] = y;
            m.r = height / 8; //0.2; 

           // double a = rnd.NextDouble() * 2 * Math.PI; // что за а?
            m.d[0] = 1;
            m.d[1] = 0;
            m.dw[0] = 1;
            m.dw[1] = 0;
            m.l = l;
            m.f = f;
            /*
            if (direction == 1)
            {
                m.color = Color.DarkBlue;
            }
            if (direction == 2)
            {
                m.color = Color.Red;
            }*/
            //m.color = Color.Black;
            m.color = Color.FromArgb((int)Math.Truncate(rnd.NextDouble() * 255), (int)Math.Truncate(rnd.NextDouble() * 255), (int)Math.Truncate(rnd.NextDouble() * 255));
            m.i0 = -1;
            m.j0 = -1;
            m.num_train = numbertrain;
            peoples++;

            PutintoCell(m);

            NewEvent(1, CurrentTime + 1.0 / m.f, m, null);

            return m;
        }



        public void NewEvent(int type, double t, Man m, Border b)
        {
            Event e = new Event();
            e.t = t;
            e.type = type;
            e.m = m;
            e.b = b;
            
            QueueADD(t, e);
        }

        public void QueueADD(double t, Event e)
        {
            int k = HashF(t);
            hTable[k].Add(t, e);
        }

        public Man NewMan() //нигде не вызываем
        {

            Man m = new Man();
            m.Type = 1;
            m.x[0] = rnd.NextDouble()*1.0;
            //m.x[0] = rnd.NextDouble() * 15.0;
            m.r = 2;
            m.x[1] = rnd.NextDouble() * (ChannelWidth - 2 * m.r) + m.r; 
            
            double a = rnd.NextDouble()*2*Math.PI; // вроде не используем
            m.d[0] = 1;
            m.d[1] = 0;
            m.dw[0] = 1;
            m.dw[1] = 0;
            m.l = height / co; //+ rnd.NextDouble() * 0.2; // задаём длину шага 
            m.f = co * 1.55 / height; //2.55 + (rnd.NextDouble() - 0.5) * 0.3; //задаём частоту шага
            m.color = Color.FromArgb((int)Math.Truncate(rnd.NextDouble() * 255), (int)Math.Truncate(rnd.NextDouble() * 255), (int)Math.Truncate(rnd.NextDouble() * 255));
            m.i0 = -1;
            m.j0 = -1;
            peoples2++; //peoples
            return m;

        }




        public void RemovePeople(Man m)  // удаляем и считаем сколько удалили
        {
            cells[m.i0, m.j0].People.Remove(m);
            //if ( m.num_train == 1.0) { peoplenum++; } //2020
            peoplenum++;
            if (m.direction==1)
            {
                expeople1++;
            }
            if (m.direction==2)
            {
                expeople2++;
            }
           // m.num_train++;
            peoples2--; //peoples
        }

     /* public void PrintField()
        {
            StreamWriter sw = new StreamWriter("Field - "+ CurrentTime.ToString()+".txt");

            foreach(Man m in People)
            {
                sw.WriteLine("Man x=" + m.x[0].ToString() + " y=" + m.x[1].ToString());

            }
            sw.Flush();
            sw.Close();
        }*/
       
        public void PrintRemove(Man m, string filename)// вывод в файл у координаты удаленнных 
        {
            StreamWriter dl1 = new StreamWriter("Remove1_.txt",true);
            //StreamWriter dl2 = new StreamWriter("Remove2.txt", true);
            StreamWriter dl2 = new StreamWriter(filename, true);
            //foreach (Man m in People)
            //{
            if (m.direction == 1)   dl1.WriteLine(CurrentTime); //m.num_train
            dl1.Flush();
            dl1.Close();

            if (m.direction == 2) dl2.WriteLine(CurrentTime); //m.num_train
            //}
            dl2.Flush();
            dl2.Close();
        }

        double delT=5;
        double lastT = 0;
        
        public void Simulate(double EndTime)
        {
            while(CurrentTime < EndTime)
            {
                if (DoEvent()==1) { CurrentTime = EndTime; }
                if ((CurrentTime-lastT >= delT) )
                {
                    lastT = CurrentTime;
                    double c = 0;
                    double d = 0;

                    d = 0.6; // + (rnd.NextDouble() - 0.5) * 0.2;
                                                                 

                    c = 2.55 + (rnd.NextDouble() - 0.5) * 0.3;
                    //AddManToX(0, 2 + rnd.NextDouble() * (bmpmask.Height-100) / scale, d, c, 0, 1);

                    //AddManToX(bmpmask.Width/scale, 2 + rnd.NextDouble() * (bmpmask.Height-100) /scale, d, c, 0, 2);
                    //peoples2 += 2;

                }
            }

        }


        public int DoEvent()
        {

            
            double T = CurrentTime;
            int Y=0;
            int k = HashF(CurrentTime);
            int EmptyEl = 0;

            while ((hTable[k].Count == 0) || ((hTable[k].Count > 0) && (hTable[k].Keys[0] > T + 2 * HashDT)))
            {
                if (hTable[k].Count == 0) EmptyEl++;
                if (EmptyEl > HashN) return 1;
                T = T + HashDT;
                k = HashF(T);
                Y++;
            }


            Event e = hTable[k].Values[0];
            //Event e = Queue.Values[0];

            CurrentTime = e.t;

            switch (e.type)
            {
                case 1:
                    e.m.DoStep(this);
                    if (CheckBorders(e.m) == 0)// проверяется только на выходе, так и должно быть?
                    {
                        NewEvent(1, CurrentTime + 1.0 / e.m.f, e.m, null);
                    }
                    break;
                case 2:
                     
                        Man m = NewMan();

                         PutintoCell(m);

                        NewEvent(1, CurrentTime + 1.0 / m.f, m, null);
                      //  NewEvent(2, CurrentTime + 1.0 / e.b.f, null, e.b);
                    
                    break;
            }


            //Queue.RemoveAt(0);
            hTable[k].RemoveAt(0);

            return 0;
        }

        


        public int CheckBorders(Man m) //удаляем вышедших за границу
        {
            string bottleneck = height.ToString() + '_'+ co.ToString() + '_' + "1"; // xxx
            if (m.direction == 2)
            {
                if (m.area1 && ExitCheckArea1(m.x[0], m.x[1], m.r) == 1)   //2022
                {
                    string filename = "RemoveArea1_" + bottleneck + ".txt";
                    PrintRemove(m, filename);
                    m.area1 = false;

                    return 0;
                }
                if (m.area2 && ExitCheckArea2(m.x[0], m.x[1], m.r) == 1)   //2022
                {
                    string filename = "RemoveArea2_" + bottleneck + ".txt";
                    PrintRemove(m, filename);
                    m.area2 = false;

                    return 0;
                }
                if (ExitCheck(m.x[0], m.x[1], m.r) == 1)   //можно exitcheck_yellow
                {
                    string filename = "Remove_" + bottleneck + ".txt";
                    PrintRemove(m, filename);

                    RemovePeople(m);
                    peoples2 -= 1;
                    return 1;
                }
            }
            if (m.direction == 1)//для тех кто идёт направо
            {
                if (ExitCheck(m.x[0], m.x[1], m.r) == 1)
                {
                    string filename = "Remove" + bottleneck + ".txt";
                    PrintRemove(m, filename);

                    RemovePeople(m);
                    peoples2 -= 1;
                    return 1;
                }
            }
            return 0;

            //foreach (Border b in Borders)
            //{
            //    if (b.type == 5)
            //    {
            //        if ((b.n[0]*m.x[0]+b.n[1]*m.x[1]-b.R)<0)
            //        {
            //            // нужно запомнить у координату и вывести в файл
            //           // PrintRemove(m);
            //            RemovePeople(m);
            //            return 1;
            //        }
            //    }

            //}
            //return 0;


        }
     


        public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
        {
            #region IComparer<TKey> Members

            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;   // Handle equality as beeing greater
                else
                    return result;
            }

            #endregion
        }

    }
}