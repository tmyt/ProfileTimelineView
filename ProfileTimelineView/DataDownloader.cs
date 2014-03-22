using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProfileTimelineView
{
    public abstract class DataDownloader
    {
        public abstract Task<List<TimelineData>> GetTimelineAsync();
    }
}
