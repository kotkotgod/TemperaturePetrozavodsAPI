using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace TemperaturePetrozavodsAPI.Models
{
    public class TemperatureData
    {
        const int daySpan = 14; //количество дней, за которые нам нужна информация

        List<DayInfo> dayInfo = new List<DayInfo>();

        /*
         * Получение и обработка ответа от внешнего API 
         * Возвращаем List<DayInfo> с 14 объектами при успехе и пустой, если при получении данных произошла ошибка
         */
        public List<DayInfo> GetData()
        {
            HttpResponseMessage responseMessage = GetExternalData(daySpan).Result;

            //При успехе десериализуем данные и приводим в нужный формат
            if (responseMessage.IsSuccessStatusCode)
            {
                ExternalResponseData unformattedDayTemperatureData = DeserealizeExternalJSON(responseMessage.Content).Result;
                dayInfo = FormatResponse(unformattedDayTemperatureData, daySpan);
                dayInfo.Reverse();
            }
            return dayInfo;
        }

        /*
         * Запрос к внешнему API (https://api.open-meteo.com/) для получения данных о температуре 
         */
        async Task<HttpResponseMessage> GetExternalData(int days)
        {
            //Вычисляем даты для API запроса
            DateTime endDate = DateTime.Today.Date; ;
            DateTime startDate = DateTime.Today.AddDays(-days + 1);

            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.open-meteo.com/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            /*
             * latitude=61.78&longitude=34.35 - координаты Петрозаводска
             * timezone=Europe/Moscow - данные придут согласно указанному часовому поясу
             * start_date и end_date - левая и правая границы периода, за который получим данные
             * hourly=temperature_2m - почасовые данные о температуре
             * Подробнее тут https://open-meteo.com/en/docs
             */
            return await client.GetAsync($"v1/forecast?latitude=61.78&longitude=34.35&timezone=Europe/Moscow&start_date={startDate.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}&hourly=temperature_2m");
        }

        async Task<ExternalResponseData> DeserealizeExternalJSON(HttpContent content)
        {
            return await content.ReadFromJsonAsync<ExternalResponseData>();
        }
        
        /* 
         * Приведение полученных данных к требуему формату.
         * 
         * На входе HourlyReport с 2 массивами с day*24 элементов:
         * time - дата со временем с разбивкой по часу
         * temperature - температура в этот момент
         * 
         * На выходе List из days элементов struct DayInfo - days последних дней до текущей даты с датой и температурой в 4 точках дня 
         */
        List<DayInfo> FormatResponse(ExternalResponseData dataToFormat, int days)
        {
            List<DayInfo> innerDayInfoList = new List<DayInfo>();
            //добавляем days дней
            for (int i = 0; i < days; i++)
            {
                //индекс для перехода на следующий день, 24 так как разбивка почасовая
                int currentIndex = i * 24;
                innerDayInfoList.Add(new DayInfo(
                        dataToFormat.hourlyReport.time[currentIndex],               //дата
                        dataToFormat.hourlyReport.temperature[currentIndex],        //температура 0:00
                        dataToFormat.hourlyReport.temperature[currentIndex + 6],    //температура 6:00
                        dataToFormat.hourlyReport.temperature[currentIndex + 12],   //температура в 12:00
                        dataToFormat.hourlyReport.temperature[currentIndex + 18])   //температура в 18:00
                );
            }
            return innerDayInfoList;
        }
        
        //Классы для десериализации получаемого с внешнего API json
        class ExternalResponseData
        {
            [JsonPropertyName("hourly")]
            public HourlyReport hourlyReport { get; set; }
        }
        class HourlyReport
        {
            public List<DateTime> time { get; set; }
            [JsonPropertyName("temperature_2m")]
            public List<double> temperature { get; set; }
        }

    }

}
