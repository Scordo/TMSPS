﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TMSPS.Core.Common
{
    public class Util
    {
        public static bool WaitUntilReadable(string path, uint intMaxTimeoutInMilliSeconds)
        {
            DateTime start = DateTime.Now;

            do
            {
                if (IsFileReadable(path))
                    return true;
            } while (start.AddMilliseconds(intMaxTimeoutInMilliSeconds) <= DateTime.Now);

            return false;
        }

        public static bool IsFileReadable(string path)
        {
            try
            {
                File.OpenRead(path).Close();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

		public static string GetCalculatedPath(string filename)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			if (Path.IsPathRooted(filename))
				return filename;

			string binaryDicrectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			return Path.Combine(binaryDicrectory, filename);
		}

        public static string GetCountryShortCut(string countryName)
        {
            Dictionary<string, string>  countryMappings = new Dictionary<string, string>
            {
                {"Afghanistan", "AFG"},
                {"Albania", "ALB"},
                {"Algeria", "ALG"},
                {"Andorra", "AND"},
                {"Angola", "ANG"},
                {"Argentina", "ARG"},
                {"Armenia", "ARM"},
                {"Aruba", "ARU"},
                {"Australia", "AUS"},
                {"Austria", "AUT"},
                {"Azerbaijan", "AZE"},
                {"Bahamas", "BAH"},
                {"Bahrain", "BRN"},
                {"Bangladesh", "BAN"},
                {"Barbados", "BAR"},
                {"Belarus", "BLR"},
                {"Belgium", "BEL"},
                {"Belize", "BIZ"},
                {"Benin", "BEN"},
                {"Bermuda", "BER"},
                {"Bhutan", "BHU"},
                {"Bolivia", "BOL"},
                {"Bosnia&Herzegovina", "BIH"},
                {"Botswana", "BOT"},
                {"Brazil", "BRA"},
                {"Brunei", "BRU"},
                {"Bulgaria", "BUL"},
                {"Burkina Faso", "BUR"},
                {"Burundi", "BDI"},
                {"Cambodia", "CAM"},
                {"Cameroon", "CAR"},
                {"Canada", "CAN"},
                {"Cape Verde", "CPV"},
                {"Central African Republic", "CAF"},
                {"Chad", "CHA"},
                {"Chile", "CHI"},
                {"China", "CHN"},
                {"Chinese Taipei", "TPE"},
                {"Colombia", "COL"},
                {"Congo", "CGO"},
                {"Costa Rica", "CRC"},
                {"Croatia", "CRO"},
                {"Cuba", "CUB"},
                {"Cyprus", "CYP"},
                {"Czech Republic", "CZE"},
                {"Czech republic", "CZE"},
                {"DR Congo", "COD"},
                {"Denmark", "DEN"},
                {"Djibouti", "DJI"},
                {"Dominica", "DMA"},
                {"Dominican Republic", "DOM"},
                {"Ecuador", "ECU"},
                {"Egypt", "EGY"},
                {"El Salvador", "ESA"},
                {"Eritrea", "ERI"},
                {"Estonia", "EST"},
                {"Ethiopia", "ETH"},
                {"Fiji", "FIJ"},
                {"Finland", "FIN"},
                {"France", "FRA"},
                {"Gabon", "GAB"},
                {"Gambia", "GAM"},
                {"Georgia", "GEO"},
                {"Germany", "GER"},
                {"Ghana", "GHA"},
                {"Greece", "GRE"},
                {"Grenada", "GRN"},
                {"Guam", "GUM"},
                {"Guatemala", "GUA"},
                {"Guinea", "GUI"},
                {"Guinea-Bissau", "GBS"},
                {"Guyana", "GUY"},
                {"Haiti", "HAI"},
                {"Honduras", "HON"},
                {"Hong Kong", "HKG"},
                {"Hungary", "HUN"},
                {"Iceland", "ISL"},
                {"India", "IND"},
                {"Indonesia", "INA"},
                {"Iran", "IRI"},
                {"Iraq", "IRQ"},
                {"Ireland", "IRL"},
                {"Israel", "ISR"},
                {"Italy", "ITA"},
                {"Ivory Coast", "CIV"},
                {"Jamaica", "JAM"},
                {"Japan", "JPN"},
                {"Jordan", "JOR"},
                {"Kazakhstan", "KAZ"},
                {"Kenya", "KEN"},
                {"Kiribati", "KIR"},
                {"Korea", "KOR"},
                {"Kuwait", "KUW"},
                {"Kyrgyzstan", "KGZ"},
                {"Laos", "LAO"},
                {"Latvia", "LAT"},
                {"Lebanon", "LIB"},
                {"Lesotho", "LES"},
                {"Liberia", "LBR"},
                {"Libya", "LBA"},
                {"Liechtenstein", "LIE"},
                {"Lithuania", "LTU"},
                {"Luxembourg", "LUX"},
                {"Macedonia", "MKD"},
                {"Malawi", "MAW"},
                {"Malaysia", "MAS"},
                {"Mali", "MLI"},
                {"Malta", "MLT"},
                {"Mauritania", "MTN"},
                {"Mauritius", "MRI"},
                {"Mexico", "MEX"},
                {"Moldova", "MDA"},
                {"Monaco", "MON"},
                {"Mongolia", "MGL"},
                {"Montenegro", "MNE"},
                {"Morocco", "MAR"},
                {"Mozambique", "MOZ"},
                {"Myanmar", "MYA"},
                {"Namibia", "NAM"},
                {"Nauru", "NRU"},
                {"Nepal", "NEP"},
                {"Netherlands", "NED"},
                {"New Zealand", "NZL"},
                {"Nicaragua", "NCA"},
                {"Niger", "NIG"},
                {"Nigeria", "NGR"},
                {"Norway", "NOR"},
                {"Oman", "OMA"},
                {"Other Countries", "OTH"},
                {"Pakistan", "PAK"},
                {"Palau", "PLW"},
                {"Palestine", "PLE"},
                {"Panama", "PAN"},
                {"Paraguay", "PAR"},
                {"Peru", "PER"},
                {"Philippines", "PHI"},
                {"Poland", "POL"},
                {"Portugal", "POR"},
                {"Puerto Rico", "PUR"},
                {"Qatar", "QAT"},
                {"Romania", "ROM"},
                {"Russia", "RUS"},
                {"Rwanda", "RWA"},
                {"Samoa", "SAM"},
                {"San Marino", "SMR"},
                {"Saudi Arabia", "KSA"},
                {"Senegal", "SEN"},
                {"Serbia", "SCG"},
                {"Sierra Leone", "SLE"},
                {"Singapore", "SIN"},
                {"Slovakia", "SVK"},
                {"Slovenia", "SLO"},
                {"Somalia", "SOM"},
                {"South Africa", "RSA"},
                {"Spain", "ESP"},
                {"Sri Lanka", "SRI"},
                {"Sudan", "SUD"},
                {"Suriname", "SUR"},
                {"Swaziland", "SWZ"},
                {"Sweden", "SWE"},
                {"Switzerland", "SUI"},
                {"Syria", "SYR"},
                {"Taiwan", "TWN"},
                {"Tajikistan", "TJK"},
                {"Tanzania", "TAN"},
                {"Thailand", "THA"},
                {"Togo", "TOG"},
                {"Tonga", "TGA"},
                {"Trinidad and Tobago", "TRI"},
                {"Tunisia", "TUN"},
                {"Turkey", "TUR"},
                {"Turkmenistan", "TKM"},
                {"Tuvalu", "TUV"},
                {"Uganda", "UGA"},
                {"Ukraine", "UKR"},
                {"United Arab Emirates", "UAE"},
                {"United Kingdom", "GBR"},
                {"United States of America", "USA"},
                {"Uruguay", "URU"},
                {"Uzbekistan", "UZB"},
                {"Vanuatu", "VAN"},
                {"Venezuela", "VEN"},
                {"Vietnam", "VIE"},
                {"Yemen", "YEM"},
                {"Zambia", "ZAM"},
                {"Zimbabwe", "ZIM"}
            };

            if (countryMappings.ContainsKey(countryName))
                return countryMappings[countryName];

            return "---";
        }
    }
}
