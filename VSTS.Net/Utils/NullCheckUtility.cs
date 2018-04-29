using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VSTS.Net.Utils
{
    [DebuggerStepThrough]
    internal static class NullCheckUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfArgumentNull<T>(T obj, string name = "")
        {
            if (obj == null)
                throw new ArgumentNullException(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNullOrEmpty(string str, string message = "")
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(message);
        }
    }
}
