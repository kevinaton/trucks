﻿namespace DispatcherWeb.Web.Utils
{
    internal class RSAParametersJson
    {
        public RSAParametersJson()
        {
        }

        public string Modulus { get; set; }
        public string Exponent { get; set; }
        public string P { get; set; }
        public string Q { get; set; }
        public string DP { get; set; }
        public string DQ { get; set; }
        public string InverseQ { get; set; }
        public string D { get; set; }
    }
}