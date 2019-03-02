﻿using System.Collections.Generic;
using System.Linq;
using TTController.Common;
using TTController.Service.Controller.Definition;
using TTController.Service.Hardware;

namespace TTController.Service.Controller.Proxy
{
    public class DpsgControllerProxy : AbstractControllerProxy
    {
        public DpsgControllerProxy(IHidDeviceProxy device, IControllerDefinition definition)
            : base(device, definition) { }

        public override IEnumerable<PortIdentifier> Ports
        {
            get
            {
                yield return new PortIdentifier(Device.VendorId, Device.ProductId, 0);
            }
        }

        public override bool SetRgb(byte port, byte mode, IEnumerable<LedColor> colors)
        {
            if (port != 0)
                return false;

            var bytes = new List<byte> { 0x30, 0x42, mode };
            foreach (var color in colors)
            {
                bytes.Add(color.G);
                bytes.Add(color.R);
                bytes.Add(color.B);
            }

            var result = Device.WriteReadBytes(bytes);
            return IsSuccess(result);
        }

        public override bool SetSpeed(byte port, byte speed)
        {
            if (port != 0)
                return false;

            var result = Device.WriteReadBytes(new List<byte> { 0x30, 0x41, 0x04, speed });
            return IsSuccess(result);
        }

        public override PortData GetPortData(byte port)
        {
            if (port != 0)
                return null;

            // 0x31, 0x33 // WATTS
            // 0x31, 0x34 // VVOut12
            // 0x31, 0x35 // VVout5
            // 0x31, 0x36 // VVOut33
            // 0x31, 0x37 // VIOut12
            // 0x31, 0x38 // VIOut5
            // 0x31, 0x39 // VIOut33
            // 0x31, 0x3a // Temp
            // 0x31, 0x3b // FanSpeed

            var data = new PortData()
            {
                PortId = 0,
                Temperature = 0,
                Rpm = 0,
                ["WATTS"] = 0
            };

            return data;
        }

        public override void SaveProfile()
        {
            return;
        }

        public override bool Init()
        {
            var result = Device.WriteReadBytes(new List<byte> { 0xfe, 0x31 });
            return IsSuccess(result);
        }

        public override bool IsValidPort(PortIdentifier port) =>
            port.ControllerProductId == Device.ProductId &&
           port.ControllerVendorId == Device.VendorId &&
           port.Id == 0;

        private bool IsSuccess(IEnumerable<byte> bytes) =>
            bytes.Any() && bytes.ElementAt(0) == 0xfc;
    }
}