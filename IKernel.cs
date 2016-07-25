using System;
using System.Collections.Generic;
using UniRx;
using Zenject;

namespace Kernel
{
    public interface IKernel : IEventsAware
    {

        DiContainer Container { get; set; }

        /// <summary>
        /// State of the kernel
        /// </summary>
        KernelState State { get; set; }

        /// <summary>
        /// Any component will be initialized with a kernel on start
        /// </summary>
        /// <param name="kernelComponent"></param>
        void InitializeComponent(IKernelComponent kernelComponent);
        
        /// <summary>
        /// Any component will be initialized with a kernel on start
        /// </summary>
        /// <param name="kernelComponent"></param>
        IObservable<IKernel> LoadKernel();

        /// <summary>
        /// Use this method to wait for kernel to load
        /// Resolved immediately if kernel is already loaded
        /// </summary>
        /// <returns></returns>
        IObservable<IKernel> WaitToLoad();

        /// <summary>
        /// Detaches kernel from any scene and makes it persist between scene loads
        /// </summary>
        void MakePersistent();

        /// <summary>
        /// Name of the kernel for debugging purposes
        /// </summary>
        string Name { get; }
    }

    public interface IEventsAware
    {
        IEventAggregator EventAggregator { get; set; }
    }

    public interface IKernelService : IEventsAware
    {
        /// <summary>
        /// </summary>
        void Setup();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        IObservable<Unit> SetupAsync();

        void KernelLoaded();
    }

    public static class KernelServiceExtensions
    {

        /// <summary>
        /// A wrapper for GetEvent on the EventAggregator GetEvent method.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns>An observable capable of subscriptions and filtering.</returns>
        public static IObservable<TEvent> OnEvent<TEvent>(this IKernelService systemController)
        {
            return systemController.EventAggregator.GetEvent<TEvent>();
        }

        /// <summary>
        /// A wrapper for the Event Aggregator.Publish method.
        /// </summary>
        /// <param name="eventMessage"></param>
        public static void Publish(this IKernelService systemController, object eventMessage)
        {
            systemController.EventAggregator.Publish(eventMessage);
        }
    }



    public interface IBeforeLoadKernelHook
    {
        void BeforeLoadKernel();
    }

    public interface IBeforeLoadKernelHookAsync
    {
        IObservable<Unit> BeforeLoadKernelAsync();
    }

    public interface IAfterLoadKernelHook
    {
        void AfterLoadKernel();
    }

    public interface IAfterLoadKernelHookAsync
    {
        IObservable<Unit> AfterLoadKernelAsync();
    }

    public interface IIoCHook : IDisposable
    {
        void BeforeIoCContainerSet(DiContainer container);
        void AfterIoCContainerSet(DiContainer container);
    }

    public interface IKernelReadyHook : IDisposable
    {
        void KernelReady();
    }

    public interface IGetServicesHook : IDisposable
    {
        void IncludeServices(IList<IKernelService> services);
    }

    public interface IInitializeComponentHook : IDisposable
    {
        void InitializeComponent();
        IObservable<IKernelComponent> InitializeComponentAsync();
    }
}