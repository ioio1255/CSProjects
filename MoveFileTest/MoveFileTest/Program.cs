using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MoveFileTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string sourcefolder = @"\\172.28.5.54\c$";
            string source = @"\\172.28.5.54\c$\5.txt";
            string dest = @"\\172.28.5.54\c$\testfolder\5.txt";
            string username = @"apjdemo\administrator";
            string pwd = "demo12!@";
            OpenNetShare(sourcefolder, username, pwd);
            File.Move(source, dest);
            CloseNetShare(sourcefolder);
        }

        public static void OpenNetShare(string path, string username, string pwd)
        {
            try
            {
                int code = NetUse(path, username, pwd);
            }
            catch (Exception ex)
            {
            }
        }

        public static void CloseNetShare(string uncPath)
        {
            try
            {
                CancelNetUse(uncPath);
            }
            catch (Exception ex)
            {
            }
        }


        /// <summary>
        /// setup net share
        /// </summary>
        /// <param name="remotepath">UNC路径。</param>
        /// <param name="username">用户名。</param>
        /// <param name="password">密码(未加密)。</param>
        /// <returns></returns>
        public static int NetUse(string remotepath, string username, string password)
        {
            CancelNetUse(remotepath);

            Win32Native.NETRESOURCEW[] n = new Win32Native.NETRESOURCEW[1];
            n[0] = new Win32Native.NETRESOURCEW();
            n[0].dwType = 1;
            n[0].lpLocalName = null;
            n[0].lpRemoteName = remotepath;
            n[0].lpProvider = null;

            int state = Win32Native.WNetAddConnection2W(n, password, username, 1);

            if (state != 0)
            {
                state = Win32Native.WNetAddConnection2W(n, password, username, 1);
            }

            return state;
        }

        /// <summary>
        /// Cancel net share.
        /// </summary>
        /// <param name="remotepath">UNC路径。</param>
        public static int CancelNetUse(string remotepath)
        {
            return Win32Native.WNetCancelConnection2(remotepath, 0, false);
        }

        /// <summary>
        /// Retrieves the local path on the given server and share name.
        /// </summary>
        /// <remarks>If remote server, should use AveImpersonator to impersonate</remarks>
        public static string GetNetShareLocalPath(string serverName, string netShareName)
        {
            string path = null;
            IntPtr ptr = IntPtr.Zero;
            int errCode = Win32Native.NetShareGetInfo(serverName, netShareName, 2, ref ptr);
            if (errCode != 0)
            {
                throw new Exception(errCode.ToString());
            }

            Win32Native.SHARE_INFO shareInfo = (Win32Native.SHARE_INFO)Marshal.PtrToStructure(ptr, typeof(Win32Native.SHARE_INFO));
            path = shareInfo.shi2_path;
            Win32Native.NetApiBufferFree(ptr);
            return path;
        }
    }

    public class Win32Native
    {
        #region --NETAPI32.DLL--


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHARE_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_netname;

            public string shi2_netname
            {
                get { return _shi2_netname; }
                set { _shi2_netname = value; }
            }
            private uint _shi2_type;

            public uint shi2_type
            {
                get { return _shi2_type; }
                set { _shi2_type = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_remark;

            public string shi2_remark
            {
                get { return _shi2_remark; }
                set { _shi2_remark = value; }
            }
            private uint _shi2_permissions;

            public uint shi2_permissions
            {
                get { return _shi2_permissions; }
                set { _shi2_permissions = value; }
            }
            private uint _shi2_max_uses;

            public uint shi2_max_uses
            {
                get { return _shi2_max_uses; }
                set { _shi2_max_uses = value; }
            }
            private uint _shi2_current_uses;

            public uint shi2_current_uses
            {
                get { return _shi2_current_uses; }
                set { _shi2_current_uses = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_path;

            public string shi2_path
            {
                get { return _shi2_path; }
                set { _shi2_path = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _shi2_passwd;

            public string shi2_passwd
            {
                get { return _shi2_passwd; }
                set { _shi2_passwd = value; }
            }
        }

        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        public static extern int NetShareGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string netname, int level, ref IntPtr bufptr);

        [DllImport("Netapi32", CharSet = CharSet.Auto)]
        internal static extern int NetApiBufferFree(IntPtr Buffer);


        [DllImport("mpr.dll")]
        public static extern int WNetAddConnection2W([MarshalAs(UnmanagedType.LPArray)] NETRESOURCEW[] lpNetResource, [MarshalAs(UnmanagedType.LPWStr)] string lpPassword, [MarshalAs(UnmanagedType.LPWStr)] string UserName, int dwFlags);

        [DllImport("mpr.dll")]
        public static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        [StructLayout(LayoutKind.Sequential)]
        public struct NETRESOURCEW
        {
            private int _dwScope;

            public int dwScope
            {
                get { return _dwScope; }
                set { _dwScope = value; }
            }
            private int _dwType;

            public int dwType
            {
                get { return _dwType; }
                set { _dwType = value; }
            }
            private int _dwDisplayType;

            public int dwDisplayType
            {
                get { return _dwDisplayType; }
                set { _dwDisplayType = value; }
            }
            private int _dwUsage;

            public int dwUsage
            {
                get { return _dwUsage; }
                set { _dwUsage = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpLocalName;

            public string lpLocalName
            {
                get { return _lpLocalName; }
                set { _lpLocalName = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpRemoteName;

            public string lpRemoteName
            {
                get { return _lpRemoteName; }
                set { _lpRemoteName = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpComment;

            public string lpComment
            {
                get { return _lpComment; }
                set { _lpComment = value; }
            }
            [MarshalAs(UnmanagedType.LPWStr)]
            private string _lpProvider;

            public string lpProvider
            {
                get { return _lpProvider; }
                set { _lpProvider = value; }
            }
        }
        #endregion
    }
}
