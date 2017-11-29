# Project Description

This module extends the image processing functionality included in Sitecore. Greyscale, RotateFlip, ImageCrop, CenterCrop and KeepOrientation are added.

## About

The image processing functionality currently included in Sitecore is compared to other CMS quiet meager. However, there is a pipeline which can be extended. This module extends the existing image processing functionalities of Sitecore by the following operations:
- Greyscale (transforms an image to greyscale)
- RotateFlip (rotates and/or flips an image on the x/y axis)
- Pixel-precise ImageCrop (crops image out of original image, starting point (x/y value) can be defined)
- CenterCrop (crops image out of the center of the original image adjusting max. width or height based on aspect ratio)
- KeepOrientation (rotates the uploaded file automatically using the orientation-flag of Exif information)

Image processing functionality supported formats are jpg, png and gif.

## Installation Guide

To set up the module, follow these steps:

- Download, Unzip and Compile this solution locally (Verify the existence of Sitecore.Kernel.dll and Sitecore.Client.dll - references)
- Copy the file "Namics.Foundation.ImageProcessor.dll" to your project bin folder
- Add reference "Namics.Foundation.ImageProcessor.dll" to the Project
- Copy the file "\App_Config\Include\Foundation\ImageProcessor\Namics.Foundation.ImageProcessor.config" to the "Project\App_Config" folder
- Start programming ;-)

## Usage

There are different ways to use the ImageProcessor functions.

### By media manager

To use the new image editing features, you have to create an object of class "CustomMediaUrlOptions", which will be passed to the class MediaManager by the method GetMediaUrl. The code below is just an example about the new available parameters. You are also able to use the basic Sitecore parameters in this case (see http://sdn.sitecore.net/Articles/XSL/5%203%20Enhancements/Image%20Enhancements.aspx). It is also allowed to combine the functions when it makes sense. If the crop functions are used, height "Height" and width "Width" must be specified.

    var options = new CustomMediaUrlOptions
        {
            Width = 200,
            Height = 100,
            CenterCrop = true,
            GreyScale = true,
            RotateFlip = RotateFlipType.RotateNoneFlipXY,
            ImageCrop = new CustomMediaUrlOptions.Cropper(10,20)
        };

    var imageField = dataSource.Fields["ImageField"];
    var image = (ImageField) imageField;

    if(image.MediaItem != null)
    {
        var imageUrl = Sitecore.StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(image.MediaItem, options).Replace(" ", "%20"));
        stringBuilder.AppendFormat("<div class='img' style='background-image:url({0}); background-repeat: no-repeat; width:500px;height:400px;'></div>",imageUrl);
    }


Parameter declaration and valid values (Also basic Sitecore parameters are allowed)

| Functionality | Parameter "Code Example" | Value | Remarks |
| --- | --- | --- | --- |
Greyscale | GreyScale = true | true / false | If "true" image will be transformed in greyscale
Flip / Rotate | RotateFlip = RotateFlipType.Rotate180FlipY | RotateNoneFlipNone, Rotate90FlipNone, Rotate180FlipNone, Rotate270FlipNone, RotateNoneFlipX, Rotate90FlipX, Rotate180FlipX, Rotate270FlipX, RotateNoneFlipY, Rotate90FlipY, Rotate180FlipY, Rotate270FlipY, RotateNoneFlipXY, Rotate90FlipXY, Rotate180FlipXY, Rotate270FlipXY | Rotates of flips an image on x/y axis
Pixel-precise cropping | ImageCrop = new CustomMediaUrlOptions.Cropper(int x, int y) | x >= 0, y >= 0, width <= original_width, height <= original_height | If specified value is out of original image's value, cropping won't be applied and the original image will be returned.
Center cropping | CenterCrop = true | true / false | Crops image out of the center of the original image adjusting max. width or height based on aspect ratio.
KeepOrientation | KeepOrientation = true | true / false | If "true" the captured image orientation flag will be considered (see first picture below)

### By Field Renderer

Your are also able to use the new functionalities in both, Sitecore field renderer and Sitecore image tag. In the Sitecore image tag as well as in the field renderer, the individual parameters has to be separated by the "&"-operator. Verify setting the parameter "useCustomFunctions=true" to activate the new functions. It is also allowed to combine the functions when it makes sense. If the crop functions are to be used, height "Height" and width "Width" must be specified.


#### Example "via user control syntax"
    <sc:FieldRenderer FieldName="My Image Field" Parameters="w=100&h=200&useCustomFunctions=1&centerCrop=1" runat="server" />

#### Example "via code behind"

    FieldRenderer.Render(dataSource, "My Image Field", "w=100&h=200&useCustomFunctions=1&centerCrop=1")

Parameter declaration and valid values (Also basic Sitecore parameters are allowed)

| Functionality | Parameter "Code Example" | Value | Remarks |
| --- | --- | --- | --- |
Greyscale | greyScale=1 | 0/1 | If greyScale=1 image will be transformed to greyscale \\
Flip/Rotate | rotateFlip=Rotate180FlipNone | RotateNoneFlipNone, Rotate90FlipNone, Rotate180FlipNone, Rotate270FlipNone, RotateNoneFlipX, Rotate90FlipX, Rotate180FlipX, Rotate270FlipX, RotateNoneFlipY, Rotate90FlipY, Rotate180FlipY, Rotate270FlipY, RotateNoneFlipXY, Rotate90FlipXY, Rotate180FlipXY, Rotate270FlipXY | 
Pixel-precise cropping | w=100&h=150&cropX=0&cropY=0 | Integers (int) | All four parameters have to be specified otherwise the crop function won't be executed. If specified value is out of original image's value, cropping won't be applied and the original image will be returned. In addition to the crop parameters height and width parameters should be determined so that the resulting <img>-tag assumes the size of the cropped framing. w = new image with (starting from X/Y coordinate). h = new image height (starting from X/Y coordinate). startX = startX-Point from the original image. startY= startY-Point from the original image.
Center cropping | centerCrop=1 | 0/1 | Crops image out of the center of the original image adjusting max. width or height based on aspect ratio.
Keep camera orientation | keepOrientation=1 | 0/1 | Considers the orientation attribute of the captured picture (see first picture below)
