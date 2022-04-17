using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace pdyn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Library problem = new Library();
        

        private void button1_Click(object sender, EventArgs e)
        {
            
            problem.MyInit(double.Parse(textBox3.Text), double.Parse(textBox5.Text), double.Parse(textBox6.Text));
           // problem.PrintField();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < int.Parse(textBox2.Text); i++)
            {
                problem.Simulate(problem.CurrentTime + double.Parse(textBox1.Text));
                //problem.Simulate(problem.CurrentTime + 1);
                //problem.PrintField();
                DrawField();
                //pictureBox1.Image.Save("Field-"+("0000000"+i.ToString()).PadRight(5)+".png");
                pictureBox1.Image.Save("AField-" + problem.CurrentTime.ToString() + ".png");
                label1.Text = "People left: " + problem.peoplenum.ToString(); //вышедшие
                label2.Text = "Number of people: " + problem.peoples2.ToString();//общее кол-во людей
                label4.Text = "Current time: " + problem.CurrentTime.ToString();

                problem.ras.WriteLine(problem.bmpmask.Height.ToString()+" "+problem.bmpmask.Width.ToString() +" "+problem.CurrentTime.ToString() + " " + problem.peoplenum.ToString() + ' '+ problem.expeople1.ToString()+' '+problem.expeople2.ToString());
                problem.rightpeop.WriteLine(problem.expeople1.ToString()+" "+problem.peoples2.ToString());

               // problem.comfort_people.WriteLine(problem.CurrentTime.ToString() + " " + problem.comfort_p.ToString());
                //problem.comfort_people.Flush();



            }
            problem.rightpeop.Flush();
            problem.ras.Flush();
            //problem.ras.Close();
        }


        public void DrawField()
        {
            double scale = problem.scale; // pixels/m было 50
            float y0 = 0;

            Brush br = new SolidBrush(Color.White);

            Brush br2 = new SolidBrush(Color.Yellow);
            Brush br3 = new SolidBrush(Color.Red);

            Brush gridbrush = new SolidBrush(Color.Silver);
            Font gridfont = new Font("Arial",6);
            Pen gridpen = new Pen(Color.Silver);

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }


            if (problem.CurrentTime>=10000.0) {problem.bmpmask=problem.dop_bmpmask; }
            Bitmap bmp = new Bitmap(problem.bmpmask.Width, problem.bmpmask.Height);
            
            Graphics gr = Graphics.FromImage(bmp);


            
            gr.FillRectangle(br, 0, 0, problem.bmpmask.Width, problem.bmpmask.Height);



            gr.DrawImage(problem.bmpmask,0,y0);


           /*
           foreach (pdyn.Library.Border b in problem.Borders)
                    {
                        if (b.type == 2)
                        {
                            gr.FillEllipse(br2, (float)((b.n[0] - b.R) * scale), (float)((b.n[1] - b.R) * scale)+y0, (float)((2*b.R) * scale), (float)((2*b.R) * scale)); // почему так задали ?

                        }

            }

            Pen pb = new Pen(Color.Green);

            foreach (pdyn.Library.Border b in problem.Borders)
             {
                 if ((b.type == 1) || (b.type == 4))
                 {
                    double xf = b.limits[0,0];
                    double xt = b.limits[1,0];
                    gr.DrawLine(pb, (float)(xf * scale), (float)((b.R - xf * b.n[0]) / b.n[1] * scale)+y0, (float)(xt * scale), (float)((b.R - xt * b.n[0]) / b.n[1] * scale)+y0);
                }
            }

            pb.Color = Color.Red;

            foreach (pdyn.Library.Border b in problem.Borders)
            {
                if ((b.type == 3)|| (b.type == 5))//no vixod
                {
                    double yf = b.limits[0, 1];
                    double yt = b.limits[1, 1];
                    gr.DrawLine(pb, (float)((b.R - yf*b.n[1]) / b.n[0] * scale), (float)(yf * scale) + y0, (float)((b.R-yt*b.n[1]) / b.n[0] * scale), (float)(yt * scale )+ y0 );

                }
            }
            pb.Color = Color.Blue;

            foreach (pdyn.Library.Border b in problem.Borders)
            {
                if (b.type == 10)// тип 4 - вертикальная стена, выход
                {
                    double y
                    b.limits[0, 1];
                    double yt = b.limits[1, 1];
                    gr.DrawLine(pb, (float)((b.R - yf * b.n[1]) / b.n[0] * scale), (float)(yf * scale) + y0, (float)((b.R - yt * b.n[1]) / b.n[0] * scale), (float)(yt * scale) + y0);

                }
            }

            //Pen p = new Pen(Color.Red);//границы
            //gr.DrawLine(p, new Point(0, 0), new Point(500, 0));
            //gr.DrawLine(p, new Point(0, 100), new Point(500, 100));к
            //gr.DrawLine(p, new Point(0, 0), new Point(0, 100));
            //gr.DrawLine(p, new Point(500, 0), new Point(500, 100));

            */

            if (checkBox1.Checked)
            {
                foreach (Library.Cell c in problem.cells)
                {
                    foreach (Library.Man m in c.People)
                    {
                        double menred = m.r * scale;
                        if (menred < problem.draw_minmanradius) menred = problem.draw_minmanradius;
                        Brush br1 = new SolidBrush(m.color);
                        gr.FillEllipse(br1, (float)(m.x[0] * scale - menred), (float)(m.x[1] * scale - menred) + y0, (float)(2 * menred), (float)(2 * menred));
                        Pen linepen = new Pen(m.color,(float)(menred/2.0));
                        gr.DrawLine(linepen, (float)(m.x[0] * scale), (float)(m.x[1] * scale), (float)(m.x[0] * scale + 2 * menred * m.dw[0]), (float)(m.x[1] * scale + 2 * menred * m.dw[1]));
                        //gr.DrawString(m.x[0].ToString() + ", " + m.x[1].ToString(), gridfont, br1, (float)(m.x[0] * scale), (float)(m.x[1] * scale));

                    }
                }
            }

            if (checkBox2.Checked)
            {
                double hlinesstep = problem.draw_meshstep; //в метрах

                for (int hlines = 1; hlines < bmp.Height / (hlinesstep * scale); hlines++)
                {
                    gr.DrawLine(gridpen, (float)(0.0), (float)(hlines * hlinesstep * scale), (float)(bmp.Width), (float)(hlines * hlinesstep * scale));
                    gr.DrawString((hlines * hlinesstep).ToString(), gridfont, gridbrush, (float)(0.0), (float)(hlines * hlinesstep * scale));
                    gr.DrawString((hlines * hlinesstep).ToString(), gridfont, gridbrush, (float)(Math.Floor(bmp.Width / (hlinesstep * scale)-1) * hlinesstep * scale), (float)(hlines * hlinesstep * scale));
                }

                for (int hlines = 1; hlines < bmp.Width / (hlinesstep * scale); hlines++)
                {
                    gr.DrawLine(gridpen, (float)(hlines * hlinesstep * scale), (float)(0.0), (float)(hlines * hlinesstep * scale), (float)(bmp.Height));
                    gr.DrawString((hlines * hlinesstep).ToString(), gridfont, gridbrush, (float)(hlines * hlinesstep * scale), (float)(0.0));
                    gr.DrawString((hlines * hlinesstep).ToString(), gridfont, gridbrush, (float)(hlines * hlinesstep * scale), (float)(Math.Floor(bmp.Height / (hlinesstep * scale)-1) * hlinesstep * scale));
                }
            }

            pictureBox1.Width = bmp.Width;
            pictureBox1.Height = bmp.Height;
           // MessageBox.Show(bmp.Width.ToString()+ bmp.Height.ToString());
            pictureBox1.Image = bmp;
            pictureBox1.Image.Save("Field.png");

            gr.Dispose();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
           
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            problem.peoplenum = 0;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            DrawField();
            label1.Text = problem.peoplenum.ToString();
            label2.Text = problem.peoples.ToString();
            label4.Text = problem.CurrentTime.ToString();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Color ccc = problem.bmpmask.GetPixel(e.X, e.Y);

            MessageBox.Show("x "+e.X+ ", "+ e.Y +" c "+ ccc.R + ", " + ccc.G + ", " + ccc.B);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
             {
                problem.bmpmask = new Bitmap(fd.FileName);
                //problem.bmpmask = new Bitmap("rhob_5"); 
                problem.bmpmask.Save("MaskLoaded.bmp");
                if (checkBox3.Checked)
                {
                    problem.FindDestination_Yellow();
                    problem.FindDestination();
                    MessageBox.Show("Destination (red): " + problem.destination_x.ToString() + ", " + problem.destination_y.ToString(), "Destination(yellow): " + problem.destinationy_x.ToString() + ", " + problem.destinationy_y.ToString());
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            FileDialog fd1 = new OpenFileDialog();
            if (fd1.ShowDialog() == DialogResult.OK)
            {
                problem.studentsXY = new System.IO.StreamReader (fd1.FileName);
            }
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_MouseClick_1(object sender, MouseEventArgs e)
        {
            Color ccc = problem.bmpmask.GetPixel(e.X, e.Y);

            MessageBox.Show("x " + e.X + ", " + e.Y + " c " + ccc.R + ", " + ccc.G + ", " + ccc.B);

            //problem.bmpmask.SetPixel(e.X, e.Y, Color.Red);
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            {
                FileDialog fd = new OpenFileDialog();
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    problem.dop_bmpmask = new Bitmap(fd.FileName);
                    problem.dop_bmpmask.Save("MaskLoaded_2.bmp");
                }
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
