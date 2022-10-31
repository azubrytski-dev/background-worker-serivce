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
        var cfg = config.GetSection(sectionName);
        _config = config.GetSection(sectionName).Get<FolderSerivceConfig>();
        _fileSystemWatcher = new FileSystemWatcher(_config.Path)
        {
            Filter = "*.mp3",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.Attributes,
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
        _logger.LogWarning($"{e.Name} has been changed");
    }
}

public class FolderSerivceConfig//(string Path, string[] Events)
{
    public string Path { get; set; }
    public string[] Events { get; set; }
}