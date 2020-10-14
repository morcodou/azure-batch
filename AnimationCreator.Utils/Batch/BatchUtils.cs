using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationCreator.Utils.Batch
{
    public class BatchUtils
    {

        BatchClient _batchClient;


        public BatchUtils(string batchAccountUrl, string batchAccountName, string batchAccountKey)
        {
            // ToDo: Create the BatchClient.
            var credential = new BatchSharedKeyCredentials(batchAccountUrl, batchAccountName, batchAccountKey);
            _batchClient = BatchClient.Open(credential);
        }


        public async Task CreatePoolIfNotExistAsync(string poolId)
        {
            try
            {
                Console.WriteLine("Creating pool [{0}]...", poolId);

                Func<ImageReference, bool> imageScanner = imageRef =>
                    imageRef.Publisher == "MicrosoftWindowsServer" &&
                    imageRef.Offer == "WindowsServer" &&
                    imageRef.Sku.Contains("2012-R2-Datacenter");

                var skuAndImage = await GetNodeAgentSkuReferenceAsync(imageScanner);

                // ToDo: Create the Pool
                var pool = _batchClient.PoolOperations.CreatePool(poolId: poolId,

                    // ToDo: Specify the VM size
                    //virtualMachineSize: "standard_d1_v2",
                    //targetDedicatedComputeNodes: 1,


                    // Four Core VMS
                    virtualMachineSize: "standard_d3_v2",
                    targetDedicatedComputeNodes: 4,

                    // ToDo: Create a VM configuration
                    virtualMachineConfiguration: new VirtualMachineConfiguration(
                        skuAndImage.Image, skuAndImage.Sku.Id)


                );

                // With Four core VMS
                pool.MaxTasksPerComputeNode = 4;
                pool.TaskSchedulingPolicy = new TaskSchedulingPolicy(ComputeNodeFillType.Spread);


                // ToDo: Add the application reference
                pool.ApplicationPackageReferences = new List<ApplicationPackageReference>
                {
                    new ApplicationPackageReference{ ApplicationId = "PolyRay", Version = "1" }
                };


                // ToDo: Commit the changes
                await pool.CommitAsync();


            }
            catch (BatchException be)
            {
                if (be.RequestInformation?.BatchError != null && be.RequestInformation.BatchError.Code == BatchErrorCodeStrings.PoolExists)
                {
                    Console.WriteLine("The pool {0} already existed when we tried to create it", poolId);
                }
                else
                {
                    throw;
                }
            }
        }


        public async Task CreateJobAsync(string jobId, string poolId)
        {
            Console.WriteLine("Creating job [{0}]...", jobId);

            // ToDo: Create the job definition
            CloudJob job = _batchClient.JobOperations.CreateJob();
            job.Id = jobId;


            // ToDo: Specify the pool
            job.PoolInformation = new PoolInformation { PoolId = poolId };


            // ToDo: Set job to completed state when all tasks are complete
            job.OnAllTasksComplete = OnAllTasksComplete.TerminateJob;


            // ToDo: Commit the job
            await job.CommitAsync();
        }




        public async Task<List<CloudTask>> AddTasksAsync(string jobId, List<ResourceFile> sceneFiles, string outputContainerSasUrl)
        {
            Console.WriteLine("Adding {0} tasks to job [{1}]...", sceneFiles.Count, jobId);
            List<CloudTask> tasks = new List<CloudTask>();
            foreach (ResourceFile sceneFile in sceneFiles)
            {
                string taskId = "task" + sceneFiles.IndexOf(sceneFile).ToString().PadLeft(6, '0');

                var sceneFileName = sceneFile.FilePath;
                var imageFileName = sceneFileName.Replace(".pi", ".tga");

                string taskCommandLine = $"cmd /c %AZ_BATCH_APP_PACKAGE_POLYRAY%\\polyray.exe {sceneFileName} -o {imageFileName}";

                // ToDo: Create the task and specify the input file
                CloudTask task = new CloudTask(taskId, taskCommandLine);
                task.ResourceFiles = new List<ResourceFile> { sceneFile };


                // ToDo: Add the output file definition
                task.OutputFiles = new List<OutputFile>
                {
                    new OutputFile(
                        filePattern: imageFileName,
                        destination: new OutputFileDestination(new OutputFileBlobContainerDestination(
                            containerUrl: outputContainerSasUrl,
                            path: taskId + $@"\{imageFileName}")),
                        uploadOptions: new OutputFileUploadOptions(OutputFileUploadCondition.TaskCompletion))

                };


                // ToDo: Add the task to the collection
                tasks.Add(task);

            }


            // ToDo: Add the tasks collection to the job
            await _batchClient.JobOperations.AddTaskAsync(jobId, tasks);


            return tasks;
        }



        private async Task<SkuAndImage> GetNodeAgentSkuReferenceAsync(Func<ImageReference, bool> scanFunc)
        {
            List<NodeAgentSku> nodeAgentSkus = await _batchClient.PoolOperations.ListNodeAgentSkus().ToListAsync();

            NodeAgentSku nodeAgentSku = nodeAgentSkus.First(sku => sku.VerifiedImageReferences.FirstOrDefault(scanFunc) != null);
            ImageReference imageReference = nodeAgentSku.VerifiedImageReferences.First(scanFunc);

            return new SkuAndImage(nodeAgentSku, imageReference);
        }
    }
}
