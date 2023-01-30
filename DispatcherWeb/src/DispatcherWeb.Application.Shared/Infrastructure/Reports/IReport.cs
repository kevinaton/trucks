﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.Reports
{
    public interface IReport
    {
		void AddReportHeader(string text);
		void AddHeader(string text);
		void AddText(string text);
		void AddEmptyLine();
	}
}
