using AvaloniaExampleProject.Shell;

namespace AvaloniaExampleProject.Services.Navigation;

public interface INavigationRegistry
{
    ViewModelBase Create(AppRoute route);
}
