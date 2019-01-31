﻿using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using OpenHardwareMonitor.Hardware;
using TTController.Service.Manager;

namespace TTController.Service
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                switch (AskChoice(  "[1]\tManage Service" + 
                                  "\n[2]\tRun in console" +
                                  "\n[3]\tDump info" + 
                                  "\n", '1', '2', '3'))
                {
                    case '1':
                        ManageService();
                        break;
                    case '2':
                        new TTService().Initialize();
                        Console.ReadKey();
                        break;
                    case '3':
                        ShowInfo();
                        Console.ReadKey();
                        break;
                }
            }
            else
            {
                ServiceBase.Run(new TTService());
            }
        }

        private static void ManageService()
        {
            var installed = ServiceController.GetServices().Any(s => s.ServiceName.Equals(TTInstaller.ServiceName));
            if (installed)
                UninstallService();
            else
                InstallService();
        }

        private static void ShowInfo()
        {
            Console.WriteLine("Controllers");
            Console.WriteLine("-------------------------------");
            using (var deviceManager = new DeviceManager())
            {
                foreach (var controller in deviceManager.Controllers)
                {
                    Console.WriteLine($"Name: {controller.Name}" +
                                      $"\nVendorId: {controller.VendorId}" +
                                      $"\nProductId: {controller.ProductId}" +
                                      $"\nPorts:");
                    foreach (var port in controller.Ports)
                    {
                        var data = controller.GetPortData(port.Id);
                        Console.WriteLine($"\tId: {port.Id}" +
                                          $"\n\tStats:" +
                                          $"\n\t\tSpeed: {data.Speed}%" +
                                          $"\n\t\tRPM: {data.Rpm} RPM" +
                                          $"\n\t\tUnknown: {data.Unknown}" +
                                          $"\n\tIdentifier: [{port.ControllerVendorId}, {port.ControllerProductId}, {port.Id}]" +
                                          $"\n");
                    }
                }
            }

            Console.WriteLine("Sensors");
            Console.WriteLine("-------------------------------");
            using (var temperatureManager = new TemperatureManager(null))
            {
                foreach (var hardware in temperatureManager.Sensors.Select(s => s.Hardware).Distinct())
                {
                    hardware.Update();

                    Console.WriteLine($"{hardware.Name}:");
                    foreach (var sensor in hardware.Sensors.Where(s => s.SensorType == SensorType.Temperature))
                        Console.WriteLine($"\t{sensor.Identifier}:" +
                                          $"\n\t\tName: {sensor.Name}" + 
                                          $"\n\t\tValue: {sensor.Value ?? float.NaN}" +
                                          $"\t");
                }
            }
        }

        private static void InstallService()
        {
            if (AskChoice("Service not found. Install? ", 'y', 'n') == 'y')
            {
                if (AskChoice("Install as custom user? ", 'y', 'n') == 'y')
                    TTInstaller.Account = ServiceAccount.User;

                ManagedInstallerClass.InstallHelper(new[] { "/LogFile=", "/LogToConsole=true", Assembly.GetExecutingAssembly().Location });
            }
        }

        private static void UninstallService()
        {
            if (AskChoice("Service already installed. Uninstall? ", 'y', 'n') == 'y')
                ManagedInstallerClass.InstallHelper(new[] { "/u", "/LogFile=", "/LogToConsole=true", Assembly.GetExecutingAssembly().Location });
        }

        private static char AskChoice(string message, params char[] choices)
        {
            while (true)
            {
                Console.Write($"{message}[{string.Join(", ", choices)}]: ");
                var keyInfo = Console.ReadKey(true);

                if (choices.Contains(keyInfo.KeyChar))
                {
                    Console.WriteLine($"{keyInfo.KeyChar}");
                    return keyInfo.KeyChar;
                }

                Console.WriteLine();
            }
        }
    }
}