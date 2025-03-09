using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1: Form
    {
        Bitmap image;
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp; | All files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true) image = newImage;

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void размытиепоГаусуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void инверсияToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void чернобелыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void яркостьВышеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new BrightnessUpFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void собельToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void волныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new WaveFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void переносToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new ShiftFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void резкостьдопToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SharpnessSecFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }

        private void выделениеГраницToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SharrFilter();
            backgroundWorker1.RunWorkerAsync(filters);
        }
    }
}
