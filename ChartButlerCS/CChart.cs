
namespace ChartButlerCS
{
    public class CChart
    {
        private string ChartName;
        private string ChartPath;

        public string GetChartName()
        {
            return ChartName;
        }

        public void SetChartName(string cname)
        {
            ChartName = cname;
        }

        public string GetChartPath()
        {
            return ChartPath;
        }

        public void SetChartPath(string cpath)
        {
            ChartPath = cpath;
        }
    }
}
