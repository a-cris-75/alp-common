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
    public class PlcInOutWebApi
    {

        Serilog.ILogger _logger = Serilog.Log.ForContext(typeof(RemotaInOutWebApi));

        //private static readonly RemotaInOutAzioniWebApi remotaInOutAzioni = new RemotaInOutAzioniWebApi();

        private string? PLC_IP;
        private int PLC_BLOCK;
        private string? PLC_NAME;
        // in alternativa all'indirizzo esplicito
        private int IDX_PLC = 0;

        public bool isInitialized = false;

        private readonly WebApiRequest reqPlcInOutWebApi = new WebApiRequest("PlcInOut");//WebApiRequest.GetInstance();


        public PlcInOutWebApi(int idx, string? name)
        {
            IDX_PLC = idx;
            PLC_NAME = "PLC " + idx.ToString();
            if (!string.IsNullOrEmpty(name))
                PLC_NAME = name;
        }

        private PlcInOutWebApi(string ip, int block)
        {
            _logger.Verbose("PlcInOutWebApi ctor-");

            PLC_IP = ip;
            PLC_BLOCK = block;
        }

     
        public async Task<bool> GetStatus()
        {
            return await reqPlcInOutWebApi.GetOutDevStatusAsync(PLC_IP);
        }

        public async Task<bool> IsStatus(bool value)
        {
            if (!isInitialized) throw new Exception("Indice non inizializzato");
            return (await reqPlcInOutWebApi.GetOutDevStatusAsync(PLC_IP) == value);
        }


        public async Task<bool> IsStatusOn()
        {
            return await IsStatus(true);
        }


        public async Task<bool> IsStatusOff()
        {
            return await IsStatus(false);
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
