using System.Text.Json;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Bot
{
    
        public class WeatherClients
        {
            private static string _addres;
            private static string _apikey;
            private static string _apihost;
            public WeatherClients()
            {
                _addres = Constants.Address;
                _apikey = Constants.ApiKey;
                _apihost = Constants.ApiHost;
            }
            public async Task<_3_hour_Forecast_5_days> GetWeatherClients(double Lat, double Lon)
            {

                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_addres}/{Lat}/{Lon}"),
                    Headers =
                {
                 { "X-RapidAPI-Key", _apikey },
                 { "X-RapidAPI-Host", _apihost },
                },
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<_3_hour_Forecast_5_days>(body);
                    return result;
                }

            }
        }
}



