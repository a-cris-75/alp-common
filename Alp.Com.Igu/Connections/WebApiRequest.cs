using Alp.Com.DataAccessLayer.DataTypes;
//using Alp.Com.Igu.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Alp.Com.Igu.Connections
{
    internal class WebApiRequest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.Connections.WebApiRequest");

        private readonly HttpClient? httpClient;
        private readonly WebApiRequest? webApiConn;// = new WebApiRequest();
        private string URI = "ImpostazioniGenerali";
        private string serverIP = ApplicationSettingsStatic.ServerIP;
        //static string IP = "169.254.104.146"; //"localhost";

        public WebApiRequest()
        {
            try
            {
                httpClient = new HttpClient();
                // Update port # in the following line.
                httpClient.BaseAddress = new Uri("http://" + serverIP + ":710/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = TimeSpan.FromSeconds(6); // Il default è 100
            } catch { }
        }

        public WebApiRequest(string uri)
        {
            try
            {
                httpClient = new HttpClient();
                // Update port # in the following line.
                httpClient.BaseAddress = new Uri("http://" + serverIP + ":710/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = TimeSpan.FromSeconds(6); // Il default è 100

                URI = uri;
            }
            catch { }
        }

        public WebApiRequest? GetInstance() => webApiConn;
        

        public async Task<List<ImpostazioneGenerale>> GetSettingsAsync()
        {
            List<ImpostazioneGenerale> res = new List<ImpostazioneGenerale>();
            if (httpClient != null)
            {               
                //HttpResponseMessage response = await httpClient.GetAsync("ImpostazioniDia/" + id.ToString());
                HttpResponseMessage response = await httpClient.GetAsync(URI).ConfigureAwait(false); // NB: .ConfigureAwait(false); è necesssario solo se la chiamata avviene da WPF, non da servizio... boh?
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    //res = await response.Content.ReadAsAsync<List<ImpostazioneGenerale>>();
                    res = await response.Content.ReadAsAsync<List<ImpostazioneGenerale>>();
                }
                else
                {
                    log.Error($"Errore in GetSettingsAsync con uri [{URI}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                }
            }
            return res;

        }

        public async Task<bool> PostNewSettingsAsync(List<ImpostazioneGenerale> lstImpostazioneDia)
        {
            bool res = true;

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync(URI, lstImpostazioneDia).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Done PostImpostazioniAsync con uri [{URI}]");
                }
                else
                {
                    log.Error($"Errore in PostImpostazioniAsync con uri [{URI}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                    res = false;
                }
            }
            else res = false;
               
            return res;

        }

        public async Task<bool> PutReplaceSettingsAsync(List<ImpostazioneGenerale> lstImpostazioneDia)
        {
            bool res = true;

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.PutAsJsonAsync(URI, lstImpostazioneDia).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Done PostImpostazioniAsync con uri [{URI}]");
                }
                else
                {
                    log.Error($"Errore in PostImpostazioniAsync con uri [{URI}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                    res = false;
                }
            }
            else res = false;
            return res ;

        }

        /// <summary>
        /// Gestione di un'operazione generica definita su server Web Api, a cu paso l'URI (es: RemotaInOut) e l'identificativo dell'operazione (name operation)
        /// operations: è un'operazione definita su Server.
        /// Dovrei poter passare dei parametri: utilizzo jsoninput.
        /// ES: 
        /// - DoTouch
        /// - Read
        /// </summary>
        /// <param name="nameoperation"></param>
        /// <returns></returns>
        public async Task GetOperationAsync(string URI,  string nameoperation, string jsonInput) 
        {
            string URICOMPLETO = URI + @"/" + nameoperation;

            var request = new HttpRequestMessage(HttpMethod.Get, URICOMPLETO);
            request.Headers.Add("Api-json", jsonInput); // Add custom header

            try
            {
                HttpResponseMessage response = await httpClient.SendAsync(request);

                //HttpResponseMessage response = await httpClient.GetAsync(URICOMPLETO,).ConfigureAwait(false); // NB: .ConfigureAwait(false); è necesssario solo se la chiamata avviene da WPF, non da servizio... boh?

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    log.Info($"Done GetOperationAsync con uri [{URICOMPLETO}]");
                    await Task.Delay(500); // ESPERIMENTO. PARE CHE ALL'ARRIVO DEL SEGNALE IL FILE NON SIA ANCORA SCRITTO CORRETTAMENTE. QUI PERO' ENTRA DOPO...
                }
                else
                {
                    log.Error($"Errore in GetOperationAsync con uri [{URI}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                }
            }
            catch (Exception ex) 
            {
                log.Error("GetOperationAsync: " + ex.Message);

            }
            return;

        }

        /// <summary>
        /// Restituisce un bool sulla lettura da dispositivo (normnalmente remota)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> GetOutAsync(ushort id)
        {
            bool res = false;
            string uri = URI + @"/";

            try
            {
                if (httpClient != null)
                {
                    HttpResponseMessage response = await httpClient.GetAsync(uri + id.ToString());
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        res = await response.Content.ReadAsAsync<bool>();
                    }
                    else
                    {
                        log.Error($"Errore in GetOutAsync con id [{id}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                    }
                }
            }
            catch { }
            return res;

        }

        public async Task<bool> UpdateOutAsync(ushort id, bool valore)
        {
            bool res = false;
            string uri = URI + @"/";
            HttpResponseMessage response = await httpClient.PutAsJsonAsync(uri + id.ToString() , valore);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            // TODO capire perché restituisce sempre false (questo valore comunque poi non è utilizzato)
            res = await response.Content.ReadAsAsync<bool>();
            return res;
        }

        public async Task<string> GetOutPlcAsync(string ip, int block, int word, string type)
        {
            string res = string.Empty;
            string uri = URI + @"/" + ip + @"|"  + block.ToString() + @"|" + word + @"|" + type;

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsAsync<string>();
                }
                else
                {
                    log.Error($"Errore in GetOutAddressAsync con uri [{uri}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                }
            }
            return res;

        }

        public async Task<string> GetOutPlcAsync(int idx, string type)
        {
            string res = string.Empty;
            string uri = URI + @"/" + idx.ToString() + @"|" + type;

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsAsync<string>();
                }
                else
                {
                    log.Error($"Errore in GetOutAddressAsync con uri [{uri}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                }
            }
            return res;

        }

        /// <summary>
        /// Metodo gestito dal Servizio WEbApi, che riceve nell'uri le informazioni sull'ip del dispositivo e la stringa status che identifica la richiesta
        /// di informnazioni sullo stato (cioè se l'ip è raggiungibile).
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<bool> GetOutDevStatusAsync(string ip)
        {
            bool res = false;
            string uri = URI + @"/Status/";
            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(uri + ip);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsAsync<bool>();
                }
                else
                {
                    log.Error($"Errore in GetOutDevStatusAsync con ip [{ip}]: StatusCode: [{(int)response.StatusCode}], ReasonPhrase: [{response.ReasonPhrase}]");
                }
            }
            return res;

        }

    }
}

