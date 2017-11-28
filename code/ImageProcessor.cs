// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessor.cs" company="Namics AG">
//   (c) Namics AG
//
//  Want to work for one of Europe's leading Sitecore Agencies? 
//  Check out http://www.namics.com/jobs/
// </copyright>
// <summary>
//   Defines the controller. Writes the manipulated image back to the stream.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

namespace Namics.Foundation.ImageProcessor
{
    /// <summary>
    /// Performs image manipulation on the fly, hooking into the Sitecore getMediaStream pipeline
    /// </summary>
    public class ImageProcessor
    {
        /// <summary>
        /// Called by Sitecore getMediaStream processor
        /// </summary>
        /// <param name="pArgs">Args from Sitecore getMediaStream pipeline containing image to process</param>
        public void Process(GetMediaStreamPipelineArgs pArgs)
        {
            // variable value should be "1" to activate namics image manipulation functionality
            var useCustomFunctionsArg = pArgs.Options.CustomOptions["useCustomFunctions"];
            if (string.IsNullOrEmpty(useCustomFunctionsArg))
            {
                return;
            }

            if (!useCustomFunctionsArg.Equals("1"))
            {
                return;
            }

            try
            {
                var bm = (Bitmap)Image.FromStream(pArgs.OutputStream.Stream);
                
                int startXArg, startYArg;
                var isStartXParsable = int.TryParse(pArgs.Options.CustomOptions["cropX"], out startXArg);
                var isStartYParsable = int.TryParse(pArgs.Options.CustomOptions["cropY"], out startYArg);

                // keepOrientatioln parameter from url
                var keepOrientationArg = pArgs.Options.CustomOptions["keepOrientation"];
                if (!string.IsNullOrEmpty(keepOrientationArg))
                {
                    if (keepOrientationArg.Equals("1"))
                    {
                        foreach (var orientation in from prop in bm.PropertyItems where prop.Id == 0x0112 select prop.Value[0])
                        {
                            string processString;

                            switch (orientation)
                            {
                                case 1:
                                    processString = "RotateNoneFlipNone";
                                    break;
                                case 2:
                                    processString = "RotateNoneFlipX";
                                    break;
                                case 3:
                                    processString = "Rotate180FlipNone";
                                    break;
                                case 4:
                                    processString = "Rotate180FlipX";
                                    break;
                                case 5:
                                    processString = "Rotate90FlipX";
                                    break;
                                case 6:
                                    processString = "Rotate90FlipNone";
                                    break;
                                case 7:
                                    processString = "Rotate270FlipX";
                                    break;
                                case 8:
                                    processString = "Rotate270FlipNone";
                                    break;
                                default:
                                    processString = "RotateNoneFlipNone";
                                    break;
                            }

                            if (!string.IsNullOrEmpty(processString))
                            {
                                bm = ProcessRotateFlip(bm, processString);
                            }
                        }
                    }
                }

                // Process pixel accurate cropping whenn all boolparameters are true
                if (isStartXParsable && isStartYParsable && pArgs.Options.Height > 0 && pArgs.Options.Width > 0)
                {
                    bm = ProcessCropping(bm, startXArg, startYArg, pArgs.Options.Width, pArgs.Options.Height);                  
                }

                // only crop image if we have a custom width and height (and crop=1)
                var crop = pArgs.Options.CustomOptions["centerCrop"];
                if (crop != null)
                {
                    var originalProportion = ((double)bm.Width) / bm.Height;
                    var argumentProportion = ((double)pArgs.Options.Width) / pArgs.Options.Height;

                    if(!originalProportion.Equals(argumentProportion))
                    {
                        if (crop.Equals("1") && pArgs.Options.Height > 0 && pArgs.Options.Width > 0)
                        {
                            bm = ProcessCenterCropping(pArgs.Options.Width, pArgs.Options.Height, bm);
                        }
                   }
                }

                // greyScale parameter from url
                var greyScaleArg = pArgs.Options.CustomOptions["greyScale"];
                if (!string.IsNullOrEmpty(greyScaleArg))
                {
                    if (greyScaleArg.Equals("1"))
                    {
                        bm = ProcessGreyScale(bm);
                    }
                }

                // rotate flip parameter from url
                var rotateFlip = pArgs.Options.CustomOptions["rotateFlip"];

                if (!string.IsNullOrEmpty(rotateFlip))
                {
                    bm = ProcessRotateFlip(bm, rotateFlip);
                }

                /**********************************************
                 *  Add more image processing options here.. 
                **********************************************/

                // Write manipulatet bitmap back to the Stream
                CreateMediaStream(pArgs, bm);
            }
            catch (Exception e)
            {
                Log.Error("Error while formatting image" + e.Message, typeof(ImageProcessor));
            }
        }

