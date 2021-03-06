using Docker.DotNet;
using Docker.DotNet.Models;
using System.Text;

namespace DockerApiLib
{
    public interface IDockerApi : IDisposable
    {
        Task<IList<ContainerInfo>> GetAllContainersAsync();
        Task<bool> CreateContainerAsync(ContainerCreateData containerCreateData);
        Task<bool> RemoveContainerAsync(ContainerInfo containerInfo);
        Task<IList<ContainerInfo>> GetExitContainerAsync();
        Task GetContainerLogsAsync(Action<string> logEvent, ContainerInfo containerInfo);
        Task ExitedContainerHandler(Action<string> exitedEvent, ContainerInfo containerInfo);
        Task ExecContainerAcync(Action<string> logEvent, ContainerCreateData containerCreateData);
    }

    public class DockerApi : IDockerApi
    {
        private IDockerClient _client;

        public DockerApi(IAddress address)
        {
            _client = new DockerClientConfiguration(
                            new Uri(address.Get()))
                             .CreateClient();
        }

        public async Task<bool> CreateContainerAsync(ContainerCreateData containerCreateData)
        {
            using var waitContainerCts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
            var createContainerResponse = await _client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = containerCreateData.Image,
                    Name = containerCreateData.Name,
                },
                waitContainerCts.Token
            );

            var result = await _client.Containers.StartContainerAsync(
                createContainerResponse.ID,
                new ContainerStartParameters(),
                
                waitContainerCts.Token
            );

            return result;
        }


        public async Task<bool> RemoveContainerAsync(ContainerInfo containerInfo)
        {
            using var containerRemoveCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var result = await _client.Containers.StopContainerAsync(
                    containerInfo.Id,
                    new ContainerStopParameters
                    {
                        WaitBeforeKillSeconds = 0,
                    },
                    containerRemoveCts.Token
                );

            await _client.Containers.RemoveContainerAsync(
               containerInfo.Id,
                new ContainerRemoveParameters
                {
                    Force = true
                },
                containerRemoveCts.Token
            );
            return result;

        }

        public async Task<IList<ContainerInfo>> GetAllContainersAsync()
        {
            using var containerListCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            IList<ContainerListResponse> containerList = await _client.Containers.ListContainersAsync(
                 new ContainersListParameters
                 {
                     All = true
                 },
                 containerListCts.Token
             );

            var result = containerList.Select(x => new ContainerInfo(x.ID, x.Names.FirstOrDefault(), x.State)).ToList();

            return result;
        }


        public async Task<IList<ContainerInfo>> GetExitContainerAsync()
        {
            using var containerListCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            IList<ContainerListResponse> containerList = await _client.Containers.ListContainersAsync(
                 new ContainersListParameters
                 {
                     
                     Filters = new Dictionary<string, IDictionary<string, bool>>
                     {
                         ["status"] = new Dictionary<string, bool>
                         {
                             ["exited"] = true
                         }
                     },
                     All = true
                 },
                 containerListCts.Token
             );

            var result = containerList.Select(x => new ContainerInfo(x.ID, x.Names.FirstOrDefault(), x.State)).ToList();

            return result;
        }

        public async Task GetContainerLogsAsync(Action<string> logEvent, ContainerInfo containerInfo)
        {
            using var containerLogsCts = new CancellationTokenSource();
            containerLogsCts.CancelAfter(TimeSpan.FromSeconds(20));

            var containerLogsTask = _client.Containers.GetContainerLogsAsync(
                containerInfo.Id,
                new ContainerLogsParameters
                {
                    ShowStderr = true,
                    ShowStdout = true,
                    Timestamps = true,
                    Follow = true
                    
                },
                containerLogsCts.Token,
                new Progress<string>((m) => { 
                    logEvent?.Invoke(m); 
                    
                    }
                )
            );

            await containerLogsTask;
        }

        public async Task ExecContainerAcync(Action<string> logEvent, ContainerCreateData containerCreateData)
        {
            using var waitContainerCts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
            var createContainerResponse = await _client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = containerCreateData.Image,
                    Name = containerCreateData.Name,
                },
                waitContainerCts.Token
            );

            var config = new ContainerAttachParameters
            {
                Stdin = false,
                Stdout = true,
                Stderr = true,
                Stream = true
            };

            MultiplexedStream stream = null;
            Task streamTask = null;
            MemoryStream stdOutputStream = new MemoryStream();
            try
            {
                if (config != null)
                {
                    stream = await _client.Containers.AttachContainerAsync(createContainerResponse.ID, false, config, default(CancellationToken));
                    streamTask = stream.CopyOutputToAsync(null, stdOutputStream, null, default(CancellationToken));
                }

                if (!await _client.Containers.StartContainerAsync(createContainerResponse.ID, new ContainerStartParameters()))
                {
                    throw new Exception("The container has already started.");
                }

                if (config != null)
                {
                    await streamTask;
                }
                logEvent?.Invoke("종료됌!");

            }
            finally
            {
                stream?.Dispose();
            }
        }

        //public async Task ExitedContainerHandler(Action<string> exitedEvent, ContainerInfo containerInfo)
        //{
        //    var config = new ContainerLogsParameters
        //    {
        //        ShowStderr = true,
        //        ShowStdout = true,
        //        Timestamps = true,
        //        Follow = true
        //    };
        //    var buffer = new byte[1024];
        //    using (var stream = await _client.Containers.GetContainerLogsAsync(containerInfo.Id, true, config, default(CancellationToken)))
        //    {
        //        var result = await stream.ReadOutputAsync(buffer, 0, buffer.Length, default(CancellationToken));

        //        do
        //        {
        //            Console.Write(Encoding.UTF8.GetString(buffer, 0, result.Count));
        //        }
        //        while (!result.EOF);
        //        exitedEvent(containerInfo.Name);
        //    }
        //}

        public async Task ExitedContainerHandler(Action<string> exitedEvent, ContainerInfo containerInfo)
        {
            
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _client.Dispose();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~DockerApi()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}