﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileTimelineView.Facebook
{
    public class FacebookDownloader : DataDownloader
    {
        public override Task<List<TimelineData>> GetTimelineAsync()
        {
            throw new NotImplementedException();
        }
    }
}
