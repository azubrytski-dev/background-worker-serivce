namespace FolderListener.Background.Services;

public class FolderService : IFolderService
{
    private readonly ILogger<FolderService> _logger;
    private readonly FolderSerivceConfig _config;
    private readonly FileSystemWatcher _fileSystemWatcher;

    public FolderService(
        ILogger<FolderService> logger,
        IConfiguration config
    )
    {
        _logger = logger;
        var sectionName = typeof(FolderService).Name;
        _config = config.GetSection(sectionName).Get<FolderSerivceConfig>();
        _fileSystemWatcher = new FileSystemWatcher(_config.Path)
        {
            Filter = _config.Filter,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.Attributes | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };
    }

    public void StartListening()
    {
        foreach (var eventType in _config.Events)
        {
            switch (eventType)
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
        if (e.ChangeType != WatcherChangeTypes.Changed) return;

        _logger.LogWarning($"{e.Name} has been changed");
    }
}

public class FolderSerivceConfig
{
    public string Path { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
    public string[] Events { get; set; } = Array.Empty<string>();
}