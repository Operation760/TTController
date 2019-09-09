using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using NLog;
using TTController.Common;
using TTController.Common.Plugin;
using TTController.Service.Hardware;
using TTController.Service.Utils;

namespace TTController.Service.Manager
{
    public sealed class DeviceManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IReadOnlyList<IControllerProxy> Controllers { get; }

        public DeviceManager()
        {
            Logger.Info("Creating Device Manager...");
            Controllers = new List<IControllerProxy>();

            var definitions = typeof(IControllerDefinition).FindImplementations()
                .Select(t => (IControllerDefinition)Activator.CreateInstance(t))
                .ToList();

            var controllers = new List<IControllerProxy>();
            foreach (var definition in definitions)
            {
                Logger.Debug("Searching for \"{0}\" controllers", definition.Name);

                var detectedDevices = DeviceList.Local.GetHidDevices().Where(d => d.VendorID == definition.VendorId && definition.ProductIds.Contains(d.ProductID));
                var detectedCount = detectedDevices.Count();

                if (detectedCount == 0)
                    continue;

                if(detectedCount == 1)
                    Logger.Trace("Found 1 controller [{vid}, {pid}]", definition.VendorId, detectedDevices.Select(d => d.ProductID).First());
                else
                    Logger.Trace("Found {count} controllers [{vid}, [{pids}]]", detectedCount, definition.VendorId, detectedDevices.Select(d => d.ProductID));

                foreach (var device in detectedDevices)
                {
                    var controller = (IControllerProxy) Activator.CreateInstance(definition.ControllerProxyType, new HidDeviceProxy(device), definition);
                    if (!controller.Init())
                    {
                        Logger.Warn("Failed to initialize \"{0}\" controller! [{1}, {2}]", definition.Name, device.VendorID, device.ProductID);
                        continue;
                    }

                    Logger.Info("Initialized \"{0}\" controller [{1}, {2}]", definition.Name, device.VendorID, device.ProductID);

                    controllers.Add(controller);
                }
            }

            Controllers = controllers;
        }

        public IControllerProxy GetController(PortIdentifier port) =>
            Controllers.FirstOrDefault(c => c.IsValidPort(port));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            Logger.Info("Disposing Device Manager...");

            var count = Controllers.Count;
            foreach (var controller in Controllers)
                controller.Dispose();

            Logger.Debug("Disposed controllers: {0}", count);
        }
    }
}
