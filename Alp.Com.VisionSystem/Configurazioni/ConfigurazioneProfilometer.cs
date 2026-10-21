using AT.cx.NET;
using Emgu.CV.Flann;
using HalconDotNet;
//using LibreriaHalconProfiloDalsa;
//using Microsoft.Extensions.Logging;
using Alp.Com.VisionSystem.ATProfilerHalcon.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Alp.Com.VisionSystem.ATProfilerHalcon.Configurazioni
{
    public class ConfigurazioneProfilometer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
                            ("ConfigurazioneProfilometer".PadRight(20, ' '));

        private readonly object _lockObject = new Object();
        private DeviceInfo _di = null;
        private Device _cam = null;
        private Calib _calib = null;

        private Dictionary<string, ConfigProfilometer> _profilometers;

        private string confile = string.Empty;
        private string calibfile = string.Empty;
        private bool _loadConfigFile = false;
        private bool _loadCalibFile = false;

        public bool isConnected = false;

        public DeviceInfo DeviceInfo { get { return _di; } }
        public Device Device { get { return _cam; } }
        public ConfigurazioneProfilometer()
        {
            ;
        }
        public ConfigurazioneProfilometer(string ip)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ip) && ip.Contains(".") && ip.Split('.').Length == 4)
                {
                    lock (_lockObject)
                    {
                        var list = AT.cx.NET.DeviceFactory.findDevices();
                        foreach (var di in list)
                        {
                            if (di.deviceIP.Equals(ip))
                            {
                                _di = di;
                                break;
                            }
                        }
                    }


                    //_di = AT.cx.NET.DeviceFactory.findDevices().FirstOrDefault(x => x.deviceIP.Equals(ip));
                }
                else
                {
                    Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer Error Device wrong ip : {ip}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer Exception: " + ex.Message + ", " + ex.StackTrace);
            }
        }

        public string GetIpProfilometer(ConfigProfilometer prof)
        {
            try
            {
                return prof.IP;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public Device[] profs;

        public bool IsReady => profs != null && profs.Length > 0;

        public void InitProfilometer(Dictionary<string, ConfigProfilometer> profilometers)
        {
            _profilometers = profilometers;

            int index = 0;

            profs = new Device[profilometers.Count];

            foreach (var item in profilometers)
            {
                try
                {
                    ConfigurazioneProfilometer prof = new ConfigurazioneProfilometer(item.Value.IP);

                    Log.Info(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ProfilometerLib: Init Profilometer " + item.Value.IP + " " + item.Value.CONFIGFILE + " " + item.Value.CALIBRATIONFILE);

                    prof.SetDeviceInfo(item.Value.IP);
                    prof.SetProfConfigurationFile(item.Value.CONFIGFILE);
                    prof.SetProfCalibrationFile(item.Value.CALIBRATIONFILE);

                    prof.LoadCalib(item.Value.CALIBRATIONFILE, true, index);

                    prof.isConnected = true;

                    prof.OpenDevice();

                    profs[index] = _cam;

                    Log.Info(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ProfilometerLib: Init Profilometer" + index + " - Completed initializzation!");

                }
                catch (Exception ex)
                {
                    Log.Error(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ProfilometerLib: Init Profilometer" + index + " Error: " + ex.Message);
                }

                index++;
            }
        }
        public async Task<HTuple[]> ExecuteOnProfilometers()
        {
            HTuple[] images = new HTuple[profs.Length];
            var index = 1;
            foreach (var prof in profs)
            {
                try
                {
                    _cam = prof;
                    Log.Info("Start acquisition!");

                    if (_cam == null) Log.Info("Dio cane");
                    _cam.allocAndQueueBuffers();
                    _cam.startAcquisition();
                    //PointCloud pointCloudImage = await prof.SingleAcqPc(prof);
                    //HTuple image = await prof.SingleAcqHimage(pointCloudImage);
                    //prof.StopGrab(prof);
                    Log.Info("Stop acquisition!");

                    _cam.stopAcquisition();
                    _cam.freeBuffers();

                    //images[index - 1] = image;
                }
                catch (Exception ex)
                {
                    Log.Error(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ProfilometerLib: Exec Profilometer" + index + " " + ex.Message);
                    return null;
                }
                index++;
            }

            return images;
        }
        public void SetProfConfigurationFile(string file)
        {
            Log.Debug(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ProfilometerLib: SetProfConfigurationFile " + file);
            confile = file;
            _loadConfigFile = true;
        }
        public void SetProfCalibrationFile(string file)
        {
            Log.Debug(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ProfilometerLib: SetProfCalibrationFile " + file);
            calibfile = file;
            _loadCalibFile = true;
        }
        public void SetDeviceInfo(string ip)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ip) && ip.Contains(".") && ip.Split('.').Length == 4)
                {
                    lock (_lockObject)
                    {
                        var list = AT.cx.NET.DeviceFactory.findDevices();
                        foreach (var di in list)
                        {
                            if (di.deviceIP.Equals(ip))
                            {
                                _di = di;
                                break;
                            }
                        }
                    }


                    //_di = AT.cx.NET.DeviceFactory.findDevices().FirstOrDefault(x => x.deviceIP.Equals(ip));
                }
                else
                {
                    Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, SetDeviceInfo Error Device wrong ip : {ip}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, SetDeviceInfo Exception: " + ex.Message + ", " + ex.StackTrace);
            }
        }
        public bool DeviceExist(string ip)
        {
            try
            {
                foreach (var di in AT.cx.NET.DeviceFactory.findDevices())
                {
                    if (di.deviceIP.Equals(ip))
                    {
                        _di = di;
                        return true;
                    }
                }
                return false;
                //return (_di = AT.cx.NET.DeviceFactory.findDevices().FirstOrDefault(x => x.deviceIP.Equals(ip))) != null;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, DeviceExist Exception: " + ex.Message + ", " + ex.StackTrace);
                Console.WriteLine("ConfigurazioneProfilometer, DeviceExist Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public DeviceInfo GetDeviceInfo(string ip)
        {
            try
            {
                foreach (var di in AT.cx.NET.DeviceFactory.findDevices())
                {
                    if (di.deviceIP.Equals(ip))
                    {
                        _di = di;
                        return _di;
                    }
                }
                //return (_di = AT.cx.NET.DeviceFactory.findDevices().FirstOrDefault(x => x.deviceIP.Equals(ip)));
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, GetDeviceInfo Exception: " + ex.Message + ", " + ex.StackTrace);
                Console.WriteLine("ConfigurazioneProfilometer, GetDeviceInfo Exception: " + ex.Message + ", " + ex.StackTrace);
                return null;
            }
        }
        public bool IsDeviceInfo()
        {
            try
            {
                return _di != null;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, IsDeviceInfo Exception: " + ex.Message + ", " + ex.StackTrace);
                Console.WriteLine("ConfigurazioneProfilometer, IsDeviceInfo Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool IsDeviceInfo(string ip)
        {
            try
            {
                return GetDeviceInfo(ip) != null;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, IsDeviceInfo Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public string GetDeviceModel()
        {
            try
            {
                return (_di != null ? _di.deviceModel : string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, GetDeviceModel Exception: " + ex.Message + ", " + ex.StackTrace);
                return string.Empty;
            }
        }
        public string GetSerialNumber()
        {
            try
            {
                return (_di != null ? _di.deviceSerialNumber : string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, GetSerialNumber  Exception: " + ex.Message + ", " + ex.StackTrace);
                return string.Empty;
            }
        }
        /*public uint? GetStatus()
        {
            try
            {
                if (_di == null)
                    return null;
                else
                    return _di.deviceStatus;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, GetStatus  Exception: " + ex.Message + ", " + ex.StackTrace);
                return null;
            }
        }*/

        public int GetStatusProfilometers()
        {
            try
            {
                bool allOk = true;
                bool allConnectable = true;

                for (int i = 0; i < profs.Length; i++)
                {
                    /*if (!profs[i].isOpen())
                    {
                        allOk = false;
                    }*/
                }

                return allOk ? (int)ProfilometerStatus.Status.Ready : (int)ProfilometerStatus.Status.Busy;
            }
            catch (Exception ex)
            {
                //Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, GetStatus  Exception: " + ex.Message + ", " + ex.StackTrace);
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, GetStatus  Exception: " + ex.Message + ", " + ex.StackTrace);
                return (int)ProfilometerStatus.Status.Error;
            }
        }
        public bool IsC6Model()
        {
            try
            {
                if (_di == null)
                    return false;
                else
                    return _di.isC6Model();
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, IsC6Model Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool OpenDevice()
        {
            try
            {
                if (_di == null)
                    return false;
                else
                    return ((_cam = DeviceFactory.openDevice(_di.deviceURI)) != null);
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, OpenDevice Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool IsOpenDevice()
        {
            try
            {
                if (_cam != null)
                    return _cam.isOpen();
                else
                    return false;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, IsOpenDevice Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool CloseDevice()
        {
            try
            {
                if (_cam == null)
                    return false;

                _cam.close();

                return (_cam != null ? !_cam.isOpen() : false);
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, CloseDevice Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool DisposeDevice()
        {
            try
            {
                if (_cam == null)
                {
                    return true;
                }
                if (_cam.isOpen())
                {
                    _cam.close();
                }
                if (_cam != null)
                {
                    _cam.Dispose();

                    _cam = null;
                }


                return true;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - ConfigurazioneProfilometer, DisposeDevice Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool LoadParameter(string conf_file, bool all)
        {
            Log.Debug(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadParameter: " + _di.deviceIP);
            try
            {
                /* TODO: FASE 2 (miglior controllo sui profilometri)
                 
                try
                {
                    if (!string.IsNullOrWhiteSpace(conf_file) && File.Exists(conf_file))
                    {
                        _cam.uploadFile(conf_file, "UserSet0");
                    }
                    else
                    {
                        _cam.downloadFile("UserSet0", conf_file);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, uploadFile  Exception: " + ex.Message + ", " + ex.StackTrace);
                    Console.WriteLine("camera doesn't support uploadFile...");
                    return false;
                }

                if (!all) return true;

                Variant val = 0;
                try
                {
                    _cam.getParam("GevSCCFGMultiPart", val);
                    val.set((bool)true);
                    _cam.setParam("GevSCCFGMultiPart", val);
                }
                catch (Exception ex)
                {
                    Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadParameter GevSCCFGMultiPart Exception: " + ex.Message + ", " + ex.StackTrace);
                    Console.WriteLine("camera doesn't support GeV multipart...");
                    return false;
                }

                // 3. Probe and adjust device settings:
                try
                {
                    _cam.getParam("DeviceScanType", val);
                }
                catch (Exception ex)
                {
                    Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadParameter Get DeviceScanType Exception: " + ex.Message + ", " + ex.StackTrace);
                    Console.WriteLine("Device doesn't support \"Linescan3D\".");
                    return false;
                }

                // switch DeviceScanType to "Linescan3D"
                if (val != "Linescan3D")
                {
                    try
                    {
                        Console.WriteLine("Current sensor devicemode is not 'Linescan3D', we activate itException e");
                        _cam.setParam("DeviceScanType", "Linescan3D");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadParameter Set DeviceScanType Exception: " + ex.Message + ", " + ex.StackTrace);
                        Console.WriteLine("Cannot switch device into Linescan3D mode.");
                        return false;
                    }
                }

                _cam.setParam("GevSCPSPacketSize", 7960);

                // activate Range and Reflectance components
                try
                {
                    val.set("Scan3dExtraction0");
                    _cam.setParam("RegionSelector", val);
                    val.set("On");
                    _cam.setParam("RegionMode", val);

                    val.set("Range");
                    _cam.setParam("ComponentSelector", val);
                    val.set(true);
                    _cam.setParam("ComponentEnable", val);

                    val.set("Reflectance");
                    _cam.setParam("ComponentSelector", val);
                    val.set(false);
                    _cam.setParam("ComponentEnable", val);

                    val.set("Scatter");
                    _cam.setParam("ComponentSelector", val);
                    val.set(false);
                    _cam.setParam("ComponentEnable", val);
                }
                catch (Exception ex)
                {
                    Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadParameter Range and Reflectance components Exception: " + ex.Message + ", " + ex.StackTrace);
                    Console.WriteLine("cannot set components");
                    return false;
                }
                */
                return true;

            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadParameter Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool SetExposure(float exposure)
        {
            Log.Debug(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, SetExposure: " + _di.deviceIP);
            try
            {
                Variant val = 0;

                /*if (exposure > 0.0 && !Double.IsNaN(exposure))
                {
                    _cam.getParam("ExposureTime", val);
                    val.set((float)exposure); //Type: Float
                    _cam.setParam("ExposureTime", val);
                }*/
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, SetExposure Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool LoadCalib(string calib_file, bool all, double pos)
        {
            Log.Debug(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadCalib: " + _di.deviceIP);
            try
            {
                Variant val = 0;
                // TODO: FASE 2 - miglior gestione delle calibrazioni ma intanto usiamo solo le configurazioni direttamente da Profilometro
                // 4. load 3d-calibration from camera or file
                if (_calib != null)
                {
                    _calib.Dispose();
                }

                _calib = new Calib();
                try
                {
                    if (!string.IsNullOrWhiteSpace(calib_file) && File.Exists(calib_file))
                    {
                        //Console.WriteLine($"Applicazione calibrazione {_calib}, {calib_file}");
                        // usata solo per organizzare il PointCloud
                        _calib.load(calib_file);
                        //_calib.save(calib_file);
                    }
                    else
                    {
                        // load calibration from sensor, possible sources are "UserData", "Calibration", "CalibrationUser"
                        //Common.downloadCalib(_cam, _calib);
                        _calib.save(calib_file);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errore calibrazione: " + ex.Message);
                }

                //if (!all) return true ; //??

                //double dy = Double.NaN;

                // set relevant acquisition parameters in calibration, we assume we have only one aoi active
                //Common.updateCalibC6(_cam, _calib, 0);

                // set the profile-step-width scaling
                /*_calib.getParam(AT.cx.NET.cx_3d_calib_param_t.CX_3D_PARAM_SY, val);

                if (!Double.IsNaN(pos) && pos > 0.0 && (double)val != pos)
                {
                    _calib.setParam(cx_3d_calib_param_t.CX_3D_PARAM_SY, pos);
                }

                // set the invalid value that will be used in point cloud for all rangemap values that are zero 
                float ivd = float.NaN;    // could be any number e.g. -10.0f, if NAN is used the points are automatically suppressed in showPointCloud 
                _calib.setParam(AT.cx.NET.cx_3d_calib_param_t.CX_3D_PARAM_METRIC_IDV, ivd);

                // Trigger cache update. When calling with argument > 0, the internal cache gets updated. 
                // This prevents a cache update at first call to metric functions cx_3d_range2calibratedABC.
                _calib.setParam(AT.cx.NET.cx_3d_calib_param_t.CX_3D_PARAM_METRIC_CACHE_MODE, 1);
                */
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, LoadCalib Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool SetProfileDistance(double distance)
        {
            try
            {
                // TODO: FASE 2 - miglior gestione dei profilometri
                /*if (_calib == null)
                {
                    return false;
                }

                if (!Double.IsNaN(distance) && distance > 0.0)
                {
                    _calib.setParam(AT.cx.NET.cx_3d_calib_param_t.CX_3D_PARAM_SY, distance);
                }
                */
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, SetProfileDistance Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool StartGrab(Device cam)
        {
            Log.Debug(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, StartGrab: " + _di.deviceIP);
            try
            {
                _cam = cam;

                if (_cam == null)
                {
                    Console.WriteLine("Prova");
                }

                // 5. Allocate and queue internal acquisition buffers
                _cam.allocAndQueueBuffers();

                // 6. Start image acquisition
                _cam.startAcquisition();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, StartGrab Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }
        public bool StopGrab(Device cam)
        {
            Log.Debug(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, StopGrab: " + _di.deviceIP);
            try
            {
                _cam = cam;

                // 8. Stop acquisition
                _cam.stopAcquisition();

                // 9. Cleanup
                _cam.freeBuffers();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, StopGrab Exception: " + ex.Message + ", " + ex.StackTrace);
                return false;
            }
        }

        /*public SingleFrame SingleAcq()
        {
            try
            {
                if (_cam == null)
                {
                    return null;
                }

                Variant val = 0;
                SingleFrame sf = null;
                // 7.1. Grab acquisition buffer, wait for valid buffer with optional timeout. Timeout is given in ms.
                var buffer = _cam.waitForBuffer(3000, true);

                if (buffer.isValid())
                {
                    buffer.getInfo(cx_buffer_info.CX_BUFFER_INFO_IS_MULTIPART, val);
                    if (!(bool)val)
                    {
                        Console.WriteLine("Buffer is not multipart.");
                        return null;
                    }

                    buffer.getInfo(cx_buffer_info.CX_BUFFER_INFO_NUM_PARTS, val);
                    int numParts = val;
                    Console.WriteLine("Number of Parts: " + numParts);

                    // 7.2. Iterate the buffer parts and do some processing on the image data or get a copy for later use.
                    // \note img holds a reference to the image data in the DeviceBuffer, if you need the image data after cx_queueBuffer you need to clone the image!

                    uint region = 0;
                    AT.cx.NET.Image rangeImg = new AT.cx.NET.Image();
                    AT.cx.NET.Image reflectanceImg = new AT.cx.NET.Image();

                    for (int partIdx = 0; partIdx < numParts; partIdx++)
                    {
                        bool isImagePart = false;

                        // print some part information
                        Console.WriteLine("Part " + partIdx + ":");

                        buffer.getPartInfo(partIdx, cx_buffer_part_info.CX_BUFFER_PART_INFO_REGION_ID, val);
                        Console.WriteLine(" RegionID: " + (int)val);
                        region = (uint)val & 0xf;

                        buffer.getPartInfo(partIdx, cx_buffer_part_info.CX_BUFFER_PART_INFO_TYPE_ID, val);
                        Console.WriteLine(" TypeID: " + Common.partTypeIDToStr((cx_buffer_part_type)((int)val)));
                        if ((int)val == (int)cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_IMAGE2D) isImagePart = true;

                        buffer.getPartInfo(partIdx, cx_buffer_part_info.CX_BUFFER_PART_INFO_DATA_PURPOSE_ID, val);
                        Console.WriteLine(" PurposeID: " + Common.partPurposeIDToStr((cx_buffer_part_purpose)((int)val)) + "\n");

                        // get RangeImage for Region 0
                        if (region == 0 && isImagePart && (int)val == (int)cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_RANGE)
                        {
                            rangeImg = buffer.getImage(partIdx);
                        }

                        // get ReflectanceImage for Region 0
                        if (region == 0 && isImagePart && (int)val == (int)cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_REFLECTANCE)
                        {
                            reflectanceImg = buffer.getImage(partIdx);
                        }
                    }
                    try
                    {
                        if (!rangeImg.isEmpty())
                        {
                            // calculate point cloud
                            PointCloud pc = new PointCloud(rangeImg.height(), rangeImg.width());
                            PointCloud.calculatePointCloud(_calib, rangeImg, pc);
                            string pathdir = "";
                            //string pathdir = Path.Combine("C:\\Images\\Profilometer", _di.deviceIP);
                            //if (!Directory.Exists(pathdir))
                            //{
                            //    Directory.CreateDirectory(pathdir);
                            //}
                            //string pathfile = Path.Combine(pathdir, DateTime.Now.ToString("yyyyMMddTHHmmss"), ".ply");
                            //pc.save(pathfile);

                            //pc.computeNormals();                                                // compute normals from point cloud points

                            //// Get buffer pointer and size                                               
                            //byte[] managedBuffer = new byte[(int)pc.points.size()];
                            //Marshal.Copy(pc.points.data(), managedBuffer, 0, (int)pc.points.size());

                            // Get buffer pointer and size
                            uint width = pc.points.width();
                            uint height = pc.points.height();
                            uint numPoints = width * height;
                            uint channels = 3;

                            byte[] managedBuffer = new byte[numPoints * channels * 4];
                            Marshal.Copy(pc.points.data(), managedBuffer, 0, managedBuffer.Length);

                            sf = new SingleFrame { Width = width, Height = height, Image = managedBuffer, PixelFormat = (pc.points.pixelFormat()).ToString(), PathFile = pathdir };

                            //if (!reflectanceImg.isEmpty())
                            //{
                            //    Common.NormalizeMinMax8U(reflectanceImg, pc.colors, true);              // compute colors from reflectance values. Function defined in cx_3d_common.                                
                            //}
                            //else
                            //{
                            //    Common.NormalizeMinMax8U(rangeImg, pc.colors, true);                    // compute colors from height values of range map. Function defined in cx_3d_common.                                
                            //}

                            //// Get buffer pointer and size                                               
                            //byte[] managedBuffer = new byte[(int)pc.colors.size()];
                            //Marshal.Copy(pc.colors.DataPointer(), managedBuffer, 0, (int)pc.colors.size());

                            //sf = new SingleFrame { Width = pc.colors.width(), Height = pc.colors.height(), Image = managedBuffer, PixelFormat = (pc.colors.pixelFormat()).ToString() };

                            //rangeImg.Dispose();
                            pc.Dispose();

                            //return sf;
                        }

                        // reflectanceImg.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, SingleAcq rangeImg Exception: " + ex.Message + ", " + ex.StackTrace);
                    }
                    finally
                    {
                        if (rangeImg != null && !rangeImg.isEmpty())
                            rangeImg.Dispose();
                        if (reflectanceImg != null && !reflectanceImg.isEmpty())
                            reflectanceImg.Dispose();
                    }

                    // 7.3. Queue back the buffer to the devices acquisition engine.
                    // \note From now on any references to the buffer images are not valid anymore and might be overwritten with new image data at any time!
                    buffer.queueBuffer();

                    return sf;
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ConfigurazioneProfilometer, SingleAcq Exception: " + ex.Message + ", " + ex.StackTrace);
                return null;
            }
        }*/
        public async Task<PointCloud> SingleAcqPc(Device cam)
        {
            try
            {
                Variant val = 0;

                _cam = cam;

                // 7.1. Grab acquisition buffer, wait for valid buffer with optional timeout. Timeout is given in ms.
                var buffer = _cam.waitForBuffer(3000, true);

                if (buffer.isValid())
                {
                    buffer.getInfo(cx_buffer_info.CX_BUFFER_INFO_IS_MULTIPART, val);
                    if (!(bool)val)
                    {
                        return null;
                    }

                    buffer.getInfo(cx_buffer_info.CX_BUFFER_INFO_NUM_PARTS, val);
                    int numParts = val;

                    // 7.2. Iterate the buffer parts and do some processing on the image data or get a copy for later use.
                    // \note img holds a reference to the image data in the DeviceBuffer, if you need the image data after cx_queueBuffer you need to clone the image!

                    uint region = 0;
                    AT.cx.NET.Image rangeImg = new AT.cx.NET.Image();
                    AT.cx.NET.Image reflectanceImg = new AT.cx.NET.Image();

                    for (int partIdx = 0; partIdx < numParts; partIdx++)
                    {
                        bool isImagePart = false;

                        // print some part information

                        buffer.getPartInfo(partIdx, cx_buffer_part_info.CX_BUFFER_PART_INFO_REGION_ID, val);
                        region = (uint)val & 0xf;

                        buffer.getPartInfo(partIdx, cx_buffer_part_info.CX_BUFFER_PART_INFO_TYPE_ID, val);
                        if ((int)val == (int)cx_buffer_part_type.CX_BUFFER_PART_TYPE_ID_IMAGE2D) isImagePart = true;

                        buffer.getPartInfo(partIdx, cx_buffer_part_info.CX_BUFFER_PART_INFO_DATA_PURPOSE_ID, val);

                        // get RangeImage for Region 0
                        if (region == 0 && isImagePart && (int)val == (int)cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_RANGE)
                        {
                            rangeImg = buffer.getImage(partIdx);
                        }

                        // get ReflectanceImage for Region 0
                        if (region == 0 && isImagePart && (int)val == (int)cx_buffer_part_purpose.CX_BUFFER_PART_PURPOSE_ID_REFLECTANCE)
                        {
                            reflectanceImg = buffer.getImage(partIdx);
                        }
                    }
                    try
                    {
                        if (!rangeImg.isEmpty())
                        {
                            // calculate point cloud
                            PointCloud pc = new PointCloud(rangeImg.height(), rangeImg.width());

                            PointCloud.calculatePointCloud(_calib, rangeImg, pc);

                            pc.computeNormals();                                                // compute normals from point cloud points

                            if (!reflectanceImg.isEmpty())
                            {
                                CommonProfilometri.NormalizeMinMax8U(reflectanceImg, pc.colors, true);              // compute colors from reflectance values. Function defined in cx_3d_common.                                
                            }
                            else
                            {
                                CommonProfilometri.NormalizeMinMax8U(rangeImg, pc.colors, true);                    // compute colors from height values of range map. Function defined in cx_3d_common.                                
                            }

                            return pc;
                        }

                        reflectanceImg.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("prova: " + ex.Message);
                        Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ProfilometerLib: SingleAcq rangeImg Exception: " + ex.Message + ", " + ex.StackTrace);
                    }
                    finally
                    {
                        Console.WriteLine("rangeImg: " + (rangeImg != null) + " " + rangeImg?.isEmpty());
                        if (rangeImg != null && !rangeImg.isEmpty())
                            rangeImg.Dispose();
                        if (reflectanceImg != null && !reflectanceImg.isEmpty())
                            reflectanceImg.Dispose();
                    }

                    // 7.3. Queue back the buffer to the devices acquisition engine.
                    // \note From now on any references to the buffer images are not valid anymore and might be overwritten with new image data at any time!
                    buffer.queueBuffer();
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ProfilometerLib: SingleAcq Exception: " + ex.Message + ", " + ex.StackTrace);
                return null;
            }
        }
        public async Task<HTuple> SingleAcqHimage(PointCloud pc)
        {
            try
            {
                var ptr = pc.points.data();

                var count = pc.points.width() * pc.points.height();

                float[] buffer = new float[count * 3];

                Marshal.Copy(ptr, buffer, 0, buffer.Length);

                List<(float x, float y, float z)> points = new List<(float x, float y, float z)>();

                var X = new HTuple();
                var Y = new HTuple();
                var Z = new HTuple();

                for (int i = 0; i < count; i++)
                {
                    float x = buffer[i * 3 + 0];
                    float y = buffer[i * 3 + 1];
                    float z = buffer[i * 3 + 2];

                    if (!float.IsNaN(z) || !float.IsNaN(x) || !float.IsNaN(y)) // tipico filtro depth invalid
                        X.Append(x);
                    Y.Append(y);
                    Z.Append(z);
                }

                HTuple oggetto = new HObjectModel3D();

                HOperatorSet.GenObjectModel3dFromPoints(
                    X,
                    Y,
                    Z,
                    out oggetto
                    );

                return oggetto;
            }
            catch (Exception ex)
            {
                Log.Error(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + $" - ProfilometerLib: SingleAcq Exception: " + ex.Message + ", " + ex.StackTrace);
                return null;
            }
        }
    }
}