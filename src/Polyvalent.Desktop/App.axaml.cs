using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Polyvalent.Application;
using Polyvalent.Data;
using Polyvalent.Desktop.ViewModels;

namespace Polyvalent.Desktop;

public sealed partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var database = new SqliteDatabase(Program.DatabasePath);
            database.InitializeAsync().GetAwaiter().GetResult();
            var store = new PolyvalentStore(database);
            var viewModel = new MainWindowViewModel(
                new ResearchService(store),
                Program.DatabasePath);
            var window = new MainWindow { DataContext = viewModel };
            window.Opened += async (_, _) => await viewModel.InitializeAsync();
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
