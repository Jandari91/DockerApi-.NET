using Docker.DotNet;
using Docker.DotNet.Models;

namespace DockerApiLib
{
    public interface IDockerApi : IDisposable
    {
        Task<IList<ContainerInfo>> GetAllContainersAsync();
        Task<bool> CreateContainerAsync(ContainerCreateData containerCreateData);
        Task<bool> RemoveContainerAsync(ContainerInfo containerInfo);

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
                containerInfo.Name,
                new ContainerStopParameters
                {
                    WaitBeforeKillSeconds = 0
                },
                containerRemoveCts.Token
            );

            await _client.Containers.RemoveContainerAsync(
                containerInfo.Name,
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

            //var result = containerList.Select(x => new ContainerInfo(x.ID, x.Names))
            var result = containerList.Select(x => new ContainerInfo(x.ID, x.Names.FirstOrDefault(), x.State)).ToList();


            return result;
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