        /// <summary>
        /// The process center cropping.
        /// </summary>
        /// <param name="pTemplateWidth">
        /// The template width.
        /// </param>
        /// <param name="pTemplateHeight">
        /// The template height.
        /// </param>
        /// <param name="pBitmap">
        /// The bitmap for manipulation.
        /// </param>
        /// <returns>
        /// The manipulated image <see cref="Bitmap"/>.
        /// </returns>
        private Bitmap ProcessCenterCropping(int pTemplateWidth, int pTemplateHeight, Bitmap pBitmap)
        {
            return ImageFunctions.CenterCropImage(pTemplateWidth, pTemplateHeight, pBitmap);
        }

        /// <summary>
        /// Processes the incoming media stream and converts it to greyscale
        /// </summary>
        /// <param name="pBitmap">The bitmap for manipulation</param>
        private Bitmap ProcessGreyScale(Bitmap pBitmap)
        {
            return ImageFunctions.MakeGreyscale(pBitmap);
        }

        /// <summary>
        /// Processes the incoming Media Stream and cropps the original bitmap
        /// </summary>
        /// <param name="pBitmap">the bitmap for manipulation</param>
        /// <param name="pStartX">Cropping Start-X-Point</param>
        /// <param name="pStartY">Cropping Start-Y-Point</param>
        /// <param name="pWidth">Cropping Width</param>
        /// <param name="pHeight">Cropping Height</param>
        private Bitmap ProcessCropping(Bitmap pBitmap, int pStartX, int pStartY, int pWidth, int pHeight)
        {
            return ImageFunctions.CropImage(pBitmap, pStartX, pStartY, pWidth, pHeight);
        }

        /// <summary>
        /// Processes the incoming Media Stream and make flip/rotate operations on the bitmap
        /// </summary>
        /// <param name="pBitmap">the bitmape for manipulation</param>
        /// <param name="pRotateFlip"></param>
        private Bitmap ProcessRotateFlip(Bitmap pBitmap, string pRotateFlip)
        {
            var type = RotateFlipType.RotateNoneFlipNone;
            switch (pRotateFlip)
            {
                case "Rotate180FlipNone":
                    type = RotateFlipType.Rotate180FlipNone;
                    break;
                case "Rotate180FlipX":
                    type = RotateFlipType.Rotate180FlipX;
                    break;
                case "Rotate180FlipXY":
                    type = RotateFlipType.Rotate180FlipXY;
                    break;
                case "Rotate180FlipY":
                    type = RotateFlipType.Rotate180FlipY;
                    break;
                case "Rotate270FlipNone":
                    type = RotateFlipType.Rotate270FlipNone;
                    break;
                case "Rotate270FlipX":
                    type = RotateFlipType.Rotate270FlipX;
                    break;
                case "Rotate270FlipXY":
                    type = RotateFlipType.Rotate270FlipXY;
                    break;
                case "Rotate270FlipY":
                    type = RotateFlipType.Rotate270FlipY;
                    break;
                case "Rotate90FlipNone":
                    type = RotateFlipType.Rotate90FlipNone;
                    break;
                case "Rotate90FlipX":
                    type = RotateFlipType.Rotate90FlipX;
                    break;
                case "Rotate90FlipXY":
                    type = RotateFlipType.Rotate90FlipXY;
                    break;
                case "Rotate90FlipY":
                    type = RotateFlipType.Rotate90FlipY;
                    break;
                case "RotateNoneFlipX":
                    type = RotateFlipType.RotateNoneFlipX;
                    break;
                case "RotateNoneFlipXY":
                    type = RotateFlipType.RotateNoneFlipXY;
                    break;
                case "RotateNoneFlipY":
                    type = RotateFlipType.RotateNoneFlipY;
                    break;
            }
            return ImageFunctions.RotateFlipImage(pBitmap, type);
        }

        /// <summary>
        /// Builds the outputstream (new Image) with the propriate format from the selection (png, jpg, gif);
        /// </summary>
        /// <param name="pArgs">TheMediaStreamPipline argument</param>
        /// <param name="pBitmap">The manipulated bitmap</param>
        private void CreateMediaStream(GetMediaStreamPipelineArgs pArgs, Bitmap pBitmap)
        {
            if (pArgs.OutputStream != null && pArgs.OutputStream.Stream != null)
            {
                var stream = new MemoryStream();
                var format = pArgs.OutputStream.Extension;
                var flag = false;

                switch (format)
                {
                    case "jpg":
                        pBitmap.Save(stream, ImageFormat.Jpeg);
                        break;
                    case "png":
                        pBitmap.Save(stream, ImageFormat.Png);
                        break;
                    case "gif":
                        pBitmap.Save(stream, ImageFormat.Gif);
                        break;
                    default:
                        pBitmap.Save(stream, ImageFormat.Jpeg);
                        flag = true;
                        break;
                }

                pArgs.OutputStream = flag ? new MediaStream(stream, "jpg", pArgs.MediaData.MediaItem) : new MediaStream(stream, pArgs.OutputStream.Extension, pArgs.MediaData.MediaItem);
            }
        }
    }
}
