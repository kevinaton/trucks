﻿using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetNextDispatchTodayResult
    {
        public GetNextDispatchTodayResult()
        {

        }

        public GetNextDispatchTodayResult(Guid dispatchGuid)
        {
            DispatchGuid = dispatchGuid;
            DispatchExists = true;
        }

        public bool DispatchExists { get; set; }
        public Guid DispatchGuid { get; set; }
    }
}
