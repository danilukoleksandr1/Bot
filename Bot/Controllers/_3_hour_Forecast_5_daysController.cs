using Microsoft.AspNetCore.Mvc;

namespace Bot.Controllers
{
  
        [ApiController]
        [Route("controller")]
        public class _3_hour_Forecast_5_daysController : ControllerBase
        {
            private readonly ILogger<_3_hour_Forecast_5_daysController> _logger;
            public _3_hour_Forecast_5_daysController(ILogger<_3_hour_Forecast_5_daysController> logger)
            {
                _logger = logger;
            }
            [HttpGet]
            public _3_hour_Forecast_5_days JsonModels(double lat, double lon)
            {
                //Database dt = new Database();
                WeatherClients weatherClients = new WeatherClients();
                //dt.InsertWeatherAsync(weatherClients.GetWeatherClients(lat, lon).Result);
                _3_hour_Forecast_5_days forecast_5_Days = weatherClients.GetWeatherClients(lat, lon).Result;
                return forecast_5_Days;
            }

            [HttpPost]
            public _3_hour_Forecast_5_days PostTest(double lat, double lon)
            {
                WeatherClients weatherClients = new WeatherClients();
                _3_hour_Forecast_5_days forecast_5_Days = weatherClients.GetWeatherClients(lat, lon).Result;
                return forecast_5_Days;
            }

            [HttpPut]
            public string PutTest(string p)
            {
                return p + p;
            }

            [HttpDelete]
            public string DeleteTest(string p)
            {
                if (p.Length > 5)
                {
                    return p;

                }
                else
                {
                    return "0";
                }
            }
        }
}

