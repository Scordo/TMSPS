using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMSPS.TRC.BL.Exceptions;

namespace TMSPS.TRC.BL.Configuration
{
    public class ProfileInfo
    {
        #region Properties

        public string Name { get; protected set; }
        public string PasswordHash { get; protected set; }
        public string FilePath { get; protected set; }

        #endregion

        #region Constructors

        protected ProfileInfo()
        {
            
        }

        public ProfileInfo(string name, string password, string filePath)
        {
            Name = name;
            PasswordHash = password.ToHash();
            FilePath = filePath;
        }

        #endregion

        #region Public Methods

        public virtual void Save()
        {
            Save(null);
        }

        protected void Save(Action<BinaryWriter> additionalActions)
        {
            string directoryPath = Path.GetDirectoryName(FilePath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            using (FileStream fileStream = File.Create(FilePath))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    writer.Write(Name);
                    writer.Write(PasswordHash);

                    if (additionalActions != null)
                        additionalActions(writer);
                }
            }
        }

        public static ProfileInfo ReadFromFile(string filePath)
        {
            if (filePath.IsNullOrTimmedEmpty())
                throw new ArgumentException("filePath is null or empty.");

            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader reader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    ProfileInfo result = new ProfileInfo { FilePath = filePath };
                    result.ReadFromReader(reader);

                    return result;
                }
            }
        }

        public static bool CheckPassword(string filePath, string password)
        {
            if (password.IsNullOrTimmedEmpty())
                return false;

            return ReadFromFile(filePath).PasswordHash == password.ToHash();
        }

        public bool CheckPassword(string password)
        {
            return PasswordHash == password.ToHash();
        }

        protected void ReadFromReader(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            string profileName = reader.ReadString();

            if (profileName.IsNullOrTimmedEmpty())
                throw new InvalidFileFormatException("Could not find profilename!");

            string passwordHash = reader.ReadString();
            
            if (passwordHash.IsNullOrTimmedEmpty())
                throw new InvalidFileFormatException("Could not find passwordhash");

            Name = profileName;
            PasswordHash = passwordHash;
        }

        public static List<ProfileInfo> GetProfileInfoList(string directoryPath, string fileExtension)
        {
            if (directoryPath.IsNullOrTimmedEmpty())
                throw new ArgumentException("directoryPath is null or empty");

            if (fileExtension.IsNullOrTimmedEmpty())
                throw new ArgumentException("fileExtension is null or empty");

            if (!Directory.Exists(directoryPath))
                return new List<ProfileInfo>();

            string[] filePaths = Directory.GetFiles(directoryPath, "*." + fileExtension);

            if (filePaths.Length == 0)
                return new List<ProfileInfo>();

            List<ProfileInfo> result = new List<ProfileInfo>();

            foreach (string filePath in filePaths)
            {
                try
                {
                    result.Add(ReadFromFile(filePath));
                }
                catch
                {
                }
            }

            return result;
        }

        public override string ToString()
        {
            return Name;
        }

        public ProfileInfo CloneProfileInfo()
        {
            return new ProfileInfo{ Name = Name, PasswordHash = PasswordHash, FilePath = FilePath};
        }

        #endregion
    }
}