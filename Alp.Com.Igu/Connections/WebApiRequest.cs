using Alp.Com.Igu.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Alp.Com.Igu.Connections
{
    internal class WebApiRequest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.Connections.WebApiRequest");

        private static readonly HttpClient httpClient;
        private static readonly WebApiRequest webApiConn = new WebApiRequest();
        static string URI = "ImpostazioniGenerali";
        static string serverIP = ApplicationSettingsStatic.ServerIP;
        //static string IP = "169.254.104.146"; //"localhost";

        static WebApiRequest()
        {
            httpClient = new HttpClient();
            // Update port # in the following line.
            httpClient.BaseAddress = new Uri("http://" + serverIP + ":710/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.Timeout = TimeSpan.FromSeconds(6); // Il default è 100
        }

        public static WebApiRequest GetInstance() => webApiConn;
        

        public async Task<List<ImpostazioneGenerale>> GetSettingsAsync()
        {
            List<ImpostazioneGenerale> res = new List<ImpostazioneGenerale>();
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

            return res;

        }

        public async Task PostNewSettingsAsync(List<ImpostazioneGenerale> lstImpostazioneDia)
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
            }

            return;

        }

        public async Task PutReplaceSettingsAsync(List<ImpostazioneGenerale> lstImpostazioneDia)
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
            }

            return;

        }

        /// <summary>
        /// operations: è un'operazione definita su Server.
        /// Dovrei poter passare dei parametri.
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

            return;

        }

      

    }
}
