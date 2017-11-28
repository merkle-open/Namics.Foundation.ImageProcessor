// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetImageFieldValue.cs" company="Namics AG">
//   (c) Namics AG
//
//  Want to work for one of Europe's leading Sitecore Agencies? 
//  Check out http://www.namics.com/jobs/
// </copyright>
// <summary>
//   removes selected parameters for clean html
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Configuration;

namespace Namics.Foundation.ImageProcessor
{
    /// <summary>
    /// Custom Sitecore ImageField
    /// </summary>
    public class GetImageFieldValue : Sitecore.Pipelines.RenderField.GetImageFieldValue
    {
        /// <summary>
        /// Custom Sitecore Image Renderer
        /// </summary>
        public class ImageRenderer : Sitecore.Xml.Xsl.ImageRenderer
        {
            /// <summary>
            /// The xhtml field.
            /// </summary>
            protected bool Xhtml;

            /// <summary>
            /// Get the Source
            /// </summary>
            /// <returns>
            /// The Source String <see cref="string"/>.
            /// </returns>
            protected override string GetSource()
            {
                var baseUrl = new Sitecore.Text.UrlString(base.GetSource().Replace("&amp;", "&"));

                if (Parameters.ContainsKey("useCustomFunctions"))
                {
                    baseUrl.Add("useCustomFunctions", Parameters["useCustomFunctions"]);

                    // remove useNamicsFuctions from the parameters dictionary so it's not added as an attribute on the img tag 
                    Parameters.Remove("useCustomFunctions");
                }

                if (Parameters.ContainsKey("centerCrop"))
                {
                    baseUrl.Add("centerCrop", Parameters["centerCrop"]);

                    // remove centerCrop from the parameters dictionary so it's not added as an attribute on the img tag 
                    Parameters.Remove("centerCrop");
                }

                if(Parameters.ContainsKey("cropX") && Parameters.ContainsKey("cropY"))
                {
                    baseUrl.Add("cropX", Parameters["cropX"]);
                    baseUrl.Add("cropY", Parameters["cropY"]);

                    // remove cropStartX, cropStartY, cropWidth and cropHeight from the parameters dictionary so they not added as attributes on the img tag
                    Parameters.Remove("cropX");
                    Parameters.Remove("cropY");
                }

                if (Parameters.ContainsKey("keepOrientation"))
                {
                    baseUrl.Add("keepOrientation", Parameters["keepOrientation"]);

                    // remove keepOrientation from the parameters dictionary so it's not added as an attribute on the img tag
                    Parameters.Remove("keepOrientation");
                }

                if (Parameters.ContainsKey("greyScale"))
                {
                    baseUrl.Add("greyScale", Parameters["greyScale"]);

                    // remove greyScale from the parameters dictionary so it's not added as an attribute on the img tag
                    Parameters.Remove("greyScale");
                }

                if (Parameters.ContainsKey("rotateFlip"))
                {
                    baseUrl.Add("rotateFlip", Parameters["rotateFlip"]);

                    // remove rotateFlip from the parameters dictionary so it's not added as an attribute on the img tag
                    Parameters.Remove("rotateFlip");
                }
                return baseUrl.GetUrl(Xhtml && Settings.Rendering.ImagesAsXhtml);
            }

            /// <summary>
            /// xhtml on the base is private
            /// </summary>
            /// <param name="pAttributes"></param>
            protected override void ParseNode(Sitecore.Collections.SafeDictionary<string> pAttributes)
            {
                var str = Extract(pAttributes, new[] { "outputMethod" });
                Xhtml = str == "xhtml" || Settings.Rendering.ImagesAsXhtml && str != "html";
                base.ParseNode(pAttributes);
            }
        }

        /// <summary>
        /// Initialize a new ImageRenderer
        /// </summary>
        /// <returns>
        /// The <see cref="ImageRenderer"/>.
        /// </returns>
        protected override Sitecore.Xml.Xsl.ImageRenderer CreateRenderer()
        {
            return new ImageRenderer();
        }
    }
}
