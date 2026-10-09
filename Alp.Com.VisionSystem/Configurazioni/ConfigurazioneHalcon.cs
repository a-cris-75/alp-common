using HalconDotNet;
//using LibreriaHalconProfiloDalsa;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Alp.Com.VisionSystem.ATProfilerHalcon.Status;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alp.Com.VisionSystem.ATProfilerHalcon.Configurazioni
{
    public class ConfigurazioneHalcon
    {
        private HDevEngine _engine;

        public class ProcedureDataOutput
        {
            public float Output1 { get; set; }
            public float Output2 { get; set; }
            public float Romboidita { get; set; }
        }

        public ProcedureDataOutput result;

        public ConfigHalcon _halconData { get; set; }

        public async void Init(ConfigHalcon halcon)
        {
            try
            {
                _halconData = halcon;
                _engine = new HDevEngine();
                _engine.SetProcedurePath(_halconData.HALCONMAIN);

                Log.Information("Prova");

                Log.Information("Init." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " Engine started!");
                //return HalconStatus.Status.Initializing;
            }
            catch (Exception ex)
            {
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " " + HalconStatus.Status.Error + ": " + ex.Message);
                //return HalconStatus.Status.Error;
            }
        }

        public ProcedureDataOutput UseProcedureProfilometer(HTuple[] values)
        {
            try
            {
                if (_halconData == null)
                    throw new InvalidOperationException("Halcon non inizializzato");

                using (var procedure = new HDevProcedure(_halconData.HALCONPROCEDURES))
                using (var call = new HDevProcedureCall(procedure))
                {
                    var result = new ProcedureDataOutput();

                    for (int i = 0; i < values.Length; i++)
                    {
                        call.SetInputCtrlParamTuple($"ObjectModel3D{i+1}", values[i]);
                    }

                    Log.Information($"{nameof(UseProcedureProfilometer)} - starting procedure");
                    call.Execute();
                    Log.Information($"{nameof(UseProcedureProfilometer)} - procedure finished");

                    

                    var Output1 = call.GetOutputCtrlParamTuple("Diagonale1");
                    var Output2 = call.GetOutputCtrlParamTuple("Diagonale2");
                    var Romboidita = call.GetOutputCtrlParamTuple("Romboidity");

                    result.Output1 = (Output1 != null && Output1 != "") ? float.Parse(Output1) : 0;
                    result.Output2 = Output2 != null && Output2 != "" ? float.Parse(Output2) : 0;
                    result.Romboidita = Romboidita != null && Romboidita != "" ? float.Parse(Romboidita) : 0;

                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(UseProcedureProfilometer)} error: {ex.Message}");
                return null;
            }
        }

        /*public ProcedureDataOutput UseProcedureProfilometer(HTuple [] values)
        {
            try
            {
                using (HDevProcedure _procedure = new HDevProcedure(_halconData.HALCONPROCEDURES))
                {
                    using (HDevProcedureCall _procedureCall = new HDevProcedureCall(_procedure))
                    {
                        var result = new ProcedureDataOutput();
                        for (var i = 0; i < values.Length; i++)
                        {
                            _procedureCall.SetInputCtrlParamTuple("ObjectModel3D" + i, values[i]);
                        }

                        // esecuzione della procedura di Halcon
                        Log.Information(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - HalconProcedures loaded and starting!");
                        _procedureCall.Execute();

                        // eseguita la procedura di Halcon
                        Log.Information(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - HalconProcedures ended and outputs are generated!");

                        var Output1 = (_procedureCall.GetOutputCtrlParamTuple("Diagonale1")).ToString();
                        var Output2 = (_procedureCall.GetOutputCtrlParamTuple("Diagonale2")).ToString();
                        var Romboidita = (_procedureCall.GetOutputCtrlParamTuple("Romboidity")).ToString();

                        result.Output1 = (Output1 != null && Output1 != "") ? float.Parse(Output1) : 0;
                        result.Output2 = Output2 != null && Output2 != "" ? float.Parse(Output2) : 0;
                        result.Romboidita = Romboidita != null && Romboidita != "" ? float.Parse(Romboidita) : 0;

                        Log.Information(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - HalconProcedures end!");

                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - HalconProcedures had generated an error: " + e.Message);
                return null;
            }
            finally
            {
                values = null;
            }

        }*/

        public int CheckStatus()
        {
            // checkHalcon
            //Logger.LogInformation(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " " + Status.HalconStatus.Status.Initializing);
            return _engine.IsInitialized() ? (int)HalconStatus.Status.Ready : (int)HalconStatus.Status.Error;
        }

        /*public Task<HalconStatus.Status> RestartHalcon()
        {
            // restartHalcon - annullo il motore e lo faccio ripartire
            //Logger.LogInformation(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " " + Status.HalconStatus.Status.Initializing);
            if (_engine.IsInitialized())
            {
                _engine = null;
            }

            return Init(_halconData);

        }*/
    }
}