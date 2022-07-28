using System.Collections.Generic;

namespace DCMS.Services.Global.Common
{

    public class AcceptTime
    {
        public List<TimeInterval> start { get; set; }
        public List<TimeInterval> end { get; set; }
    }
    public class TimeInterval
    {
        public int hour { get; set; }
        public int min { get; set; }
    }
}
