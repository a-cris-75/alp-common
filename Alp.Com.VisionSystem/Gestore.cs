using Emgu.CV.Ocl;
using Alp.Com.VisionSystem.ATProfilerHalcon.Configurazioni;
using Newtonsoft.Json;
using Serilog;
using Alp.Com.VisionSystem.ATProfilerHalcon.Status;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Alp.Com.VisionSystem.ATProfilerHalcon
{
    public class ConfigProfilometer
    {
        public string IP { get; set; }
        public string CONFIGFILE { get; set; }
        public string CALIBRATIONFILE { get; set; }
        public bool ENABLE { get; set; }

    }

    public class ConfigHalcon
    {
        public string HALCONMAIN { get; set; }
        public string HALCONPROCEDURES { get; set; }
        public string PROCEDURE { get; set; }
    }

    public class Utilities
    {
        public Dictionary<string, ConfigProfilometer> PROFILOMETERS { get; set; }

        public ConfigHalcon HALCON { get; set; }
    }
    public class Gestore
    {
        public Utilities Config { get; set; }
        private ProfilometerManager _profilometers = new ProfilometerManager();
        private HalconManager _halcon = new HalconManager();
        public async Task<bool> StartUp()
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("logs/log-dll-.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                if (!LoadConfig() || Config == null)
                {
                    Log.Error("While startup - problem with the configuration load, it got null");
                    return false;
                }

                Log.Information("While startup - Starting initialization Halcon and Profilometers!");
                
                _halcon.Init(Config.HALCON);

                Log.Information("While startup - Halcon Initialized!");

                _profilometers.Init(Config.PROFILOMETERS);

                Log.Information("While startup - Profilometers Initialized!");

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("While startup - Exception: " + ex.Message);
                return false;
            }
        }

        public int StatusProfilometer()
        {
            return _profilometers.GetStatus();
        }

        public int StatusHalcon()
        {
            return (int)_halcon.Status();
        }

        public int StatusDalsa()
        {
            //TODO: per fase 2
            return 0;
        }

        public async Task<List<ConfigurazioneHalcon.ProcedureDataOutput>> Execute(int nCycle = 0)
        {
            try
            {
                var halconStatus = StatusHalcon();
                var profilometerStatus = StatusProfilometer();
                var results = new List<ConfigurazioneHalcon.ProcedureDataOutput>();
                var allOk = halconStatus != (int)ProfilometerStatus.Status.Error && profilometerStatus != (int)HalconStatus.Status.Error;

                if (!allOk)
                {
                    Log.Error("While execute the procedure - problem with " + (halconStatus != (int)ProfilometerStatus.Status.Error ? "Profilometers and the config" : "Halcon and the config"));
                    return null;
                }

                if (nCycle == 0 || nCycle == 1)
                {
                    var data = await _profilometers.Execute();
                    var risultato = _halcon.UseProcedureProfilometer(data);

                    Log.Information("Execution ended and everything went well!");

                    results.Add(risultato);
                }
                else
                {
                    int i = 0;

                    Log.Information("Execution started for n: " + nCycle + " times!");

                    while (i < nCycle - 1)
                    {
                        var data = await _profilometers.Execute();
                        var risultato = _halcon.UseProcedureProfilometer(data);
                        i++;
                        results.Add(risultato);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Log.Error("While executing - exception: " + ex.Message);
                return null;
            }
        }

        public bool LoadConfig()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"config.json");
                
                if (!File.Exists(path)) return false;

                string jsonFile = File.ReadAllText(path);
                Config = Newtonsoft.Json.JsonConvert.DeserializeObject<Utilities>(jsonFile);
                if (Config == null)
                {
                    Log.Error("While loading configurations: -  JSON deserialization returned null");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("While loading configurations: - " + ex.Message);
                return false;
            }
        }
    }
}
