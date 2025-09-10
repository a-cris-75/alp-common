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

        //public async Task<Esito> SpegniDeviceAsync(ushort idx, bool checkLock = false)
        //{
        //    _logger.Verbose($"SpegniTelecamereAsync... checkLock: [{checkLock}]");

        //    if (checkLock && lockAzioniTlc)
        //        return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sulle telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

        //    if (checkLock) lockAzioniTlc = true;

        //    Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Spegnimento telecamere andato a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

        //    try
        //    {
        //        if (await  reqRemotaInOutWebApi.GetOutAsync(idx) == true)
        //            _ = await reqRemotaInOutWebApi.UpdateOutAsync(idx, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        esito = new Esito { Eccezione = ex, Titolo = "Errore spegnimento telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
        //    }

        //    if (checkLock) lockAzioniTlc = false;

        //    _logger.Verbose($"SpegniTelecamereAsync... esito: [{esito}]");

        //    return esito;
        //}

        //public async Task<Esito> ResetDeviceAsync(ushort idx, bool checkLock = false)
        //{
        //    _logger.Verbose($"ResetTelecamereAsync... checkLock: [{checkLock}]");

        //    if (checkLock && lockAzioniTlc)
        //        return new Esito { Eccezione = null, Titolo = "Attenzione", Messaggio = "Attendere il termine dell'azione precedente sulle telecamere prima di effettuarne un'altra.", Ok = false, Icona = System.Drawing.SystemIcons.Warning };

        //    if (checkLock) lockAzioniTlc = true;

        //    Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Reset telecamere andato a buon fine", Ok = true, Icona = System.Drawing.SystemIcons.Information };

        //    try
        //    {
        //        esito = await SpegniDeviceAsync(idx);
        //        if (esito.Ok)
        //        {
        //            await Task.Delay(RITARDO_RESET_TELECAMERE_MS);
        //            esito = await VerificaEAccendiDeviceAsync(idx);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        esito = new Esito { Eccezione = ex, Titolo = "Errore reset telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
        //    }

        //    if (checkLock) lockAzioniTlc = false;

        //    _logger.Verbose($"ResetTelecamereAsync... esito: [{esito}]");

        //    return esito;
        //}

        //public async Task<Esito> VerificaStatoDeviceAsync(ushort idx)
        //{
        //    //_logger.Verbose($"VerificaStatoIlluminatoriAsync..."); // RIESUMARE..?

        //    Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono accesi", Ok = true, Icona = System.Drawing.SystemIcons.Information };

        //    try
        //    {

        //        Task<bool> task1 = reqRemotaInOutWebApi.GetOutAsync(idx);

        //        await Task.WhenAll(task1);

        //        bool areIlluminatoriOff = task1.Result;

        //        if (areIlluminatoriOff)
        //        {
        //            esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono spenti", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        esito = new Esito { Eccezione = ex, Titolo = "Errore verifica accensione illuminatori", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
        //    }

        //    //_logger.Verbose($"VerificaStatoIlluminatoriAsync... esito: [{esito}]");// RIESUMARE..?

        //    return esito;
        //}

        //public async Task<Esito> VerificaEAggiornaStatoIlluminatoriAsync()
        //{
        //    //_logger.Verbose($"VerificaEAggiornaStatoIlluminatoriAsync..."); // RIESUMARE..?

        //    Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono accesi", Ok = true, Icona = System.Drawing.SystemIcons.Information };

        //    try
        //    {

        //        Task<bool> task1 = _remotaInOutAzioniBaseIlluminatori.AreIlluminatoriOn();

        //        await Task.WhenAll(task1);

        //        StatoRemota.AreIlluminatoriOn = task1.Result;

        //        if (!StatoRemota.AreIlluminatoriOn)
        //        {
        //            esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Gli illuminatori sono spenti", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
        //        }

        //        StatoRemota.IsRemotaAlive = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        StatoRemota.IsRemotaAlive = false;
        //        esito = new Esito { Eccezione = ex, Titolo = "Errore verifica accensione illuminatori", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
        //    }

        //    //_logger.Verbose($"VerificaEAggiornaStatoIlluminatoriAsync... esito: [{esito}]");// RIESUMARE..?

        //    return esito;
        //}

        //public async Task<Esito> VerificaEAggiornaStatoTelecamereAsync()
        //{
        //    //_logger.Verbose($"VerificaEAggiornaStatoTelecamereAsync..."); // RIESUMARE..?

        //    Esito esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Le telecamere sono accese", Ok = true, Icona = System.Drawing.SystemIcons.Information };

        //    try
        //    {

        //        Task<bool> task1 = _remotaInOutAzioniBaseTelecamere.IsTlc1On();
        //        Task<bool> task2 = _remotaInOutAzioniBaseTelecamere.IsTlc2On();

        //        await Task.WhenAll(task1, task2);

        //        StatoRemota.IsTelecamera1On = task1.Result;
        //        StatoRemota.IsTelecamera2On = task2.Result;

        //        if (!StatoRemota.IsTelecamera1On && !StatoRemota.IsTelecamera2On)
        //        {
        //            esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "Le telecamere sono spente", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
        //        }
        //        else if (!StatoRemota.IsTelecamera1On)
        //        {
        //            esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "La telecamera n. 1 è spenta", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
        //        }
        //        else if (!StatoRemota.IsTelecamera1On)
        //        {
        //            esito = new Esito { Eccezione = null, Titolo = "Informazione", Messaggio = "La telecamera n. 2 è spenta", Ok = true, Icona = System.Drawing.SystemIcons.Exclamation };
        //        }

        //        StatoRemota.IsRemotaAlive = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        StatoRemota.IsRemotaAlive = false;
        //        esito = new Esito { Eccezione = ex, Titolo = "Errore verifica accensione telecamere", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
        //    }

        //    //_logger.Verbose($"VerificaEAggiornaStatoTelecamereAsync... esito: [{esito}]");// RIESUMARE..?

        //    return esito;
        //}


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

        //public async Task<bool> GetOutAsync(ushort id)
        //{
        //    bool res = false;
        //    HttpResponseMessage response = await httpClient.GetAsync("RemoteInOut/" + id.ToString());
        //    response.EnsureSuccessStatusCode();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        res = await response.Content.ReadAsAsync<bool>();
        //    }
        //    else
        //    {
        //        _logger.Error($"Errore in GetOutAsync con id [{id}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
        //    }

        //    return res;

        //}

        //public async Task<bool> UpdateOutAsync(ushort id, bool valore)
        //{
        //    bool res = false;

        //    HttpResponseMessage response = await httpClient.PutAsJsonAsync($"RemoteInOut/{id.ToString()}", valore);
        //    response.EnsureSuccessStatusCode();

        //    // Deserialize the updated product from the response body.
        //    // TODO capire perché restituisce sempre false (questo valore comunque poi non è utilizzato)
        //    res = await response.Content.ReadAsAsync<bool>();
        //    return res;
        //}
    }

}
