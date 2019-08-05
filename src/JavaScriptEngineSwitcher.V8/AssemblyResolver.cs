﻿using System;
using System.IO;
using System.Reflection;

using Microsoft.ClearScript.V8;

using JavaScriptEngineSwitcher.V8.Constants;

namespace JavaScriptEngineSwitcher.V8
{
	/// <summary>
	/// Assembly resolver
	/// </summary>
	internal static class AssemblyResolver
	{
		/// <summary>
		/// Initialize a assembly resolver
		/// </summary>
		public static void Initialize()
		{
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveHandler;
		}

		private static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
		{
			if (args.Name == DllName.ClearScriptV8Universal)
			{
				var currentDomain = (AppDomain)sender;
				string baseDirectoryPath = currentDomain.SetupInformation.PrivateBinPath;
				if (string.IsNullOrEmpty(baseDirectoryPath))
				{
					// `PrivateBinPath` property is empty in test scenarios, so
					// need to use the `BaseDirectory` property
					baseDirectoryPath = currentDomain.BaseDirectory;
				}

				string platformName;
				int platformBitness;
				if (Environment.Is64BitProcess)
				{
					platformName = "x64";
					platformBitness = 64;
				}
				else
				{
					platformName = "x86";
					platformBitness = 32;
				}

				string assemblyName = DllName.ClearScriptV8Universal + "-" + platformBitness;
				string assemblyDirectoryPath = Path.Combine(baseDirectoryPath, platformName);
				string assemblyFileName = assemblyName + ".dll";
				string assemblyFilePath = Path.Combine(assemblyDirectoryPath, assemblyFileName);
				bool assemblyFileExists = File.Exists(assemblyFilePath);

				if (assemblyFileExists)
				{
					return Assembly.LoadFile(assemblyFilePath);
				}

				Assembly clearScriptAssembly = typeof(V8ScriptEngine).Assembly;
				string clearScriptAssemblyFullName = clearScriptAssembly.FullName;
				int commaPosition = clearScriptAssemblyFullName.IndexOf(',');

				if (commaPosition != -1)
				{
					string assemblyFullName = assemblyName +
						clearScriptAssemblyFullName.Substring(commaPosition);

					return Assembly.Load(assemblyFullName);
				}
			}

			return null;
		}
	}
}