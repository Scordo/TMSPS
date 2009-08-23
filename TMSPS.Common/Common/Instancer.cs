using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TMSPS.Core.Common
{
    public class Instancer
    {
        #region Public Methods

        public static T GetInstanceOfInterface<T>(string assemblyName, string tyepNameToInstantiate, params object[] arguments)
        {
			if (assemblyName == null)
				throw new ArgumentNullException("assemblyName");

			if (!assemblyName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				assemblyName += ".dll";

            if (!typeof(T).IsInterface)
                throw new ArgumentException("Generic type parameter <T> is not an interface.");

			if (!Path.IsPathRooted(assemblyName))
			{
				string currentAppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				assemblyName = Path.Combine(currentAppPath, assemblyName);
			}

            Assembly assembly;

            try
            {
				assembly = Assembly.LoadFile(assemblyName);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Could not load Assembly " + assemblyName, ex);
            }

            object providerInstance;

            Type typeToInstantiate = assembly.GetType(tyepNameToInstantiate);

            if (typeToInstantiate == null)
                throw new InvalidOperationException(string.Format("Could not create instance of {0}. The type does not exist in the assembly {1}", tyepNameToInstantiate, assemblyName));

            try
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                providerInstance = Activator.CreateInstance(typeToInstantiate, flags, null, arguments, CultureInfo.InvariantCulture);
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