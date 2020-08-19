using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;

namespace 智能网络机器人控制系统1._0
{
    class JoystickUtil
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Device init(Control parent, Device applicationDevice)
        {
            // Enumerate joysticks in the system.
            foreach (DeviceInstance instance in Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
            {
                // Create the device.  Just pick the first one
                applicationDevice = new Device(instance.InstanceGuid);
                break;
            }

            if (null == applicationDevice)
            {
                return null;
            }

            // Set the data format to the c_dfDIJoystick pre-defined format.
            applicationDevice.SetDataFormat(DeviceDataFormat.Joystick);
            // Set the cooperative level for the device.
            // applicationDevice.SetCooperativeLevel(parent, CooperativeLevelFlags.Exclusive | CooperativeLevelFlags.Foreground);
            // Enumerate all the objects on the device.
            foreach (DeviceObjectInstance d in applicationDevice.Objects)
            {
                // For axes that are returned, set the DIPROP_RANGE property for the
                // enumerated axis in order to scale min/max values.

                if ((0 != (d.ObjectId & (int)DeviceObjectTypeFlags.Axis)))
                {
                    // Set the range for the axis.
                    applicationDevice.Properties.SetRange(ParameterHow.ById, d.ObjectId, new InputRange(+100, +200));
                }
                // Update the controls to reflect what
                // objects the device supports.
                // UpdateControls(d);
            }
            return applicationDevice;
        }
    }
}
