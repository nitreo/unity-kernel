using UniRx;
using System; // Required for WP8 and Store APPS

namespace Kernel
{
    public interface IEventAggregator
    {
        IObservable<TEvent> GetEvent<TEvent>();
        void Publish<TEvent>(TEvent evt);
    }
}