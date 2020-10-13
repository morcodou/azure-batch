using AnimationCreator.Utils.Storage;
using Microsoft.Azure.Batch;
using Microsoft.WindowsAzure.Storage.Blob;

using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace AnimationCreator.Utils.Batch
{
    public class JobHelper
    {

        private string m_BatchAccountUrl;
        private string m_BatchAccountName;
        private string m_BatchAccountKey;

        private BatchUtils m_BatchUtils;


        public JobHelper(string batchAccountUrl, string batchAccountName, string batchAccountKey)
        {           
            m_BatchAccountUrl = batchAccountUrl;
            m_BatchAccountName = batchAccountName;
            m_BatchAccountKey = batchAccountKey;

            m_BatchUtils = new BatchUtils(batchAccountUrl, batchAccountName, batchAccountKey);
        }

        public void CreateRenderPool(string poolId)
        {
            m_BatchUtils.CreatePoolIfNotExistAsync(poolId).Wait();
        }

        public void CreateJob(string poolId, string jobId)
        {
            m_BatchUtils.CreateJobAsync(jobId, poolId).Wait();
        }

        public void AddTasks(string jobId, int numberOfTasks)
        {
            var storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            var frameBlobContainerName = ConfigurationManager.AppSettings["FrameBlobContainerName"];
            var sceneBlobContainerName = ConfigurationManager.AppSettings["SceneBlobContainerName"];

            var frameBlobHelper = new BlobHelper(storageConnectionString, frameBlobContainerName, 1);
            var sceneBlobHelper = new BlobHelper(storageConnectionString, sceneBlobContainerName, 10);

            var outputContainerSasUrl = frameBlobHelper.GetContainerSasUrl(SharedAccessBlobPermissions.Write);

            // Get the scene resource files files
            var sceneResourceFiles = new List<ResourceFile>();
            for (int frameNumber = 0; frameNumber < numberOfTasks; frameNumber++)
            {
                string blobName = $"F{ frameNumber.ToString().PadLeft(6, '0') }.pi";
                var sceneResourceFile = sceneBlobHelper.GetResourceFile(blobName);
                sceneResourceFiles.Add(sceneResourceFile);
            }

            // Add the tasks
            var tasks = m_BatchUtils.AddTasksAsync(jobId, sceneResourceFiles, outputContainerSasUrl).Result;


        }


    }
}
