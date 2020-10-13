using AnimationCreator.Utils.Batch;
using AnimationCreator.Utils.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnimationCreator.Console
{
    class AnimationCreatorConsole
    {


        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 100;

            int width = 1280;
            int height = 720;

            int numberOfFrames = 10;
            string poolName = "RenderPool";
            string jobName = "RenderJob";

            //// Create the Animation and upload to Blob storage
            System.Console.WriteLine("Press enter to CreateTestAnimation...");
            System.Console.ReadLine();
            CreateTestAnimation(width, height, numberOfFrames, generateErrors: false);
            System.Console.WriteLine("CreateTestAnimation complete!");

            // Create the Pool
            System.Console.WriteLine("Press enter to CreatePool...");
            System.Console.ReadLine();
            CreatePool(poolName);
            System.Console.WriteLine("CreatePool complete!");

            //// Create the Azure Batch job
            System.Console.WriteLine("Press enter to CreateJob...");
            System.Console.ReadLine();
            CreateJob(poolName, jobName);
            System.Console.WriteLine("CreateJob complete!");

            //// Add tasks to the job
            System.Console.WriteLine("Press enter to AddTasks...");
            System.Console.ReadLine();
            AddTasks(jobName, numberOfFrames);
            System.Console.WriteLine("AddTasks complete!");

            // Download the animation frames
            System.Console.WriteLine("Press enter to DownloadFrames...");
            System.Console.ReadLine();
            DownloadFrames(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Animation"), numberOfFrames);
            System.Console.WriteLine("DownloadFrames complete!");

            System.Console.WriteLine("Demo Complete!");
            System.Console.ReadLine();
        }


        static void CreateTestAnimation(int width, int height, int frames, bool generateErrors)
        {
            var animationCreator = new AnimationCreator(width, height);
            animationCreator.Frames = frames;
            animationCreator.GenerateErrors = generateErrors;

            animationCreator.CreateSceneFiles();
        }


        static void CreatePool(string poolId)
        {
            var batchAccountUrl = ConfigurationManager.AppSettings["BatchAccountUrl"];
            var batchAccountName = ConfigurationManager.AppSettings["BatchAccountName"];
            var batchAccountKey = ConfigurationManager.AppSettings["BatchAccountKey"];


            var jobHelper = new JobHelper(batchAccountUrl, batchAccountName, batchAccountKey);
            jobHelper.CreateRenderPool(poolId);

        }
        static void CreateJob(string poolId, string jobId)
        {
            var batchAccountUrl = ConfigurationManager.AppSettings["BatchAccountUrl"];
            var batchAccountName = ConfigurationManager.AppSettings["BatchAccountName"];
            var batchAccountKey = ConfigurationManager.AppSettings["BatchAccountKey"];


            var jobHelper = new JobHelper(batchAccountUrl, batchAccountName, batchAccountKey);
            jobHelper.CreateJob(poolId, jobId);
        }

        static void AddTasks(string jobId, int numberOfTasks)
        {
            var batchAccountUrl = ConfigurationManager.AppSettings["BatchAccountUrl"];
            var batchAccountName = ConfigurationManager.AppSettings["BatchAccountName"];
            var batchAccountKey = ConfigurationManager.AppSettings["BatchAccountKey"];

            var jobHelper = new JobHelper(batchAccountUrl, batchAccountName, batchAccountKey);
            jobHelper.AddTasks(jobId, numberOfTasks);
        }



        static void DownloadFrames(string folderName, int count)
        {
            var storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            var frameBlobContainerName = ConfigurationManager.AppSettings["FrameBlobContainerName"];

            var blobHelper = new BlobHelper(storageConnectionString, frameBlobContainerName, 10);
            blobHelper.DownloadFrames(folderName, count);
        }

    }
}
