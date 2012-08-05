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
		private static volatile DefaultObjectRegistry _instance;
		private static readonly object Locker = new object();
		
		private DefaultObjectRegistry()
		{
			Tree = new ObjectTree(LoadDefaultModules());
		}

		/// <summary>
		/// Default instance.
		/// </summary>
		[CLSCompliant(false)]
		public static DefaultObjectRegistry Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (Locker)
					{
						if (_instance == null)
						{
							_instance = new DefaultObjectRegistry();
						}
					}
				}
				
				return _instance;
			}
		}


		private static ModuleLoader LoadSingle(string mibFileContent, string name)
		{
			ModuleLoader result;
			using (TextReader reader = new StringReader(mibFileContent))
			{
				result = new ModuleLoader(reader, name);
			}
			
			return result;
		}
	}
}