using System;
using System.Collections.Generic;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem
{
    public class ValueStore
    {
        #region Non Public Members

        private readonly Dictionary<string, object> _keysAndValues = new Dictionary<string, object>();

        #endregion

        #region Events

        public event EventHandler<EventArgs<KeyValuePair<string, object>>> ValueRemoved;
        public event EventHandler<EventArgs<KeyValuePair<string, object>>> ValueInserted;
        public event EventHandler<ValueUpdatedEventArgs> ValueUpdated;
        public event EventHandler Cleared;

        #endregion

        #region Public Methods

        public bool ContainsKey(string key)
        {
            if (key.IsNullOrTimmedEmpty())
                throw new ArgumentException("key is null or empty");

            return _keysAndValues.ContainsKey(key.Trim());
        }

        public void Clear()
        {
            _keysAndValues.Clear();

            if (Cleared != null)
                Cleared(this, EventArgs.Empty);
        }

        public void Remove(string key)
        {
            if (key.IsNullOrTimmedEmpty())
                throw new ArgumentException("key is null or empty");

            key = key.Trim();

            if (ContainsKey(key))
            {
                object value = _keysAndValues[key];
                _keysAndValues.Remove(key);

                if (ValueRemoved != null)
                    ValueRemoved(this, new EventArgs<KeyValuePair<string, object>>(new KeyValuePair<string, object>(key, value)));
            }
        }

        public void SetOrUpdate(string key, object value)
        {
            if (key.IsNullOrTimmedEmpty())
                throw new ArgumentException("key is null or empty");

            key = key.Trim();

            if (ContainsKey(key))
            {
                object oldValue = _keysAndValues[key];
                _keysAndValues[key] = value;

                if (ValueUpdated != null)
                    ValueUpdated(this, new ValueUpdatedEventArgs(key, oldValue, value));
            }
            else
            {
                _keysAndValues[key] = value;

                if (ValueInserted != null)
                    ValueInserted(this, new EventArgs<KeyValuePair<string, object>>(new KeyValuePair<string, object>(key, value)));
            }

            
        }

        public T GetValue<T>(string key)
        {
            return GetValueOrDefault(key, default(T), true);
        }

        public T GetValueOrDefault<T>(string key, T defaultValue)
        {
            return GetValueOrDefault(key, defaultValue, false);
        }

        #endregion

        #region Non Public Methods

        private T GetValueOrDefault<T>(string key, T defaultValue, bool exceptionOnKeyMissing)
        {
            if (!_keysAndValues.ContainsKey(key))
            {
                if (exceptionOnKeyMissing)
                    throw new KeyNotFoundException("Could not find key " + key);

                return defaultValue;
            }

            return (T) _keysAndValues[key];
        }

        #endregion

        public class ValueUpdatedEventArgs : EventArgs
        {
            #region Properties

            public object OldValue { get; private set; }
            public object NewValue { get; private set; }
            public string Key { get; private set; }

            #endregion

            #region Constructor

            public ValueUpdatedEventArgs(string key, object oldValue, object newValue)
            {
                OldValue = oldValue;
                NewValue = newValue;
                Key = key;
            }

            #endregion
        }
    }
}
