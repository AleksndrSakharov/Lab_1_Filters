using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;

namespace WindowsFormsApp1
{
    abstract class Filters
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for(int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for(int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

        
    }

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y) 
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= norm;
                }
            }
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intencivity = (int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(intencivity, intencivity, intencivity);
            return resultColor;
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 30;
            Color sourceColor = sourceImage.GetPixel(x, y);
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            int intencivity = (int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B);
            resultR = intencivity + 2 * k;
            resultG = intencivity + (int)(0.5 * k);
            resultB = intencivity - 1 * k;

            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }

    class BrightnessUpFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 30;
            Color sourceColor = sourceImage.GetPixel(x, y);
            return Color.FromArgb(Clamp(sourceColor.R + k, 0, 255), Clamp(sourceColor.G + k, 0, 255), Clamp(sourceColor.B + k, 0, 255));
        }
    }

    class SobelFilter : MatrixFilter
    {
        public SobelFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1.0f;
            kernel[1, 0] = -2.0f;
            kernel[2, 0] = -1.0f;
            kernel[0, 1] = 0;
            kernel[1, 1] = 0;
            kernel[2, 1] = 0;
            kernel[0, 2] = 1.0f;
            kernel[1, 2] = 2.0f;
            kernel[2, 2] = 1.0f;
        }
    }

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = 0;
            kernel[1, 0] = -1.0f;
            kernel[2, 0] = 0;
            kernel[0, 1] = -1.0f;
            kernel[1, 1] = 5.0f;
            kernel[2, 1] = -1.0f;
            kernel[0, 2] = 0;
            kernel[1, 2] = -1.0f;
            kernel[2, 2] = 0;
        }
    }

    class WaveFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newX = Clamp((int)(x + 20 * Math.Sin(2 * Math.PI * y / 30)), 0, sourceImage.Width - 1);
            int newY = y;
            return sourceImage.GetPixel(newX, newY);
        }
    }

    class ShiftFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newX = Clamp(x + 50, 0, sourceImage.Width - 1);
            int newY = Clamp(y, 0, sourceImage.Height - 1);
            return sourceImage.GetPixel(newX, newY);
        }
    }

    class SharpnessSecFilter : MatrixFilter
    {
        public SharpnessSecFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1.0f;
            kernel[1, 0] = -1.0f;
            kernel[2, 0] = -1.0f;
            kernel[0, 1] = -1.0f;
            kernel[1, 1] = 9.0f;
            kernel[2, 1] = -1.0f;
            kernel[0, 2] = -1.0f;
            kernel[1, 2] = -1.0f;
            kernel[2, 2] = -1.0f;
        }
    }

    class SharrFilter : MatrixFilter
    {
        public SharrFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = 3.0f;
            kernel[1, 0] = 0;
            kernel[2, 0] = -3.0f;
            kernel[0, 1] = 10.0f;
            kernel[1, 1] = 0;
            kernel[2, 1] = -10.0f;
            kernel[0, 2] = 3.0f;
            kernel[1, 2] = 0;
            kernel[2, 2] = -3.0f;
        }
    }

    class DilationFilter : Filters
    {
        private int[,] structuringElement = new int[,]
        {
        { 0, 1, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 }
        };

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = structuringElement.GetLength(0) / 2;
            int radiusY = structuringElement.GetLength(1) / 2;
            Color maxColor = Color.Black;

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    if (structuringElement[j + radiusX, i + radiusY] == 1)
                    {
                        Color neighborColor = sourceImage.GetPixel(idX, idY);
                        if (neighborColor.GetBrightness() > maxColor.GetBrightness())
                        {
                            maxColor = neighborColor;
                        }
                    }
                }
            }
            return maxColor;
        }
    }

    class ErosionFilter : Filters
    {
        private int[,] structuringElement = new int[,]
        {
        { 0, 1, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 }
        };

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = structuringElement.GetLength(0) / 2;
            int radiusY = structuringElement.GetLength(1) / 2;
            Color minColor = Color.White;

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    if (structuringElement[j + radiusX, i + radiusY] == 1)
                    {
                        Color neighborColor = sourceImage.GetPixel(idX, idY);
                        if (neighborColor.GetBrightness() < minColor.GetBrightness())
                        {
                            minColor = neighborColor;
                        }
                    }
                }
            }
            return minColor;
        }
    }


    class OpeningFilter : Filters
    {
        private ErosionFilter erosion = new ErosionFilter();
        private DilationFilter dilation = new DilationFilter();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Bitmap erodedImage = erosion.processImage(sourceImage, null);
            return dilation.calculateNewPixelColor(erodedImage, x, y);
        }
    }

    class ClosingFilter : Filters
    {
        private DilationFilter dilation = new DilationFilter();
        private ErosionFilter erosion = new ErosionFilter();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Bitmap dilatedImage = dilation.processImage(sourceImage, null);
            return erosion.calculateNewPixelColor(dilatedImage, x, y);
        }
    }

    class GradientFilter : Filters
    {
        private DilationFilter dilation = new DilationFilter();
        private ErosionFilter erosion = new ErosionFilter();

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color dilatedColor = dilation.calculateNewPixelColor(sourceImage, x, y);
            Color erodedColor = erosion.calculateNewPixelColor(sourceImage, x, y);

            int r = Clamp(dilatedColor.R - erodedColor.R, 0, 255);
            int g = Clamp(dilatedColor.G - erodedColor.G, 0, 255);
            int b = Clamp(dilatedColor.B - erodedColor.B, 0, 255);

            return Color.FromArgb(r, g, b);
        }
    }

}
