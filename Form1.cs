using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ph2beta
{
	public partial class Form1 : Form
	{
		protected Bitmap img = null; 
		private int[] hist; // для гистограммы
		protected Bitmap img1 = null; // окошко графика
		List<Point> point = new List<Point> { new Point(0, 255), new Point(255, 0)}; // список всех точек для интерполяции
		
		public Form1()
		{
			InitializeComponent();
			img1 = (Bitmap)pictureBox1.Image;
		}

		#region // окно графика
		private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
		{
			int pointIn = 0;
			for (int i = 0; i < point.Count; i++)
			{
				if(
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 8) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 7) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 6) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 5) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 4) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 3) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 2) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y + 1) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y)     ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 1) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 2) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 3) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 4) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 5) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 6) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 7) ||
				(point[i].X < e.X + 10) && (point[i].X > e.X - 10) && (point[i].Y == e.Y - 8)
					)
				{
					pointIn = i;
				}
			}
			if (pointIn != 0)
			{
				point.RemoveAt(pointIn);
			}
			else
			{
				point.Add(new Point(e.X, e.Y));
			}

			point.Sort((x, y) => x.X.CompareTo(y.X));

			using (Graphics g = Graphics.FromImage(img1))
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.Clear(Color.White);
			}

			imageEditing();

			pictureBox1.Refresh();

			drawing(); 
			textBox1.Text = Convert.ToString(e.X);
			textBox2.Text = Convert.ToString(e.Y);
		}

		private void drawing()
		{
			using (Graphics g = Graphics.FromImage(img1))
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = SmoothingMode.HighQuality;
				var p = Pens.Red.Clone() as Pen;
				p.Width = 5;


			
				for (int i = 0; i < point.Count - 1; i++)
				{
					PointF point1 = new PointF(point[i].X, point[i].Y);
					PointF point2 = new PointF(point[i + 1].X, point[i + 1].Y);
				
					g.DrawLine(p, point1, point2);

					Pen pen = new Pen(Color.Black, 8f);
					g.DrawEllipse(pen, point[i].X - 3, point[i].Y - 3, 8, 8);
				}
				
			}
			pictureBox1.Refresh();
		}
		#endregion

		#region // гистограмма
		private void pictureBox2_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();//диалог выбора файла
			string filename_2 = ofd.FileName;
			ofd.Filter = "|*.png;*.jpg;*.bmp;*.gif;";
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				try
				{
					// загружаем изображение
					img= new Bitmap(ofd.FileName);
					pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
					// делаем размеры img 512 512
					Bitmap img22 = new Bitmap(img, new Size(512, 512));
					img = img22;

					pictureBox2.Image = img;
					
					hist = GetHistogramm(img as Bitmap);

					this.pictureBox3.Paint += (o, e) => DrawHistogramm(e.Graphics, pictureBox3.ClientRectangle, hist, pictureBox3.Height);
					this.pictureBox3.Resize += (o, e) => Refresh();
					Refresh(); 
				}
				catch // в случае ошибки выводим MessageBox
				{
					MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

			}
		}
		private static void DrawHistogramm(Graphics g, Rectangle rect, int[] hist, int Height)
		{
		
			float max = hist.Max();
			if (max > 0)
				for (int i = 0; i < hist.Length; i++)
				{
					float h = rect.Height * hist[i] / (float)max;
					g.FillRectangle(Brushes.Purple, i * rect.Width / (float)hist.Length, rect.Height - h, rect.Width / (float)hist.Length, h);
				}
		}

		private static int[] GetHistogramm(Bitmap image)
		{
			int[] result = new int[256];
			Color color;
			for (int x = 0; x < image.Width; x++)
				for (int y = 0; y < image.Height; y++)
				{
					color = image.GetPixel(x, y);
					int r = color.R;

					int g = color.G;

					int b = color.B;

					int height = (r + g + b) / 3;
					result[height]++;
				}

			return result;
		}
		#endregion

		#region // логика графика
		private void imageEditing() // пробую обработать изображение
		{
			
			int widthImage = img.Width;
			int heightImage = img.Height;
			byte[] m = new byte[0];


			using (Bitmap _tmp = new Bitmap(widthImage, heightImage, PixelFormat.Format24bppRgb))
			{
				//устанавливаем DPI такой же как у исходного
				_tmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);
				using (var g = Graphics.FromImage(_tmp))
				{
					g.DrawImageUnscaled(img, 0, 0);
				}
				m = getImgBytes(_tmp);
			}

			for (int i = 0; i < m.Length; i += 3)
			{
				
				List<Point> Lpoints = new List<Point>();// массив точек которые левее
				List<Point> Rpoints = new List<Point>();// массив точек которые правее
				
				foreach(var mypoint in point)
				{
					if (mypoint.X < m[i])
						Lpoints.Add(mypoint);
					if (mypoint.X > m[i])
						Rpoints.Add(mypoint);
				}
				int правая_точкаX = Rpoints[0].X;
				int левая_точкаX = Lpoints[0].X;
				foreach (var el in Lpoints)
				{
					if (левая_точкаX < el.X)
						левая_точкаX = el.X;
				}
				foreach (var el in Rpoints)
				{
					if (правая_точкаX > el.X)
						правая_точкаX = el.X;
				}
				var e = правая_точкаX;
				правая_точкаX = левая_точкаX;
				левая_точкаX = e;
				int левая_точкаY = 0;
				int правая_точкаY = 0;

				foreach (var el in point)
				{
					if (el.X == левая_точкаX)
						левая_точкаY = el.Y;
					if (el.X == правая_точкаX)
						правая_точкаY = el.Y;
				}
				int расстояние_по_x = левая_точкаX - правая_точкаX;
				int расстояние_по_y = правая_точкаY - левая_точкаY;
				int расстояние_до_i = левая_точкаX - m[i];
				int xx = (int)(расстояние_до_i * расстояние_по_y) / расстояние_по_x;
				m[i] = (byte)xx;
			}
			
			for (int i = 1; i < m.Length; i += 3)
			{
				List<Point> Lpoints = new List<Point>();// массив точек которые левее
				List<Point> Rpoints = new List<Point>();// массив точек которые правее

				foreach (var mypoint in point)
				{
					if (mypoint.X < m[i])
						Lpoints.Add(mypoint);
					if (mypoint.X > m[i])
						Rpoints.Add(mypoint);
				}
				
				int правая_точкаX = Rpoints[0].X;
				int левая_точкаX = Lpoints[0].X;
				foreach (var el in Lpoints)
				{
					if (левая_точкаX < el.X)
						левая_точкаX = el.X;
				}

				foreach (var el in Rpoints)
				{
					if (правая_точкаX > el.X)
						правая_точкаX = el.X;
				}
				var e = правая_точкаX;
				правая_точкаX = левая_точкаX;
				левая_точкаX = e;
				int левая_точкаY = 0;
				int правая_точкаY = 0;

				foreach (var el in point)
				{
					if (el.X == левая_точкаX)
						левая_точкаY = el.Y;
					if (el.X == правая_точкаX)
						правая_точкаY = el.Y;
				}
				int расстояние_по_x = левая_точкаX - правая_точкаX;
				int расстояние_по_y = правая_точкаY - левая_точкаY;
				int расстояние_до_i = левая_точкаX - m[i];
				int xx = (int)(расстояние_до_i * расстояние_по_y) / расстояние_по_x;
				m[i] = (byte)xx;
			}
			
			for (int i = 2; i < m.Length; i += 3)
			{

				List<Point> Lpoints = new List<Point>();// массив точек которые левее
				List<Point> Rpoints = new List<Point>();// массив точек которые правее

				foreach (var mypoint in point)
				{
					if (mypoint.X < m[i])
						Lpoints.Add(mypoint);
					if (mypoint.X > m[i])
						Rpoints.Add(mypoint);
				}
				
				int правая_точкаX = Rpoints[0].X;
				int левая_точкаX = Lpoints[0].X;
				foreach (var el in Lpoints)
				{
					if (левая_точкаX < el.X)
						левая_точкаX = el.X;
				}

				
				foreach (var el in Rpoints)
				{
					if (правая_точкаX > el.X)
						правая_точкаX = el.X;
				}
				var e = правая_точкаX;
				правая_точкаX = левая_точкаX;
				левая_точкаX = e;
				int левая_точкаY = 0;
				int правая_точкаY = 0;

				foreach (var el in point)
				{
					if (el.X == левая_точкаX)
						левая_точкаY = el.Y;
					if (el.X == правая_точкаX)
						правая_точкаY = el.Y;
				}
				int расстояние_по_x = левая_точкаX - правая_точкаX;
				int расстояние_по_y = правая_точкаY - левая_точкаY;
				int расстояние_до_i = левая_точкаX - m[i];
				int xx = (int)(расстояние_до_i * расстояние_по_y) / расстояние_по_x;
				m[i] = (byte)xx;
			}

		
			Bitmap img_ret = new Bitmap(widthImage, heightImage, PixelFormat.Format24bppRgb);
			img_ret.SetResolution(img.HorizontalResolution, img.VerticalResolution);
			writeImageBytes(img_ret, m);
			
			SaveImg(img_ret);
			pictureBox2.Refresh();
			// обновление гисограммы
			hist = GetHistogramm(img_ret as Bitmap);
			this.pictureBox3.Paint += (o, e) => DrawHistogramm(e.Graphics, pictureBox3.ClientRectangle, hist, pictureBox3.Height);
			this.pictureBox3.Resize += (o, e) => Refresh();
			Refresh();
		}
		private void SaveImg(Bitmap img_out) 
		{
			pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBox2.Image = img_out; 
		}
		static void writeImageBytes(Bitmap img, byte[] bytes)
		{
			var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
			ImageLockMode.WriteOnly,
			img.PixelFormat);
			Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
			img.UnlockBits(data); 
		}
		static byte[] getImgBytes(Bitmap img)
		{
			byte[] bytes = new byte[img.Width * img.Height * 3]; 
			var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), 
			ImageLockMode.ReadOnly,
			img.PixelFormat);
			Marshal.Copy(data.Scan0, bytes, 0, bytes.Length); 
			img.UnlockBits(data); 
			return bytes; 
		}

		#endregion

		#region // то что не нужно
		private void textBox2_TextChanged(object sender, EventArgs e)
		{

		}

		#endregion
	}
}
