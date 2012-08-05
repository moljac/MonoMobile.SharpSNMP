using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lextm.SharpSnmpLib.Mib
{
	/// <summary>
	/// Default object registry.
	/// </summary>
	public sealed partial class DefaultObjectRegistry : ObjectRegistryBase
	{
		private static IList<ModuleLoader> LoadDefaultModules()
		{
			IList<ModuleLoader> result = 
				new List<ModuleLoader>(5)
						{
							// mc++
							// Resources
							LoadSingle(Encoding.ASCII.GetString(Resources.SNMPV2_SMI), "SNMPV2-SMI"),
							LoadSingle(Encoding.ASCII.GetString(Resources.SNMPV2_CONF), "SNMPV2-CONF"),
							LoadSingle(Encoding.ASCII.GetString(Resources.SNMPV2_TC), "SNMPV2-TC"),
							LoadSingle(Encoding.ASCII.GetString(Resources.SNMPV2_MIB), "SNMPV2-MIB"),
							LoadSingle(Encoding.ASCII.GetString(Resources.SNMPV2_TM), "SNMPV2-TM")
						};
			return result;
		}
	}
}