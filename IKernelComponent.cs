using System;
using uFrame.Kernel;

namespace Kernel
{
    public interface IKernelComponent : IEventsAware, IDisposable
    {

        /// <summary>
        /// Scene of this behaviour
        /// </summary>
        UnityEngine.SceneManagement.Scene Scene { get; }

        /// <summary>
        /// Callback to be invoked when kernel is ready
        /// </summary>
        void KernelLoaded();
    }
}