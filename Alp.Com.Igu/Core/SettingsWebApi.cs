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
    public class SettingsWebApi
    {

        Serilog.ILogger _logger = Serilog.Log.ForContext(typeof(RemotaInOutWebApi));

        public bool isInitialized = false;
        private readonly WebApiRequest reqRemotaInOutWebApi = new WebApiRequest("ImpostazioniGenerali");//WebApiRequest.GetInstance();


        public SettingsWebApi()
        {
            _logger.Verbose("SettingsWebApi ctor-");
        }

  
        public async Task<List<DataAccessLayer.DataTypes.ImpostazioneGenerale>> GetSettings()
        {
            return await reqRemotaInOutWebApi.GetSettingsAsync();
        }

        public async Task<bool> SetSettings(List<DataAccessLayer.DataTypes.ImpostazioneGenerale> lstSettings)
        {
            if (!isInitialized) throw new Exception("Indice non inizializzato");
            return (await reqRemotaInOutWebApi.PostNewSettingsAsync(lstSettings));
        }


      

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
