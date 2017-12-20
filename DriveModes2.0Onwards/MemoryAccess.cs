using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace LeMemoryAccess
{
    /// <summary>
    ///     Credits to CamxxCore. IQ 999
    /// </summary>
    internal static class MemoryAccess
    {
        public sealed class Pattern
        {
            private readonly string _bytes, _mask;
            private IntPtr _result;

            private static NativeMethods.MODULEINFO _moduleHandle;
            private static long _moduleAddr;

            static Pattern()
            {
                GetModuleInfo(null);
            }

            public Pattern(string bytes, string mask)
            {
                _bytes = bytes;
                _mask = mask;
            }

            public void Init()
            {
                _result = FindPattern(null);
            }

            private static unsafe void GetModuleInfo(string moduleName)
            {
                NativeMethods.GetModuleInformation(
                    NativeMethods.GetCurrentProcess(),
                    NativeMethods.GetModuleHandle(moduleName),
                    out _moduleHandle,
                    sizeof(NativeMethods.MODULEINFO));

                _moduleAddr = _moduleHandle.lpBaseOfDll.ToInt64();
            }

            private unsafe IntPtr FindPattern(string moduleName)
            {
                var end = _moduleAddr + _moduleHandle.SizeOfImage;

                var b = _bytes.ToCharArray();
                var m = _mask.ToCharArray();

                for (; _moduleAddr < end; _moduleAddr++)
                    if (BCompare((byte*)_moduleAddr, b, m))
                        return new IntPtr(_moduleAddr);

                return IntPtr.Zero;
            }

            public IntPtr Get(int offset = 0)
            {
                return _result + offset;
            }

            private static unsafe bool BCompare(byte* pData, IEnumerable<char> bMask, IReadOnlyList<char> szMask)
            {
                return !bMask.Where((t, i) => szMask[i] == 'x' && pData[i] != t).Any();
            }
        }

        public static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentProcess();

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("psapi.dll", SetLastError = true)]
            public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo,
                int cb);

            [StructLayout(LayoutKind.Sequential)]
            // ReSharper disable once InconsistentNaming
            public struct MODULEINFO
            {
                public IntPtr lpBaseOfDll;
                public uint SizeOfImage;
                public IntPtr EntryPoint;
            }
        }
    }
}