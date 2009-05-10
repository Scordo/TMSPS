using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace TMSPS.Core.Common
{
    public class FlatFileNicknameResolver : NicknameResolverBase
    {
        #region Non Public Members

        private readonly object _writeLock = new object();

        #endregion

        #region Properties

        private string FilePath { get; set; }

        #endregion

        #region Public Methods

        public override string Get(string login)
        {
            if (NicknameCache.ContainsKey(login))
                return NicknameCache[login].Nickname;

            long nickStartPos;
            return GetNickname(login, out nickStartPos);
        }

        public override void Set(string login, string nickname)
        {
            long nickStartPos;
            string storedNickname = GetNickname(login, out nickStartPos);

            if (storedNickname != nickname)
            {
                if (storedNickname == null)
                {
                    lock (_writeLock)
                    {
                        AppendNickname(login, nickname);
                        UpdateCacheForLogin(login, nickname);
                    }
                }
                else
                {
                    lock (_writeLock)
                    {
                        UpdateNickname(nickStartPos, nickname);
                    }
                }
            }

            UpdateCacheForLogin(login, nickname);
        }

        public override void ReadConfigSettings(XElement configElement)
        {
            string baseDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            FilePath = Path.Combine(baseDirectory, "logins.cfg");
            
            if (configElement == null)
                return;

            XAttribute filePathAttribute = configElement.Attribute("filePath");
            if (filePathAttribute == null || filePathAttribute.Value.IsNullOrTimmedEmpty())
                return;

            FilePath = Path.IsPathRooted(filePathAttribute.Value) ? filePathAttribute.Value : Path.Combine(baseDirectory, filePathAttribute.Value);
        }

        #endregion

        #region Non Public Methods

        private string GetNickname(string loginToFind, out long nickStartPos)
        {
            const int loginWidth = 100; // normal width is 50 --> unicode * 2
            const int nickWidth = 100; // normal width is 50 --> unicode * 2
            nickStartPos = 2; // utf 16 preamble offset diff
            Encoding utf16Encoding = Encoding.GetEncoding("utf-16");

            if (File.Exists(FilePath))
            {
                using (FileStream stream = File.OpenRead(FilePath))
                {
                    // read the 2 utf-16 marker bytes
                    stream.ReadByte();
                    stream.ReadByte();

                    byte[] buffer = new byte[loginWidth + nickWidth + 4];

                    while (stream.Read(buffer, 0, buffer.Length) > 0)
                    {
                        string login = utf16Encoding.GetString(buffer, 0, loginWidth);

                        if (login.StartsWith(loginToFind, StringComparison.InvariantCultureIgnoreCase) && string.Compare(login.TrimEnd(), loginToFind, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            nickStartPos += loginWidth;
                            return utf16Encoding.GetString(buffer, loginWidth, nickWidth).TrimEnd();
                        }

                        nickStartPos = stream.Position;
                    }
                }
            }

            nickStartPos = -1;
            return null;
        }

        private void UpdateNickname(long nickPos, string newNickName)
        {
            using (FileStream stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                stream.Seek(nickPos, SeekOrigin.Begin);

                string newNick = newNickName.PadRight(50);
                byte[] newNickBytes = Encoding.GetEncoding("utf-16").GetBytes(newNick);

                stream.Write(newNickBytes, 0, newNickBytes.Length);
            }
        }

        private void AppendNickname(string login, string newNickName)
        {
            using (FileStream stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                if (stream.Length == 0)
                {
                    // write the 2 utf-16 marker bytes
                    stream.WriteByte(255);
                    stream.WriteByte(254);
                }
                else
                {
                    stream.Seek(0, SeekOrigin.End);
                }

                string line = string.Concat(login.PadRight(50), newNickName.PadRight(50), Environment.NewLine);
                byte[] lineBytes = Encoding.GetEncoding("utf-16").GetBytes(line);

                stream.Write(lineBytes, 0, lineBytes.Length);
            }
        }

        #endregion
    }
}