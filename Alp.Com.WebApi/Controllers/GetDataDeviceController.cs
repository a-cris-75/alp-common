using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alp.Com.DataAccessLayer.DataTypes;

namespace Alp.Com.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetDataDeviceController : ControllerBase
    {

        private readonly ILogger<GetDataDeviceController> _logger;
        private readonly ApplicationSettings _options;
         public GetDataDeviceController(ILogger<GetDataDeviceController> logger, ApplicationSettings options)
        {
            _logger = logger;
            _options = options;
        }


      

        [HttpGet]
        //public async Task<ActionResult<List<ImpostazioneDia>>> GetAsync()
        public async Task<List<ImpostazioneDia>> GetAsync()
        {
            try
            {
                _logger.LogInformation($"Chiamata a GetAsync");

                List<ImpostazioneDia> lstImpostazioneDia = await GestioneImpostazioniDia.GetInstance.LeggiFileImpostazioniAsync(_options.PercorsoFileImpostazioniDia);

                return lstImpostazioneDia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eccezione");
                throw;
            }
        }

        


        // POST api/values
        [HttpPost]
        public async Task PostAsync([FromBody] List<ImpostazioneDia> lstImpostazioneDia)
        {
            try
            {
                _logger.LogInformation($"Chiamata a PostAsync({lstImpostazioneDia.ToString()})");

                await GestioneImpostazioniDia.GetInstance.SalvaFileImpostazioniAsync(_options.PercorsoFileImpostazioniDia, lstImpostazioneDia);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eccezione");
                throw;
            }
        }

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
