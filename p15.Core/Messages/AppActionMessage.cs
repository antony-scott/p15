namespace p15.Core.Messages
{
    public enum AppAction
    {
        BrowseSource,
        Go,
        Sync,
        OpenIDE,
        MessageQueues
    }

    public class AppActionMessage
    {
        public AppAction Action { get; }
        public string PackageName { get; }
        public string AppName { get; }

        public AppActionMessage(AppAction action, string packageName, string appName)
        {
            Action = action;
            PackageName = packageName;
            AppName = appName;
        }
    }
}
