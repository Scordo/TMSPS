﻿using System;
using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Configuration
{
	public class Credentials
	{
	    #region Members

	    private Dictionary<string, HashSet<string>> _credentials;
	    private readonly string _filePath;

	    #endregion

	    #region Constructors

	    public Credentials(IEnumerable<KeyValuePair<string, HashSet<string>>> credentials)
	    {
	        _credentials = new Dictionary<string, HashSet<string>>();

	        foreach (KeyValuePair<string, HashSet<string>> credential in credentials)
	        {
	            _credentials[credential.Key] = new HashSet<string>(credential.Value);
	        }
	    }

	    public Credentials(string filePath)
	    {
	        _filePath = filePath;
	        ReReadFromFile();
	    }

	    #endregion

	    #region Public Methods

        public void ReReadFromFile()
        {
            _credentials = CredentialsReader.ReadCredentialsFromConfigFile(_filePath);
        }

	    public bool UserHasRight(string user, string right)
	    {
	        return UserHasAnyRight(user, right);
	    }

	    public bool UserHasAnyRight(string user, params string[] rights)
	    {
	        if (user == null)
	            throw new ArgumentNullException("user");

	        if (rights == null)
	            return true;

	        user = user.ToLower();

	        if (!_credentials.ContainsKey(user))
	            return false;

	        foreach (string right in rights)
	        {
	            if (right == null)
	                continue;

	            if (_credentials[user].Contains(right.ToLower()))
	                return true;
	        }

	        return false;
	    }

	    public HashSet<string> GetAllUserRights(string user)
	    {
	        if (user == null)
	            throw new ArgumentNullException("user");

	        user = user.ToLower();

	        return (!_credentials.ContainsKey(user)) ? new HashSet<string>() : new HashSet<string>(_credentials[user]);
	    }

	    #endregion
	}
}