using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CSNovelCrawler.Class;


namespace CSNovelCrawler.Core
{
  public class CoreManager
  {
    private readonly ILoggerFactory _loggerFactory;
    /// <summary>
    /// 起始路径
    /// </summary>
    public static string StartupPath { get; set; }
    /// <summary>
    /// 任務管理器
    /// </summary>
    public TaskManager TaskManager { get; private set; }
    /// <summary>
    /// 插件管理器
    /// </summary>
    public PluginManager PluginManager { get; private set; }
    /// <summary>
    /// 配置管理器
    /// </summary>
    public ConfigManager ConfigManager { get; private set; }

    public CoreManager(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
      _loggerFactory = loggerFactory;
      StartupPath = System.Environment.CurrentDirectory;
      ConfigManager = new ConfigManager(loggerFactory, configuration);
      PluginManager = new PluginManager(loggerFactory);
      TaskManager = new TaskManager(loggerFactory, ConfigManager.Settings);
    }
  }
}
