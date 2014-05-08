﻿using CefSharp.Internals;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace CefSharp.BrowserSubprocess
{
    public class SubprocessServiceHost : ServiceHost, ISubprocessCallback
    {
        public SubprocessProxy Service { get; set; }

        private SubprocessServiceHost()
            : base(typeof(SubprocessProxy), new Uri[0])
        {
        }

        public static SubprocessServiceHost Create(int parentProcessId, int browserId)
        {
            var host = CreateServiceHost();

            var serviceName = SubprocessProxyFactory.GetServiceName(parentProcessId, browserId);
            host.KillExistingServiceIfNeeded(serviceName);

            host.AddServiceEndpoint(
                typeof(ISubprocessProxy),
                new NetNamedPipeBinding(),
                new Uri(serviceName)
            );

            host.Open();
            return host;
        }

        private static SubprocessServiceHost CreateServiceHost()
        {
            var host = new SubprocessServiceHost();
            var serviceDebugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();

            if (serviceDebugBehavior == null)
            {
                serviceDebugBehavior = new ServiceDebugBehavior
                {
                    IncludeExceptionDetailInFaults = true
                };
                host.Description.Behaviors.Add(serviceDebugBehavior);
            }
            else
            {
                serviceDebugBehavior.IncludeExceptionDetailInFaults = true;
            }

            return host;
        }

        private void KillExistingServiceIfNeeded(string serviceName)
        {
            // It might be that there is an existing process already bound to this port. We must get rid of that one, so that the
            // endpoint address gets available for us to use.
            try
            {
                var javascriptProxy = SubprocessProxyFactory.CreateSubprocessProxyClient(serviceName, this, TimeSpan.FromSeconds(1));
                javascriptProxy.Terminate();
            }
            catch
            {
                // We assume errors at this point are caused by things like the endpoint not being present (which will happen in
                // the first render subprocess instance).
            }
        }

        // Don't need to provide an implementation of those methods, since we only instantiate the JavascriptProxy to be able
        // to terminate dangling instances.
        public object CallMethod(int objectId, string name, object[] parameters)
        {
            throw new NotImplementedException();
        }

        public object GetProperty(int objectId, string name)
        {
            throw new NotImplementedException();
        }

        public object SetProperty(int objectId, string name)
        {
            throw new NotImplementedException();
        }
    }
}