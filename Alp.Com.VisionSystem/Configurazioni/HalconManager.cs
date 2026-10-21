using HalconDotNet;
//using Serilog;
using Alp.Com.VisionSystem.ATProfilerHalcon.Status;
using System;
using static Alp.Com.VisionSystem.ATProfilerHalcon.Configurazioni.ConfigurazioneHalcon;

namespace Alp.Com.VisionSystem.ATProfilerHalcon.Configurazioni
{
    public class HalconManager
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
                            ("HalconManager".PadRight(20, ' '));

        private HDevEngine _engine;
        private ConfigHalcon _config;

        public HalconStatus.Status Init(ConfigHalcon config)
        {
            try
            {
                _config = config;

                _engine = new HDevEngine();
                _engine.SetProcedurePath(_config.HALCONMAIN);

                Log.Info("Halcon engine initialized");

                return HalconStatus.Status.Initializing;
            }
            catch (Exception ex)
            {
                Log.Error("Halcon Init error: " + ex.Message);
                return HalconStatus.Status.Error;
            }
        }

        public HalconStatus.Status Status()
        {
            return _engine != null && _engine.IsInitialized()
                ? HalconStatus.Status.Ready
                : HalconStatus.Status.Error;
        }

        public ProcedureDataOutput UseProcedureProfilometer(HTuple[] values)
        {
            try
            {

                using (var procedure = new HDevProcedure(_config.HALCONPROCEDURES))
                using (var call = new HDevProcedureCall(procedure))
                {
                    var result = new ProcedureDataOutput();

                    // TODO: da mettere a posto solo per il ciclo for per i valori

                    call.SetInputCtrlParamTuple($"ObjectModel3D1", values[1]);
                    call.SetInputCtrlParamTuple($"ObjectModel3D2", values[0]);

                    Log.Info($"{nameof(UseProcedureProfilometer)} - starting procedure");
                    call.Execute();
                    Log.Info($"{nameof(UseProcedureProfilometer)} - procedure finished");

                    var Output1 = call.GetOutputCtrlParamTuple("Diagonale1").ToString();
                    var Output2 = call.GetOutputCtrlParamTuple("Diagonale2").ToString();
                    var Romboidita = call.GetOutputCtrlParamTuple("Romboidity").ToString();

                    result.Output1 = (Output1 != null && Output1 != "") ? float.Parse(Output1) : (float)0;
                    result.Output2 = (Output2 != null && Output2 != "") ? float.Parse(Output2) : (float)0;
                    result.Romboidita = (Romboidita != null && Romboidita != "[]") ? float.Parse(Romboidita) : (float)0;

                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(UseProcedureProfilometer)} error: {ex.Message}");
                return null;
            }
        }
    }
}