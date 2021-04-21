using Nethermind.Config;

namespace Nethermind.Pipeline
{
    [ConfigCategory(HiddenFromDocs = true)]
    public interface IPipelinePluginsConfig : IConfig
    {
        [ConfigItem(Description = "List of enabled pipeline plugins")]
        string[] Enabled { get; set; }
    }
}
