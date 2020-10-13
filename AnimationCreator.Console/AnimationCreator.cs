using AnimationCreator.Utils.Animation;
using AnimationCreator.Utils.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationCreator.Console
{
    class AnimationCreator
    {
        private static int ErrorFrameRate = 5;

        public int Width { get; set; }
        public int Height { get; set; }
        public int Frames { get; set; }

        public bool GenerateErrors { get; set; }


        public AnimationCreator(int width, int height)
        {
            Width = width;
            Height = height;
        }


        public void CreateSceneFiles()
        {
            var pinAnimation = new PinAnimation(Width, Height);

            pinAnimation.CameraY = 4;
            pinAnimation.CameraAtX = 0;
            pinAnimation.CameraAtY = 2;
            pinAnimation.CameraAtZ = 0;

            var storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            var sceneBlobContainerName = ConfigurationManager.AppSettings["SceneBlobContainerName"];
            var frameBlobContainerName = ConfigurationManager.AppSettings["FrameBlobContainerName"];

            var blobHelper = new BlobHelper(storageConnectionString, sceneBlobContainerName, 10);
            var frameBlobHelper = new BlobHelper(storageConnectionString, frameBlobContainerName, 10);

            for (int frame = 0; frame < Frames; frame++)
            {
                var depthFileNamePart = (frame % 100).ToString().PadLeft(6, '0');
                var fileNamePart = frame.ToString().PadLeft(6, '0');

                var imageFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Depth\A{depthFileNamePart}.png");
                pinAnimation.SetPinDepth(imageFile);
                pinAnimation.AdvanceFrame();
                var sceneDescription = pinAnimation.ToString();

                string fileName = $"F{fileNamePart}.pi";

                if (GenerateErrors && frame % ErrorFrameRate == 0)
                {
                    blobHelper.UploadText(fileName, "SCENE FILE ERROR\r\n" + sceneDescription);
                }
                else
                {
                    blobHelper.UploadText(fileName, sceneDescription);
                }

            }
            System.Console.WriteLine("Complete!");

        }

    }
}
