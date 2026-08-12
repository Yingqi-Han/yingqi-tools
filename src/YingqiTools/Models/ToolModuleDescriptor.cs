namespace YingqiTools.Models;

internal sealed record ToolModuleDescriptor(
    string Id,
    string Name,
    string Description,
    string IconName,
    Type PageType,
    Func<string> StatusProvider);
