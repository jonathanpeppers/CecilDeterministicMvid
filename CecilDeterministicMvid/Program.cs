using System;
using System.IO;
using Mono.Cecil;

namespace CecilDeterministicMvid
{
	public class Program
	{
		static void Main ()
		{
			var dir = Path.GetDirectoryName (typeof (Program).Assembly.Location);
			var path = Path.Combine (dir, "Mono.Android.dll");
			var writerParameters = new WriterParameters {
				DeterministicMvid = true,
			};
			var readerParameters = new ReaderParameters { ReadWrite = true };
			using (var assembly = AssemblyDefinition.ReadAssembly (path, readerParameters)) {
				Console.WriteLine ($"Existing mvid: {assembly.MainModule.Mvid}");
				foreach (TypeDefinition type in assembly.MainModule.Types)
					ProcessType (type);
				assembly.Write (writerParameters);
			}
			// Open it again?
			using (var assembly = AssemblyDefinition.ReadAssembly (path)) {
				Console.WriteLine ($"Final mvid: {assembly.MainModule.Mvid}");
			}
		}

		static void ProcessType (TypeDefinition type)
		{
			if (type.HasFields)
				foreach (FieldDefinition field in type.Fields)
					ProcessAttributeProvider (field);

			if (type.HasMethods)
				foreach (MethodDefinition method in type.Methods)
					ProcessAttributeProvider (method);
		}

		static void ProcessAttributeProvider (Mono.Cecil.ICustomAttributeProvider provider)
		{
			if (!provider.HasCustomAttributes)
				return;

			for (int i = 0; i < provider.CustomAttributes.Count; i++) {
				if (!IsRegisterAttribute (provider.CustomAttributes [i]))
					continue;

				provider.CustomAttributes.RemoveAt (i--);
			}
		}

		static bool IsRegisterAttribute (CustomAttribute attribute)
		{
			return attribute.Constructor.DeclaringType.FullName == "Android.Runtime.RegisterAttribute";
		}
	}
}
