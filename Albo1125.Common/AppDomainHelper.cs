// Updated AppDomainHelper - Removes COM usage, uses internal tracking
// Only works in .NET Framework, not .NET Core or .NET 5–8.
// Updated by pherem on 5/3/2025 This is replacement for the original AppDomainHelper.cs file. using mscoree
// updated for newer framework 4.8

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Albo1125.Common
{
    /// <summary>
    /// Provides utilities for interacting with and invoking actions across AppDomains.
    /// </summary>
    [Serializable]
    public static class AppDomainHelper
    {
        public delegate void CrossAppDomainCallDelegate(params object[] payload);
        public delegate object CrossAppDomainCallRetValueDelegate(params object[] payload);

        // Internally tracked domains created via this helper
        private static readonly List<AppDomain> CreatedDomains = new List<AppDomain>();

        /// <summary>
        /// Creates and tracks a new AppDomain with the given friendly name.
        /// </summary>
        public static AppDomain CreateAppDomain(string name)
        {
            var domain = AppDomain.CreateDomain(name);
            CreatedDomains.Add(domain);
            return domain;
        }

        /// <summary>
        /// Gets a tracked AppDomain by its friendly name.
        /// </summary>
        public static AppDomain GetAppDomainByName(string name) =>
            CreatedDomains.FirstOrDefault(domain => domain.FriendlyName == name);

        /// <summary>
        /// Lists all AppDomains created via this helper.
        /// </summary>
        public static IList<AppDomain> GetAppDomains() =>
            CreatedDomains.ToList();

        /// <summary>
        /// Invokes a void delegate across AppDomains.
        /// </summary>
        public static void InvokeOnAppDomain(AppDomain appDomain, CrossAppDomainCallDelegate targetFunc, params object[] payload)
        {
            if (appDomain == null || targetFunc == null) return;

            appDomain.SetData($"{appDomain.FriendlyName}_payload", payload);
            appDomain.SetData($"{appDomain.FriendlyName}_func", targetFunc);
            appDomain.DoCallBack(InvokeOnAppDomainInternal);
        }

        /// <summary>
        /// Invokes a delegate across AppDomains and returns a result.
        /// </summary>
        public static T InvokeOnAppDomain<T>(AppDomain appDomain, CrossAppDomainCallRetValueDelegate targetFunc, params object[] payload)
        {
            if (appDomain == null || targetFunc == null) return default;

            appDomain.SetData($"{appDomain.FriendlyName}_payload", payload);
            appDomain.SetData($"{appDomain.FriendlyName}_func", targetFunc);
            appDomain.DoCallBack(InvokeOnAppDomainRetInternal);

            object result = appDomain.GetData("result");
            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Runs a static method in a new AppDomain and optionally unloads it afterward.
        /// </summary>
        /// <param name="domainName">Friendly name for the AppDomain.</param>
        /// <param name="assemblyPath">Path to the .dll or .exe file.</param>
        /// <param name="typeName">Fully qualified type name (e.g., "MyNamespace.MyClass").</param>
        /// <param name="methodName">Name of the static method to call.</param>
        /// <param name="args">Optional method arguments (must match the method signature).</param>
        /// <param name="unloadAfter">If true, the domain is unloaded after execution.</param>
        public static void RunStaticMethodInNewAppDomain(string domainName, string assemblyPath, string typeName, string methodName, object[] args = null, bool unloadAfter = true)
        {
            AppDomain domain = CreateAppDomain(domainName);

            try
            {
                domain.DoCallBack(() =>
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    Type type = assembly.GetType(typeName);
                    if (type == null)
                        throw new TypeLoadException($"Type '{typeName}' not found in assembly '{assembly.FullName}'.");

                    MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null)
                        throw new MissingMethodException($"Method '{methodName}' not found in type '{type.FullName}'.");

                    method.Invoke(null, args);
                });
            }
            finally
            {
                if (unloadAfter)
                {
                    AppDomain.Unload(domain);
                    CreatedDomains.Remove(domain);
                }
            }
        }

        #region Internal Callbacks

        private static void InvokeOnAppDomainInternal()
        {
            var domain = AppDomain.CurrentDomain;
            string name = domain.FriendlyName;

            var payload = domain.GetData($"{name}_payload") as object[];
            var func = domain.GetData($"{name}_func") as CrossAppDomainCallDelegate;

            func?.Invoke(payload);
        }

        private static void InvokeOnAppDomainRetInternal()
        {
            var domain = AppDomain.CurrentDomain;
            string name = domain.FriendlyName;

            var payload = domain.GetData($"{name}_payload") as object[];
            var func = domain.GetData($"{name}_func") as CrossAppDomainCallRetValueDelegate;

            if (func != null)
            {
                object result = func.Invoke(payload);
                domain.SetData("result", result);
            }
        }

        #endregion
    }
}


