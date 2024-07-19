using System.Threading.Tasks;

namespace FSWPrism.Interfaces
{
    public interface IFSWService
    {
        Task StartAsync();
        Task StopAsync();
        Task SetDirectoryPathAsync(string path);
        string Path { get; }
    }
}
