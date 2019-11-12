using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace KaguyaProjectV2.KaguyaBot.Core.Commands.ImageGeneration
{
    public class KaguyaLevelUpImage
    {
        /*
         * The purpose of this class is to (hopefully) create an image, from scratch,
         * that will display various stats on a user, instead of just a plain text embed (fancy, I know).
         */

        public void CreateImage()
        {
            string filePath = $"{Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."))}\\Resources\\Artwork";

            using (Image image = Image.Load(Path.Combine(filePath, "BGTemplate.png")))
            {
            }

            int width = 640;
            int height = 480;

            // Creates a new image with all the pixels set as transparent. 
            using (var image = new Image<Rgba32>(width, height))
            {
            }
        }
    }
}