namespace TemperaturePetrozavodsAPI.Models
{
    //Структуры данных для отправки
    public struct DayInfo
    {
        public DateTime Date { get; set; }
        public Temperatures Temperature { get; set; }
        public DayInfo(DateTime d, double n, double m, double a, double e)
        {
            Date = d;
            Temperature = new Temperatures(n, m, a, e);
        }
    }
    //Температура в 4 точках
    public struct Temperatures
    {
        public Temperatures(double night, double morning, double afternoon, double evening)
        {
            Night = night;
            Morning = morning;
            Afternoon = afternoon;
            Evening = evening;
        }
        public double Night { get; set; }
        public double Morning { get; set; }
        public double Afternoon { get; set; }
        public double Evening { get; set; }
    }
}
