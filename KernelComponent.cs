using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using uFrame.Kernel;
using Kernel;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Scene = UnityEngine.SceneManagement.Scene;

namespace Kernel
{

    /// <summary>
    /// Use this code to turn your own monobeh classes into IKernelComponent.
    /// Literally, implement the interface on your base class and copy-paste the code
    /// </summary>
    public class KernelComponent : MonoBehaviour, IKernelComponent
    {

        public IEventAggregator EventAggregator { get; set; }

        public virtual void Start()
        {
            this.LocateLoadedKernel().Subscribe(loadedKernel => loadedKernel.InitializeComponent(this));
        }

        public Scene Scene { get { return gameObject.scene; } } //dreaming of new C# 5.0 => ,right?

        public virtual void KernelLoaded() {  }

        public void Dispose() { }

        /// <summary>Wait for an Event to occur on the global event aggregator.</summary>
        /// <example>
        /// this.OnEvent&lt;MyEventClass&gt;().Subscribe(myEventClassInstance=&gt;{ DO_SOMETHING_HERE });
        /// </example>
        public IObservable<TEvent> OnEvent<TEvent>()
        {
            return EventAggregator.GetEvent<TEvent>();
        }

        /// <summary>Publishes a command to the event aggregator. Publish the class data you want, and let any "OnEvent" subscriptions handle them.</summary>
        /// <example>
        /// this.Publish(new MyEventClass() { Message = "Hello World" });
        /// </example>
        public void Publish(object eventMessage)
        {
            EventAggregator.Publish(eventMessage);
        }


    }

    public interface IInjectable
    {
        
    }

}
