using Prism.Events;
using System.IO;

namespace FSWPrism.Events
{
    public class FileSystemEvent : PubSubEvent<FileChangedResult> { }
    public record struct FileChangedResult
    {
        public WatcherChangeTypes ChangeType { get; set; }
        public string? Name { get; set; }
        public string? OldName { get; set; }
    }
}
