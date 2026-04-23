// Decompiled with JetBrains decompiler
// Type: StoreProcInstall.FileUtil
// Assembly: StoreProcInstall, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8462C1C8-389B-466C-9DBD-096EED56ADA2
// Assembly location: C:\Users\BenWighton\Downloads\StoreProcInstall (1).exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

#nullable disable
namespace StoreProcInstall
{
  public static class FileUtil
  {
    private const int RmRebootReasonNone = 0;
    private const int CCH_RM_MAX_APP_NAME = 255;
    private const int CCH_RM_MAX_SVC_NAME = 63;

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(
      uint pSessionHandle,
      uint nFiles,
      string[] rgsFilenames,
      uint nApplications,
      [In] FileUtil.RM_UNIQUE_PROCESS[] rgApplications,
      uint nServices,
      string[] rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
    private static extern int RmStartSession(
      out uint pSessionHandle,
      int dwSessionFlags,
      string strSessionKey);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint pSessionHandle);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmGetList(
      uint dwSessionHandle,
      out uint pnProcInfoNeeded,
      ref uint pnProcInfo,
      [In, Out] FileUtil.RM_PROCESS_INFO[] rgAffectedApps,
      ref uint lpdwRebootReasons);

    public static List<Process> WhoIsLocking(string path)
    {
      string strSessionKey = Guid.NewGuid().ToString();
      List<Process> processList = new List<Process>();
      uint pSessionHandle;
      if (FileUtil.RmStartSession(out pSessionHandle, 0, strSessionKey) != 0)
        throw new Exception("Could not begin restart session.  Unable to determine file locker.");
      try
      {
        uint pnProcInfoNeeded = 0;
        uint pnProcInfo = 0;
        uint lpdwRebootReasons = 0;
        string[] rgsFilenames = new string[1]{ path };
        if (FileUtil.RmRegisterResources(pSessionHandle, (uint) rgsFilenames.Length, rgsFilenames, 0U, (FileUtil.RM_UNIQUE_PROCESS[]) null, 0U, (string[]) null) != 0)
          throw new Exception("Could not register resource.");
        int list = FileUtil.RmGetList(pSessionHandle, out pnProcInfoNeeded, ref pnProcInfo, (FileUtil.RM_PROCESS_INFO[]) null, ref lpdwRebootReasons);
        if (list == 234)
        {
          FileUtil.RM_PROCESS_INFO[] rgAffectedApps = new FileUtil.RM_PROCESS_INFO[(int) pnProcInfoNeeded];
          pnProcInfo = pnProcInfoNeeded;
          if (FileUtil.RmGetList(pSessionHandle, out pnProcInfoNeeded, ref pnProcInfo, rgAffectedApps, ref lpdwRebootReasons) != 0)
            throw new Exception("Could not list processes locking resource.");
          processList = new List<Process>((int) pnProcInfo);
          for (int index = 0; (long) index < (long) pnProcInfo; ++index)
          {
            try
            {
              processList.Add(Process.GetProcessById(rgAffectedApps[index].Process.dwProcessId));
            }
            catch (ArgumentException ex)
            {
            }
          }
        }
        else if (list != 0)
          throw new Exception("Could not list processes locking resource. Failed to get size of result.");
      }
      finally
      {
        FileUtil.RmEndSession(pSessionHandle);
      }
      return processList;
    }

    private struct RM_UNIQUE_PROCESS
    {
      public int dwProcessId;
      public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    private enum RM_APP_TYPE
    {
      RmUnknownApp = 0,
      RmMainWindow = 1,
      RmOtherWindow = 2,
      RmService = 3,
      RmExplorer = 4,
      RmConsole = 5,
      RmCritical = 1000, // 0x000003E8
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RM_PROCESS_INFO
    {
      public FileUtil.RM_UNIQUE_PROCESS Process;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public string strAppName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
      public string strServiceShortName;
      public FileUtil.RM_APP_TYPE ApplicationType;
      public uint AppStatus;
      public uint TSSessionId;
      [MarshalAs(UnmanagedType.Bool)]
      public bool bRestartable;
    }
  }
}
