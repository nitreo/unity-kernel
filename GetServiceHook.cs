using System.Collections.Generic;
using UnityEngine;

namespace Kernel
{
    public class GetServiceHook : MonoBehaviour, IGetServicesHook
    {
        public void Dispose()
        {
            
        }

        public void IncludeServices(IList<IKernelService> services)
        {
        }
    }
}