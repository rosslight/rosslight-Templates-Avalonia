using System.ComponentModel;

namespace AvaloniaExampleProject.Features.Dialogs;

internal sealed class PropertyChangedObservable<TSource>(
    TSource source,
    string propertyName,
    Func<TSource, bool> selector
) : IObservable<bool>
    where TSource : INotifyPropertyChanged
{
    public IDisposable Subscribe(IObserver<bool> observer)
    {
        observer.OnNext(selector(source));

        void OnPropertyChanged(object? _, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName)
                observer.OnNext(selector(source));
        }

        source.PropertyChanged += OnPropertyChanged;
        return new Subscription(() => source.PropertyChanged -= OnPropertyChanged);
    }

    private sealed class Subscription(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}
