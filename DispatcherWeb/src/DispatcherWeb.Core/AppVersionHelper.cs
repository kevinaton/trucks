﻿using System;
using System.IO;
using Abp.Reflection.Extensions;

namespace DispatcherWeb
{
    /// <summary>
    /// Central point for application version.
    /// </summary>
    public class AppVersionHelper
    {
        /// <summary>
        /// Gets current version of the application.
        /// It's also shown in the web page.
        /// </summary>
        public static string Version => typeof(AppVersionHelper).GetAssembly().GetName().Version.ToString();

        /// <summary>
        /// Gets release (last build) date of the application.
        /// It's shown in the web page.
        /// </summary>
        public static DateTime ReleaseDate => new FileInfo(typeof(AppVersionHelper).GetAssembly().Location).LastWriteTime;
    }
}