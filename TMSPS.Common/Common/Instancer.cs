using System;
using System.Reflection;

namespace TMSPS.Core.Common
{
    public class Instancer
    {
        #region Public Methods

        public static T GetInstanceOfInterface<T>(string assemblyFullName, string tyepNameToInstantiate)
        {
			if (assemblyFullName == null)
				throw new ArgumentNullException("assemblyFullName");

			if (!assemblyFullName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				assemblyFullName += ".dll";

            if (!typeof(T).IsInterface)
                throw new ArgumentException("Generic type parameter <T> is not an interface.");

            Assembly assembly;

            try
            {
				assembly = Assembly.LoadFile(assemblyFullName);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Could not load Assembly " + assemblyFullName, ex);
            }

            object providerInstance;

            try
            {
                providerInstance = assembly.CreateInstance(tyepNameToInstantiate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not create instance of " + tyepNameToInstantiate, ex);
            }

            if (providerInstance == null)
				throw new InvalidOperationException("Could not create instance of " + tyepNameToInstantiate);

            if (!(providerInstance is T))
                throw new ArgumentException(string.Format("Class '{0}' does not implement {1}.", providerInstance.GetType().FullName, typeof(T).FullName));

            return (T)providerInstance;
        }

        #endregion
    }
}