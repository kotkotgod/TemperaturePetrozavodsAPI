using Microsoft.AspNetCore.Mvc;
using TemperaturePetrozavodsAPI.Models;

namespace WeatherPetrozavodsk.Controllers
{
    //доступ к api по имени контроллера = /api/Temperature
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            TemperatureData td = new TemperatureData();
            
            List<DayInfo> toSend = td.GetData();
            
            if (toSend.Count != 0)
                return Ok(toSend);
            else
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}