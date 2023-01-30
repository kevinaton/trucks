﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Features;
using DispatcherWeb.Features;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class FeatureCheckerExtensions
    {
        public static async Task<bool> AllowLeaseHaulersFeature(this IFeatureChecker featureChecker) =>
            await featureChecker.IsEnabledAsync(AppFeatures.AllowLeaseHaulersFeature);

        public static async Task<bool> AllowMultiOfficeFeature(this IFeatureChecker featureChecker) =>
            await featureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature);


    }
}
