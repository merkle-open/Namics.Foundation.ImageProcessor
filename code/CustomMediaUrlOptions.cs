// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomMediaUrlOptions.cs" company="Namics AG">
//   (c) Namics AG
//
//  Want to work for one of Europe's leading Sitecore Agencies? 
//  Check out http://www.namics.com/jobs/
// </copyright>
// <summary>
//   Generates the parameter-string
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Drawing;
using System.Globalization;

using Sitecore.Resources.Media;
using Sitecore.Text;

namespace Namics.Foundation.ImageProcessor
{
    /// <summary>
    /// Custom Url-Options Class
    /// </summary>
    public class CustomMediaUrlOptions : MediaUrlOptions
    {
        /// <summary>
        /// The use namics functions.
        /// </summary>
        public bool UseCustomFunctions { get; set; }

        /// <summary>
        /// The grey scale.
        /// </summary>
        public bool GreyScale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether keep orientation.
        /// </summary>
        public bool KeepOrientation { get; set; }

        /// <summary>
        /// The mirror image.
        /// </summary>
        public bool MirrorImage { get; set; }

        /// <summary>
        /// The center crop.
        /// </summary>
        public bool CenterCrop { get; set; }

        /// <summary>
        /// The rotate flip.
        /// </summary>
        public RotateFlipType RotateFlip { get; set; }

        /// <summary>
        /// The image crop.
        /// </summary>
        public Cropper ImageCrop { get; set; }

        /// <summary>
        /// The _m crop start x.
        /// </summary>
        private static int _mCropStartX;

        /// <summary>
        /// The _m crop start y.
        /// </summary>
        private static int _mCropStartY;

        /// <summary>
        /// The cropper.
        /// </summary>

        public CustomMediaUrlOptions()
        {
            UseCustomFunctions = true;
        }
        
        public class Cropper
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Cropper"/> class.
            /// </summary>
            /// <param name="pCropStartX">
            /// The p crop start x.
            /// </param>
            /// <param name="pCropStarty">
            /// The p crop start y.
            /// </param>
            public Cropper(int pCropStartX, int pCropStarty)
            {
                _mCropStartX = pCropStartX;
                _mCropStartY = pCropStarty;
            }
        }

        /// <summary>
        /// Overrides the ToString() method from the base class MediaUrlOptions
        /// </summary>
        /// <returns>The manipulated string</returns>
        public override string ToString()
        {
            if (UseCustomFunctions)
            {
                var urlString = new UrlString();
                urlString.Add("useCustomFunctions", 1.ToString(CultureInfo.InvariantCulture));

                if (KeepOrientation)
                {
                    urlString.Add("keepOrientation", 1.ToString(CultureInfo.InvariantCulture));
                }

                if (GreyScale)
                {
                    urlString.Add("greyScale", 1.ToString(CultureInfo.InvariantCulture));
                }

                if (!RotateFlip.ToString().Equals("RotateNoneFlipNone"))
                {
                    urlString.Add("rotateFlip", RotateFlip.ToString());
                }

                if (CenterCrop)
                {
                    urlString.Add("centerCrop", 1.ToString(CultureInfo.InvariantCulture));
                }

                if (ImageCrop != null)
                {
                    urlString.Add("cropX", _mCropStartX.ToString(CultureInfo.InvariantCulture));
                    urlString.Add("cropY", _mCropStartY.ToString(CultureInfo.InvariantCulture));
                }

                string finalString;
                if (!string.IsNullOrEmpty(base.ToString()))
                {
                    finalString = "&" + urlString;
                }
                else
                {
                    finalString = urlString.ToString();
                }

                return base.ToString() + finalString;
            }

            return base.ToString();
        }
    }
}
