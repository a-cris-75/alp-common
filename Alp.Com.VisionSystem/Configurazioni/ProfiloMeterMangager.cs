//using Configurazioni;
using HalconDotNet;
using log4net;
using Alp.Com.VisionSystem.ATProfilerHalcon.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alp.Com.VisionSystem.ATProfilerHalcon.Configurazioni
{
    public class ProfilometerManager
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
                            ("ProfilometerManager".PadRight(20, ' '));


        private readonly Dictionary<string, ConfigurazioneProfilometer> _devices = new Dictionary<string, ConfigurazioneProfilometer>();

        public void Init(Dictionary<string, ConfigProfilometer> config)
        {
            foreach (var item in config)
            {
                var device = new ConfigurazioneProfilometer(item.Value.IP);

                device.SetDeviceInfo(item.Value.IP);
                device.SetProfConfigurationFile(item.Value.CONFIGFILE);
                device.SetProfCalibrationFile(item.Value.CALIBRATIONFILE);

                device.LoadCalib(item.Value.CALIBRATIONFILE, true, 0);

                device.OpenDevice();

                _devices[item.Key] = device;
            }
        }

        public int GetStatus()
        {
            bool anyError = false;
            try
            {
                anyError = _devices.Values.Any(d => !d.IsOpenDevice());
                if (anyError) Log.Error($"Status profilometer returned: {anyError}");
            }
            catch (Exception ex) {
                Log.Error($"Error getstatus profilometer: {ex.Message}");
            }
            return anyError ? (int)ProfilometerStatus.Status.Error : (int)ProfilometerStatus.Status.Ready;
        }

        public async Task<HTuple[]> Execute()
        {
            var result = new List<HTuple>();

            bool isGrabbing = false;

            int i = 1;
            foreach (var device in _devices.Values)
            {
                try
                {
                    Log.Info("Starting grab on: Profilometer" + i);
                    isGrabbing = device.StartGrab(device.Device);

                    var pc = await device.SingleAcqPc(device.Device);

                    if (pc == null)
                    {
                        Log.Error("While grabbing, returned pointcloud null");
                        return null;
                    }

                    var hobj = await device.SingleAcqHimage(pc);
                    isGrabbing = device.StopGrab(device.Device);
                    Log.Info("Ended grab on: Profilometer" + i);

                    result.Add(hobj);
                }
                catch (Exception ex)
                {
                    Log.Error("While grabbing procedure ended - " + ex.Message);
                    return null;
                }

                i += 1;                
            }

            return result.ToArray();
        }

        public IEnumerable<ConfigurazioneProfilometer> Devices => _devices.Values;
    }
}
