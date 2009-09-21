using System;
using System.IO;
using System.Net;
using TMSPS.Core.Logging;

namespace TMSPS.Core.PluginSystem.Plugins.TMX
{
    public class TMXInfo
    {
        #region Constants

        private const string UNITED_INFO_URL_PREFIX = "http://united.tm-exchange.com/apiget.aspx?action=apitrackinfo&id=";
        private const string FOREVER_INFO_URL_PREFIX = "http://tmnforever.tm-exchange.com/apiget.aspx?action=apitrackinfo&id=";

        private const string UNITED_DOWNLOAD_URL_PREFIX = "http://united.tm-exchange.com/get.aspx?action=trackgbx&id=";
        private const string FOREVER_DOWNLOAD_URL_PREFIX = "http://tmnforever.tm-exchange.com/get.aspx?action=trackgbx&id=";

        #endregion


        #region Properties

        public bool Erroneous { get { return ErrorMessage != null; } }
        public string ErrorMessage { get; private set; }
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string UserID { get; private set; }
        public string Author { get; private set; }
        public string Uploaded { get; private set; }
        public string Updated { get; private set; }
        public bool Visible { get; private set; }
        public string Type { get; private set; }
        public string Environment { get; private set; }
        public string Mood { get; private set; }
        public string Style { get; private set; }
        public string Routes { get; private set; }
        public string Length { get; private set; }
        public string DifficultLevel { get; private set; }
        public string LBRating { get; private set; }
        public string Game { get; private set; }
        public string Description { get; private set; }

        #endregion

        #region Constructor

        private TMXInfo()
        {
            
        }

        #endregion

        #region Public Methods

        public static TMXInfo Parse(string input)
        {
            if (input.IsNullOrTimmedEmpty())
                return null;

            string[] parts = input.Split(new [] {'\t'}, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 16)
                return new TMXInfo {ErrorMessage = input};

            TMXInfo result = new TMXInfo
            {
                    ID = parts[0],
                    Name = parts[1],
                    UserID = parts[2],
                    Author = parts[3],
                    Uploaded = parts[4],
                    Updated = parts[5],
                    Visible = parts[6].Equals("true", StringComparison.InvariantCultureIgnoreCase),
                    Type = parts[7],
                    Environment = parts[8],
                    Mood = parts[9],
                    Style = parts[10],
                    Routes = parts[11],
                    Length = parts[12],
                    DifficultLevel = parts[13],
                    LBRating = parts[14],
                    Game = parts[15]
            };

            if (parts.Length > 16)
                result.Description = parts[16];

            return result;
        }

        public static TMXInfo Retrieve(string id)
        {
            using (WebClient webClient = new WebClient())
            {
                string response = null;

                try
                {
                    response = webClient.DownloadString(GetInfoUrl(id));
                }
                catch (Exception ex)
                {
                    CoreLogger.UniqueInstance.Info("Error downloading TMXInfo for id " + id, ex);
                }

                return Parse(response);
            }
        }

        public static byte[] DownloadTrack(string id)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] data = null;

                try
                {
                    data = webClient.DownloadData(GetDownloadUrl(id));
                }
                catch (Exception ex)
                {
                    CoreLogger.UniqueInstance.Info("Error downloading TMXInfo for id " + id, ex);
                }

                return data;
            }
        }

        public string GetTMXFilePath(string tracksDirectory)
        {
            return GetTMXFilePath(tracksDirectory, ID);
        }

        public static string GetTMXFilePath(string tracksDirectory, string trackID)
        {
            if (tracksDirectory == null)
                throw new ArgumentNullException("tracksDirectory");

            return Path.Combine(tracksDirectory, GetRelativeFilePath(trackID));
        }

        public string GetRelativeFilePath()
        {
            return @"Challenges\TMX\" + GetTMXFilename();
        }

        public static string GetRelativeFilePath(string trackID)
        {
            return @"Challenges\TMX\" + GetTMXFilename(trackID);
        }

        public string GetTMXFilename()
        {
            return GetTMXFilename(ID);
        }

        public static string GetTMXFilename(string trackID)
        {
            if (trackID == null)
                throw new ArgumentNullException("trackID");

            return string.Format("TMX_{0}.Challenge.Gbx", trackID.Trim());
        }

        #endregion

        #region Non Public Methods

        private static string GetDownloadUrl(string trackID)
        {
            bool isUnited;
            trackID = NormalizeTrackID(trackID, out isUnited);

            return string.Concat(isUnited ? UNITED_DOWNLOAD_URL_PREFIX : FOREVER_DOWNLOAD_URL_PREFIX, trackID);
        }

        private static string GetInfoUrl(string trackID)
        {
            bool isUnited;
            trackID = NormalizeTrackID(trackID, out isUnited);

            return string.Concat(isUnited ? UNITED_INFO_URL_PREFIX : FOREVER_INFO_URL_PREFIX, trackID);
        }

        private static string NormalizeTrackID(string trackID, out bool isUnited)
        {
            if (trackID == null)
                throw new ArgumentNullException(trackID);

            trackID = trackID.Trim();
            isUnited = trackID.Trim().EndsWith("u", StringComparison.InvariantCultureIgnoreCase);

            if (isUnited)
                trackID = trackID.Remove(trackID.Length - 1);

            return trackID;
        }

        #endregion

    }
}