using Avalonia;
using Polyvalent.Data;

namespace Polyvalent.Desktop;

internal static class Program
{
    internal static string DatabasePath { get; private set; } = DatabasePathResolver.Resolve();

    [STAThread]
    public static void Main(string[] args)
    {
        DatabasePath = ResolveDatabasePath(args);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static string ResolveDatabasePath(string[] args)
    {
        string? explicitDatabase = null;
        string? explicitDataDirectory = null;
        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] is not ("--db" or "--data-dir"))
            {
                continue;
            }

            if (index + 1 >= args.Length)
            {
                throw new ArgumentException($"option '{args[index]}' requires a value.");
            }

            if (args[index] == "--db")
            {
                explicitDatabase = args[++index];
            }
            else
            {
                explicitDataDirectory = args[++index];
            }
        }

        if (explicitDatabase is not null && explicitDataDirectory is not null)
        {
            throw new ArgumentException("use --db or --data-dir, not both.");
        }

        return DatabasePathResolver.Resolve(explicitDatabase, explicitDataDirectory);
    }
}
