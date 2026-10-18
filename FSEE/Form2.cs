using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FSEE
{

    public partial class Form2 : Form
    {
        private string imagePath; // Store image path for later use
        private bool isSelecting = false;
        private Point startPoint;
        private Rectangle selectionRect;
        private Bitmap originalImage;
        private Bitmap workingimage;
        private Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private Stack<Bitmap> redoStack = new Stack<Bitmap>();


        public Form2(string filePath) // Constructor to accept filePath
        {
            InitializeComponent();
            imagePath = filePath; // Store file path for later use
            if (pictureBox1.Image != null)
            {
                originalImage = new Bitmap(pictureBox1.Image);
            }
        }

        public Form2()
        {
            InitializeComponent();
        }



        private void PushUndoState()
        {
            if (workingimage != null)
            {
                undoStack.Push(new Bitmap(workingimage)); // Deep copy
                redoStack.Clear(); // Clear redo history on new action
            }
        }

        //---------------------------------------------------------------------------
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;

            isSelecting = true;
            startPoint = e.Location;
            selectionRect = new Rectangle(e.Location, new Size(0, 0));
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelecting) return;

            int width = e.X - startPoint.X;
            int height = e.Y - startPoint.Y;
            selectionRect = new Rectangle(startPoint.X, startPoint.Y, width, height);
            pictureBox1.Invalidate(); // Redraw to show selection rectangle
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;

            if (selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                PushUndoState();
                CropImage();
            }
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (isSelecting)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
            }
        }
        //---------------------------------------------------------------------------

        private Bitmap AdjustSaturation(Bitmap original, float saturationFactor)
        {
            Bitmap adjusted = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color originalColor = original.GetPixel(x, y);
                    float hue = originalColor.GetHue();
                    float saturation = originalColor.GetSaturation();
                    float brightness = originalColor.GetBrightness();

                    saturation *= saturationFactor;
                    saturation = Math.Min(1.0f, Math.Max(0.0f, saturation)); // Clamp

                    Color newColor = FromAhsb(originalColor.A, hue, saturation, brightness);
                    adjusted.SetPixel(x, y, newColor);
                }
            }

            return adjusted;
        }

        public static Color FromAhsb(int a, float h, float s, float b)
        {
            if (0 > a || 255 < a)
                throw new ArgumentOutOfRangeException("a", a, "Alpha must be between 0 and 255.");
            if (0f > h || 360f < h)
                throw new ArgumentOutOfRangeException("h", h, "Hue must be between 0 and 360.");
            if (0f > s || 1f < s)
                throw new ArgumentOutOfRangeException("s", s, "Saturation must be between 0 and 1.");
            if (0f > b || 1f < b)
                throw new ArgumentOutOfRangeException("b", b, "Brightness must be between 0 and 1.");

            if (0 == s)
            {
                return Color.FromArgb(a, Convert.ToInt32(b * 255), Convert.ToInt32(b * 255), Convert.ToInt32(b * 255));
            }

            float fMax, fMid, fMin;
            int iSextant;
            if (0.5 < b)
            {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            }
            else
            {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h)
                h -= 360f;
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = h * (fMax - fMin) + fMin;
            }
            else
            {
                fMid = fMin - h * (fMax - fMin);
            }

            int iMax = Convert.ToInt32(fMax * 255);
            int iMid = Convert.ToInt32(fMid * 255);
            int iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant)
            {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        }



        private void LoadImage(string filePath)
        {
            try
            {
                if (pictureBox1 != null) // Ensure pictureBox1 is initialized
                {
                    pictureBox1.Image = Image.FromFile(filePath);
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    originalImage = new Bitmap(pictureBox1.Image);
                }
                else
                {
                    MessageBox.Show("Error: pictureBox1 is not initialized.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                LoadImage(imagePath);
            }

            if (pictureBox1.Image != null)
            {
                numericUpDownw.Value = pictureBox1.Width;
                numericUpDownh.Value = pictureBox1.Height;
                workingimage = originalImage; // Initialize base for editing
            }



        }

      

       



        private Bitmap ApplyGrayscaleEffect(Bitmap original)
        {
            Bitmap grayscaleBitmap = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color originalColor = original.GetPixel(x, y);

                    // Convert to grayscale using luminosity method
                    int grayValue = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11);
                    Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);

                    grayscaleBitmap.SetPixel(x, y, grayColor);
                }
            }

            return grayscaleBitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                PushUndoState(); // Save current state
                workingimage = ApplyGrayscaleEffect(workingimage);
                pictureBox1.Image = workingimage;
            }
        }


        private void button7_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                // Save current image to redo stack
                redoStack.Push(new Bitmap(workingimage));

                // Restore previous image from undo stack
                Bitmap previousImage = undoStack.Pop();

                // Dispose current image
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                // Apply the previous image
                pictureBox1.Image = previousImage;
                workingimage = new Bitmap(previousImage);
            }
            else
            {
                MessageBox.Show("No actions to undo.");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                PushUndoState();
                workingimage = ApplyInvertEffect(workingimage);
                pictureBox1.Image = workingimage;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                // Save current image to undo stack
                undoStack.Push(new Bitmap(workingimage));

                // Restore next image from redo stack
                Bitmap nextImage = redoStack.Pop();

                // Dispose current image
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                // Apply the next image
                pictureBox1.Image = nextImage;
                workingimage = new Bitmap(nextImage);
            }
            else
            {
                MessageBox.Show("No actions to redo.");
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                PushUndoState();
                int newWidth = (int)numericUpDownw.Value;
                int newHeight = (int)numericUpDownh.Value;

                Bitmap resizedBitmap = new Bitmap(originalImage, new Size(newWidth, newHeight));

                pictureBox1.Image.Dispose();
                pictureBox1.Image = resizedBitmap;
                workingimage = resizedBitmap;
            }
            else
            {
                MessageBox.Show("Original image not available for resizing.");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }//----------------------------------------------------------

        private void button5_Click(object sender, EventArgs e)
        {
            SaveImageExample.SaveImage(pictureBox1);
        }


        private Bitmap AdjustBrightness(Bitmap original, int brightnessValue)
        {
            Bitmap adjustedImage = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color originalColor = original.GetPixel(x, y);

                    int r = Clamp(originalColor.R + brightnessValue, 0, 255);
                    int g = Clamp(originalColor.G + brightnessValue, 0, 255);
                    int b = Clamp(originalColor.B + brightnessValue, 0, 255);

                    Color newColor = Color.FromArgb(r, g, b);
                    adjustedImage.SetPixel(x, y, newColor);
                }
            }

            return adjustedImage;
        }

        private int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }


        private void CropImage()
        {
            if (pictureBox1.Image == null || selectionRect.Width <= 0 || selectionRect.Height <= 0)
                return;

            Bitmap original = new Bitmap(pictureBox1.Image);

            // scaling factors width and height
            float xRatio = (float)original.Width / pictureBox1.Width;
            float yRatio = (float)original.Height / pictureBox1.Height;

            // PictureBox selection coordinates to original image coordinates
            Rectangle adjustedRect = new Rectangle(
                (int)(selectionRect.X * xRatio),
                (int)(selectionRect.Y * yRatio),
                (int)(selectionRect.Width * xRatio),
                (int)(selectionRect.Height * yRatio)
            );

            // Ensure crop area is within image bounds
            adjustedRect = Rectangle.Intersect(new Rectangle(0, 0, original.Width, original.Height), adjustedRect);

            // Perform the crop
            Bitmap croppedImage = original.Clone(adjustedRect, original.PixelFormat);

            // Dispose 
            pictureBox1.Image.Dispose();

            // setting cropped image to picture box
            workingimage = croppedImage;
            pictureBox1.Image = croppedImage;
        }

        private Bitmap ApplyInvertEffect(Bitmap original)
        {
            Bitmap invertedBitmap = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color originalColor = original.GetPixel(x, y);

                    // Invert colors
                    Color invertedColor = Color.FromArgb(255 - originalColor.R, 255 - originalColor.G, 255 - originalColor.B);

                    invertedBitmap.SetPixel(x, y, invertedColor);
                }
            }

            workingimage = invertedBitmap;
            return invertedBitmap;
        }

        //private Bitmap adjustbrightness(Bitmap original)
        //{
        //    if (pictureBox1.Image != null)
        //    {
        //        Bitmap originalBitmap = new Bitmap(pictureBox1.Image);
        //        int brightnessValue = trackBar1.Value; // Trackbar value between -100 and 100 for example


        //        Bitmap brightenedImage = AdjustBrightness(originalBitmap, brightnessValue);

        //        pictureBox1.Image.Dispose();
        //        pictureBox1.Image = brightenedImage;

        //        return brightenedImage;
        //    }
        //    else
        //    {
        //        MessageBox.Show("No image loaded to adjust brightness.");
        //    }

        //    return null;

        //}
        private void button6_Click(object sender, EventArgs e)
        {
           
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            if (workingimage != null)
            {
                PushUndoState();
                int brightnessValue = trackBar1.Value;
                Bitmap brightened = AdjustBrightness(workingimage, brightnessValue);
                pictureBox1.Image = brightened;
               
            }
            //else if (originalImage != null)
            //{
            //    // Fallback to original if no effect was applied before
            //    int brightnessValue = trackBar1.Value;
            //    Bitmap brightened = AdjustBrightness(originalImage, brightnessValue);
            //    pictureBox1.Image = brightened;
            //    workingimage = originalImage; // Set base for future adjustments
            //}
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDownw_ValueChanged(object sender, EventArgs e)
        {
            PushUndoState();
            ResizeImageToMatchUI();
        }

        private void numericUpDownh_ValueChanged(object sender, EventArgs e)
        {
            PushUndoState();
            ResizeImageToMatchUI();
        }

        private void ResizeImageToMatchUI()
        {
            int newWidth = (int)numericUpDownw.Value;
            int newHeight = (int)numericUpDownh.Value;

            if (workingimage != null && newWidth > 0 && newHeight > 0)
            {
                Bitmap resized = ResizeImageHighQuality(workingimage, newWidth, newHeight);

                pictureBox1.Image?.Dispose(); // Dispose old image
                pictureBox1.Image = resized;

                workingimage = new Bitmap(resized); // Update working copy

                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }


        private Bitmap ResizeImageHighQuality(Bitmap source, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(source, 0, 0, width, height);
            }
            return result;
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (workingimage != null)
            {
                PushUndoState();
                float saturationFactor = trackBar2.Value / 100.0f; // e.g., 0.0 to 2.0 (assuming trackBar2 range is 0–200)
                Bitmap adjusted = AdjustSaturation(workingimage, saturationFactor);
                pictureBox1.Image = adjusted;
               
            }
        }
    }






    public class SaveImageExample
    {
        public static void SaveImage(PictureBox pictureBox1)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("There is no image to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save Image";
                saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    
                    ImageFormat format = ImageFormat.Png; // Default format
                    switch (System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower())
                    {
                        case ".jpg":
                        case ".jpeg":
                            format = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                        case ".gif":
                            format = ImageFormat.Gif;
                            break;
                    }

                    
                    pictureBox1.Image.Save(saveFileDialog.FileName, format);
                    MessageBox.Show("Image saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}







// 1 grayscale
// 2 