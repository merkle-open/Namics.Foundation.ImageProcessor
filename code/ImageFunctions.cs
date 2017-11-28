// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageFunctions.cs" company="Namics AG">
//   (c) Namics AG
//
//  Want to work for one of Europe's leading Sitecore Agencies? 
//  Check out http://www.namics.com/jobs/
// </copyright>
// <summary>
//   Defines all image manipulation functions
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;

namespace Namics.Foundation.ImageProcessor
{
    /// <summary>
    /// Contains functions for image manipulation
    /// </summary>
    public class ImageFunctions
    {
        /// <summary>
        /// Converts a bitmap to grayscale
        /// </summary>
        /// <param name="pOriginal">
        /// The p original.
        /// </param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        public static Bitmap MakeGreyscale(Bitmap pOriginal)
        {  
            // create a blank bitmap the same size as original
            var newBitmap = new Bitmap(pOriginal.Width, pOriginal.Height);

            // get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);

            // create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
               new float[][] 
                  {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                  });

            // create some image attributes
            var attributes = new ImageAttributes();

            // set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            // draw the original image on the new image
            // using the grayscale color matrix
            g.DrawImage(pOriginal, new Rectangle(0, 0, pOriginal.Width, pOriginal.Height),0, 0, pOriginal.Width, pOriginal.Height, GraphicsUnit.Pixel, attributes);

            // dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        /// <summary>
        /// Converts the original image to a cropped image
        /// </summary>
        /// <param name="pOriginal">original bitmap</param>
        /// <param name="pStartX">cropping start-x-point</param>
        /// <param name="pStartY">cropping start-y-point</param>
        /// <param name="pWidth">cropping widht</param>
        /// <param name="pHeight">cropping height</param>
        /// <returns>Returns the cropped or the original image</returns>
        public static Bitmap CropImage(Bitmap pOriginal, int pStartX, int pStartY, int pWidth, int pHeight)
        {
            // check if the cropping parameters are in image range            
            if ((pWidth >= 0 && pHeight >= 0) && (pStartX >= 0) && ((pStartX + pWidth) <= pOriginal.Width)
                && (pStartY >= 0) && ((pStartY + pHeight) <= pOriginal.Height))
            {
                var cropRect = new Rectangle(pStartX, pStartY, pWidth, pHeight);
                var target = new Bitmap(cropRect.Width, cropRect.Height);
               
                using (var g = Graphics.FromImage(target))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(pOriginal, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
                }
                return target;
            }
            
            // return original image when cropping parameters are out of original image range
            return pOriginal;
        }

        /// <summary>
        /// Makes flip or/and rotate operations on the original bitmap 
        /// </summary>
        /// <param name="pOriginal">the original bitmap</param>
        /// <param name="pType">The RotateFlipType-Enum Value</param>
        /// <returns>the manipulated bitmap</returns>
        public static Bitmap RotateFlipImage(Bitmap pOriginal, RotateFlipType pType)
        {
            var bmap = (Bitmap)pOriginal.Clone();
            bmap.RotateFlip(pType);
            return bmap;
        }

        /// <summary>
        /// Crops the given image (from pipeline args) to the given dimensions
        /// </summary>
        /// <param name="pTemplateWidth">Width to crop image</param>
        /// <param name="pTemplateHeight">Height to crop image</param>
        /// <param name="pBitMap">The original bitmap</param>
        public static Bitmap CenterCropImage(int pTemplateWidth, int pTemplateHeight, Bitmap pBitMap)
        {
            Bitmap outputBitmap;
            var initImage = pBitMap;
            var templateRate = double.Parse(pTemplateWidth.ToString(CultureInfo.InvariantCulture)) / pTemplateHeight;
            var initRate = double.Parse(initImage.Width.ToString(CultureInfo.InvariantCulture)) / initImage.Height;

            if (templateRate == initRate) // if requested w/h is the same ratio as original
            {
                Image templateImage = new Bitmap(pTemplateWidth, pTemplateHeight);
                var templateG = Graphics.FromImage(templateImage);
                templateG.CompositingMode = CompositingMode.SourceCopy;
                templateG.InterpolationMode = InterpolationMode.High;
                templateG.SmoothingMode = SmoothingMode.None;
                templateG.Clear(Color.White);
                templateG.DrawImage(initImage, new Rectangle(0, 0, pTemplateWidth, pTemplateHeight), new Rectangle(0, 0, initImage.Width, initImage.Height), GraphicsUnit.Pixel);

                outputBitmap = (Bitmap) templateImage.Clone();
            }
            else
            {
                // if a new ratio is requested
                Image pickedImage;
                Graphics pickedG;
                var fromR = new Rectangle(0, 0, 0, 0);
                var toR = new Rectangle(0, 0, 0, 0);

                // calculate dimensions of new image
                if (templateRate > initRate)
                {
                    pickedImage = new Bitmap(initImage.Width, int.Parse(Math.Floor(initImage.Width / templateRate).ToString(CultureInfo.InvariantCulture)));
                    pickedG = Graphics.FromImage(pickedImage);
                    pickedG.CompositingMode = CompositingMode.SourceCopy;
                    fromR.X = 0;
                    fromR.Y = int.Parse(Math.Floor((initImage.Height - initImage.Width / templateRate) / 2).ToString(CultureInfo.InvariantCulture));
                    fromR.Width = initImage.Width;
                    fromR.Height = int.Parse(Math.Floor(initImage.Width / templateRate).ToString(CultureInfo.InvariantCulture));
                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = initImage.Width;
                    toR.Height = int.Parse(Math.Floor(initImage.Width / templateRate).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    pickedImage = new Bitmap(int.Parse(Math.Floor(initImage.Height * templateRate).ToString(CultureInfo.InvariantCulture)), initImage.Height);
                    pickedG = Graphics.FromImage(pickedImage);
                    pickedG.CompositingMode = CompositingMode.SourceCopy;
                    fromR.X = int.Parse(Math.Floor((initImage.Width - initImage.Height * templateRate) / 2).ToString(CultureInfo.InvariantCulture));
                    fromR.Y = 0;
                    fromR.Width = int.Parse(Math.Floor(initImage.Height * templateRate).ToString(CultureInfo.InvariantCulture));
                    fromR.Height = initImage.Height;
                    toR.X = 0;
                    toR.Y = 0;
                    toR.Width = int.Parse(Math.Floor(initImage.Height * templateRate).ToString(CultureInfo.InvariantCulture));
                    toR.Height = initImage.Height;
                }

                pickedG.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);
                var templateImage = new Bitmap(pTemplateWidth, pTemplateHeight);
                var templateG = Graphics.FromImage(templateImage);
                templateG.CompositingMode = CompositingMode.SourceCopy;
                templateG.Clear(Color.White);
                templateG.DrawImage(pickedImage, new Rectangle(0, 0, pTemplateWidth, pTemplateHeight), new Rectangle(0, 0, pickedImage.Width, pickedImage.Height), GraphicsUnit.Pixel);

                outputBitmap = (Bitmap)templateImage.Clone();
                templateG.Dispose();
                templateImage.Dispose();
                pickedG.Dispose();
                pickedImage.Dispose();
            }

            initImage.Dispose();
            return outputBitmap;
        }
    }
}