namespace App.WindowsService;

public class FolderService : IFolderService
{
    private readonly ILogger _logger;
    private readonly FolderSerivceConfig _config;
    private readonly FileSystemWatcher _fileSystemWatcher;

    public FolderService(
        ILogger logger,
        IConfiguration config
    )
    {
        _logger = logger;
        _config = config.GetSection("FolderListener").Get<FolderSerivceConfig>();
        _fileSystemWatcher = new FileSystemWatcher(_config.Path)
        {
            Filter = "*.mp3",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.Attributes,
            EnableRaisingEvents = true
        };
    }

    public void StartListening()
    {
        foreach(var eventType in _config.Events)
        {
             switch(eventType)
             {
                case "Create":
                    _fileSystemWatcher.Changed += HandleChangedEvent;
                    break;
                case "Update":
                    _fileSystemWatcher.Created += CreateEventHandler;
                    break;
                case "Delete":
                    _fileSystemWatcher.Deleted += DeleteEventHandler;
                    break;
             }
        }
        _fileSystemWatcher.Renamed += RenameEventHandler;
    }

    private void RenameEventHandler(object sender, RenamedEventArgs e)
    {
        _logger.LogWarning($"Renamed from {e.OldName} to {e.Name}");
    }

    private void DeleteEventHandler(object sender, FileSystemEventArgs e)
    {
        _logger.LogWarning($"{e.Name} has been deleted");
    }

    private void CreateEventHandler(object sender, FileSystemEventArgs e)
    {
        _logger.LogWarning($"{e.Name} has been created");
    }

    private void HandleChangedEvent(object sender, FileSystemEventArgs e)
    {
        _logger.LogWarning($"{e.Name} has been changed");
    }
}

public record FolderSerivceConfig(string Path, string[] Events);