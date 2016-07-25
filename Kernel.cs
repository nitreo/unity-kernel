﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using uFrame.Kernel;
using UniRx;
using UnityEngine;
using Zenject;

namespace Kernel
{
    public class Kernel : MonoBehaviour, IKernel
    {
        private IObservable<IKernel> _kernelReadyObservable;
        private IObservable<IKernel> _thisObservable;
        private List<IBeforeLoadKernelHook> _beforeLoadKernelHooks;
        private List<IBeforeLoadKernelHookAsync> _beforeLoadKernelHooksAsync;

        public virtual DiContainer Container { get; set; }
        
        public virtual IEventAggregator EventAggregator { get; set; }

        public KernelState State { get; set; }

        public string Name
        {
            get { return gameObject.name; }
        }

        public UnityEngine.SceneManagement.Scene UnityScene
        {
            get { return gameObject.scene; }
        }

        private IObservable<IKernel> KernelReadyObservable
        {
            get
            {
                if (State == KernelState.Ready)
                    return _thisObservable ?? (_thisObservable = Observable.Return(this as IKernel));

                return _kernelReadyObservable ??
                      (_kernelReadyObservable = Observable.FromCoroutineValue<IKernel>(WaitForKernelReadyCoroutine));

            }
        }

        public void MakePersistent()
        {
            DontDestroyOnLoad(gameObject);
        }

        public IObservable<IKernel> WaitToLoad()
        {
            return KernelReadyObservable; //TODO Same here
        }

        /// <summary>
        /// Coroutine which is used to wait for kernel to load
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForKernelReadyCoroutine()
        {
            while (State != KernelState.Ready)
                yield return new WaitForSeconds(0.1f);
            yield return this;
        }

        public virtual void Awake()
        {
            KernelRegister.RegisterKernelForScene(UnityScene, this);
        }

        public virtual void InitializeComponent(IKernelComponent kernelComponent)
        {
            var monobeh = kernelComponent as MonoBehaviour;
            monobeh.name += " @ " + Name;
            kernelComponent.EventAggregator = EventAggregator;
            if(kernelComponent is IInjectable) Container.Inject(kernelComponent);
                kernelComponent.KernelLoaded();
        }

        private void BindLocal<T>()
        {
            foreach (var instance in GetLocal<T>())
            {
                Container.BindInstance(instance);
            }
        }

        private List<T> GetLocal<T>()
        {
            return gameObject
                .GetComponentsInChildren(typeof(T))
                .Cast<T>()
                .ToList();
        }

        private List<T> ResolveAll<T>()
        {
            return Container.ResolveAll<T>(true);
        }

        public List<IBeforeLoadKernelHook> BeforeLoadKernelHooks
        {
            get { return _beforeLoadKernelHooks ?? (_beforeLoadKernelHooks = GetLocal<IBeforeLoadKernelHook>()); }
            set { _beforeLoadKernelHooks = value; }
        }

        public List<IBeforeLoadKernelHookAsync> BeforeLoadKernelHooksAsync
        {
            get { return _beforeLoadKernelHooksAsync ?? (_beforeLoadKernelHooksAsync = GetLocal<IBeforeLoadKernelHookAsync>()); }
            set { _beforeLoadKernelHooksAsync = value; }
        }


        public IObservable<Unit> ProcessBeforeKernelLoadHooks()
        {
            foreach (var hook in BeforeLoadKernelHooks)
            {
                hook.BeforeLoadKernel();
            }

            return BeforeLoadKernelHooksAsync
                .Select(_ => _.BeforeLoadKernelAsync())
                .Where(_ => _ != null)
                .WhenAll()
                .Select(_=>Unit.Default);
        }


        public IObservable<List<IKernelService>> ProcessServices()
        {
            var services = GetLocal<IKernelService>();

            GetLocal<IGetServicesHook>().ForEach(s => s.IncludeServices(services)); //Include possible low-fidelity-introduced services

            foreach (var service in services)
            {
                Container.Bind(service.GetType()).ToInstance(service);
                Container.BindAllInterfacesToInstance(service);
            }

            foreach (var service in services)
            {
                service.EventAggregator = EventAggregator;
                service.Setup();
            }

            return services
                .Select(service => service.SetupAsync())
                .Where(setupAsyncObservable => setupAsyncObservable != null)
                .WhenAll().Select(_ => services);
        } 

        public virtual IObservable<IKernel> LoadKernel()
        {

            ProcessBeforeKernelLoadHooks()
            .ContinueWith(_=>ProcessServices())
            .Subscribe(services =>
             {

                 foreach (var service in services)
                 {
                     Container.Inject(service);
                 }

                 services.ForEach(s => s.KernelLoaded());
                 ResolveAll<IKernelReadyHook>().ForEach(h => h.KernelReady());

                 KernelRegister.Logger.Log(string.Format("{0} Kernel Ready", Name ?? gameObject.scene.name));
                 State = KernelState.Ready; //TODO Stub
             });

            return KernelReadyObservable;
        }


    }
}