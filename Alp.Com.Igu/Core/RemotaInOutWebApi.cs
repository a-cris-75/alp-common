using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
//using AlpTlc.Connessione;
//using AlpTlc.Biz.Core;
//using AlpTlc.Connessione.WebAPI.RemotaInOut;
using System.Threading;
using Alp.Com.Igu.Connections;
//using AlpTlc.Domain.StatoMacchina;

namespace Alp.Com.Igu.Core
{
    /// <summary>
    /// Gestione azioni su remota tramite Web Api
    /// </summary>
    public class RemotaInOutWebApi
    {

        Serilog.ILogger _logger = Serilog.Log.ForContext(typeof(RemotaInOutWebApi));

        //private static readonly RemotaInOutAzioniWebApi remotaInOutAzioni = new RemotaInOutAzioniWebApi();

        private ushort DO_INDEX;
        private string? DEV_NAME;

        public bool isInitialized = false;

        private readonly WebApiRequest reqRemotaInOutWebApi = new WebApiRequest("RemoteInOut");//WebApiRequest.GetInstance();

        //private static readonly RemotaInOutAzioniBaseTelecamere _remotaInOutAzioniBaseTelecamere = RemotaInOutAzioniBaseTelecamere.GetInstance();
        //private static readonly RemotaInOutAzioniBaseIlluminatori _remotaInOutAzioniBaseIlluminatori = RemotaInOutAzioniBaseIlluminatori.GetInstance();

        //static ushort RITARDO_RESET_TELECAMERE_MS;
        //static ushort RITARDO_ACCENSIONE_ILLUMINATORI_MS;

        //bool lockAzioniIlluminatori = false;
        //bool lockAzioniTlc = false;

        static RemotaInOutWebApi()
        {
        }

        public RemotaInOutWebApi(ushort index, string? namedev)
        {
            _logger.Verbose("RemotaInOutAzioni ctor-");
            DO_INDEX = index;
            DEV_NAME = "DEVICE " + index.ToString();
            if (namedev != null)
            {
                DEV_NAME = namedev;
            }
        }

      

        public async Task<Esito> GetStatusEsito()
        {
            bool res = true;
            Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", 
                Messaggio = "Status device OK (" + this.DEV_NAME + ")", 
                Ok = true, 
                Icona = System.Drawing.SystemIcons.Information };

            try
            {
                res = await reqRemotaInOutWebApi.GetOutAsync(DO_INDEX);
            }
            catch (Exception ex)
            {
                esito = new Esito { Eccezione = ex, Titolo = "Errore", 
                    Messaggio = "Status device NOT OK (" + this.DEV_NAME + "): " + ex.Message, Ok = false, 
                    Icona = System.Drawing.SystemIcons.Error };
            }
            return esito; 
        }

        public async Task<bool> GetStatus()
        {
            return await reqRemotaInOutWebApi.GetOutAsync(DO_INDEX);
        }

        public async Task<bool> IsStatus(bool value)
        {
            if (!isInitialized) throw new Exception("Indice non inizializzato");
            return (await reqRemotaInOutWebApi.GetOutAsync(DO_INDEX) == value);
        }


        public async Task<bool> IsStatusOn()
        {
            return await IsStatus(true);
        }


        public async Task<bool> IsStatusOff()
        {
            return await IsStatus(false);
        }

        public async Task<bool> SwitchStatus(bool value)
        {
            if (!isInitialized) throw new Exception("Indice non inizializzato");
            return await reqRemotaInOutWebApi.UpdateOutAsync(DO_INDEX, value);
        }

        public async Task<bool> SwitchStatusOn()
        {
            return await SwitchStatus(true);
        }

        public async Task<bool> SwitchStatusOff()
        {
            return await SwitchStatus(false);
        }

        #region EXAMPLES
        //public RemotaInOutAzioniWebApi GetInstance(ushort idx)// => remotaInOutAzioni;
        //{
        //    return new RemotaInOutAzioniWebApi(idx);
        //}



        //public async Task<Esito> VerificaEAccendiDeviceAsync(ushort idx, bool checkLock = false)
        //{
        //    _logger.Verbose($"VerificaEAccendiTelecamereAsync... checkLock: [{checkLock}]");

        //    if (checkLock && lockAzioniTlc)
        //        return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sulle telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

        //    if (checkLock) lockAzioniTlc = true;

        //    Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Accensione telecamere andata a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

        //    try
        //    {
        //        //if (await _remotaInOutAzioniBaseTelecamere.IsTlc1Off())
        //        if (await reqRemotaInOutWebApi.GetOutAsync(idx))
        //        {
        //            _logger.Information($"VerificaEAccendiTelecamereAsync: la telecamera 1 è spenta. Accendiamola...");
        //            _ = await reqRemotaInOutWebApi.UpdateOutAsync(idx, true);
        //        }

        //        //if (await requestApi.GetOutAsync(idx))
        //        //{
        //        //    _logger.Information($"VerificaEAccendiTelecamereAsync: la telecamera 2 è spenta. Accendiamola...");
        //        //    _ = await _remotaInOutAzioniBaseTelecamere.SwitchTlc2On();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        esito = new Esito { Eccezione = ex, Titolo = "Errore accensione telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
        //    }

        //    if (checkLock) lockAzioniTlc = false;

        //    _logger.Verbose($"VerificaEAccendiTelecamereAsync... esito: [{esito}]");

        //    return esito;
        //}


        #endregion
    }

}
