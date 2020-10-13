using Microsoft.Azure.Batch;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnimationCreator.Utils.Storage
{
    public class BlobHelper
    {
        private int m_MaxParallelThreads;
        private CloudBlobContainer m_BlobContainer;
        private static Semaphore m_Semaphore;

        public BlobHelper(string storageConnection, string blobContainerName, int maxParallelThreads)
        {
            m_MaxParallelThreads = maxParallelThreads;

            m_BlobContainer = CloudStorageAccount.Parse(storageConnection).CreateCloudBlobClient().GetContainerReference(blobContainerName);
            m_BlobContainer.CreateIfNotExists();

            m_Semaphore = new Semaphore(maxParallelThreads, maxParallelThreads);
        }

        public void UploadText(string blobName, string content)
        {
            var blob = m_BlobContainer.GetBlockBlobReference(blobName);

            m_Semaphore.WaitOne();
            var blobTask = blob.UploadTextAsync(content).ContinueWith(t =>
            {
                m_Semaphore.Release();
                Console.Write(".");
            });

        }

        public void UploadFile(string filePath)
        {
            var blobName = Path.GetFileName(filePath);

            m_BlobContainer.GetBlockBlobReference(blobName).UploadFromFile(filePath);

        }

        public ResourceFile GetResourceFile(string blobName)
        {
            // Create a shared access signature for the blob
            var blob = m_BlobContainer.GetBlockBlobReference(blobName);
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7),
                Permissions = SharedAccessBlobPermissions.Read
            };
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);
            string blobSasUri = String.Format("{0}{1}", blob.Uri, sasBlobToken);

            return new ResourceFile(blobSasUri, blobName);
        }

        public string GetContainerSasUrl(SharedAccessBlobPermissions permissions)
        {
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7),
                Permissions = permissions
            };

            string sasContainerToken = m_BlobContainer.GetSharedAccessSignature(sasConstraints);

            return String.Format("{0}{1}", m_BlobContainer.Uri, sasContainerToken);
        }


        public void DownloadFrames(string folderName, int count)
        {
            for (int frameNumber = 0; frameNumber < count; frameNumber++)
            {
                var blobName = $"task{ frameNumber.ToString().PadLeft(6, '0') }/{ GetFrameName(frameNumber) }";
                var blob = m_BlobContainer.GetBlockBlobReference(blobName);

                m_Semaphore.WaitOne();
                blob.DownloadToFileAsync(Path.Combine(folderName, GetFrameName(frameNumber)), FileMode.Create).ContinueWith(t =>
                {
                    m_Semaphore.Release();
                    Console.Write(".");
                });
            }
        }


        string GetFrameName(int frameNumber)
        {
            return $"F{frameNumber.ToString().PadLeft(6, '0') }.tga";
        }




    }
}
