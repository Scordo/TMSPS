using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities
{
    public class PlayerEntity
    {
        private DateTime _created;
        private DateTime _lastTimePlayedChanged;

        public virtual int? Id { get; private set; }
        public virtual string Login { get; set; }
        public virtual string Nickname { get; set; }
        public virtual int Wins { get; set; }
        public virtual long TimePlayed { get; set; }
        public virtual DateTime? LastChanged { get; set; }

        public virtual DateTime Created
        {
            get { return _created; }
            private set { _created = value; }
        }

        public virtual DateTime LastTimePlayedChanged
        {
            get { return _lastTimePlayedChanged; }
            set { _lastTimePlayedChanged = value; }
        }

        public PlayerEntity()
        {
            _created = DateTime.Now;
            _lastTimePlayedChanged = _created;
        }

        public virtual PlayerEntity Clone()
        {
            return (PlayerEntity)MemberwiseClone();
        }
    }
}
