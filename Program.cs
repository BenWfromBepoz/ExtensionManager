// Decompiled with JetBrains decompiler
// Type: StoreProcInstall.Program
// Assembly: StoreProcInstall, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8462C1C8-389B-466C-9DBD-096EED56ADA2
// Assembly location: C:\Users\BenWighton\Downloads\StoreProcInstall (1).exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable disable
namespace StoreProcInstall
{
  internal static class Program
  {
    [STAThread]
    private static async Task Main(string[] args)
    {
      if (await Program.SelfUpdate())
      {
        Application.Restart();
      }
      else
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run((Form) new frmMain(((IEnumerable<string>) args).Any<string>((Func<string, bool>) (a => a.Contains("auto"))), ((IEnumerable<string>) args).Any<string>((Func<string, bool>) (a => a.Contains("allowdowngrade"))), ((IEnumerable<string>) args).Any<string>((Func<string, bool>) (a => a.Contains("test")))));
      }
    }

    private static async Task<bool> SelfUpdate()
    {
      string filename = Process.GetCurrentProcess().MainModule?.FileName;
      Version ver = new Version(Process.GetCurrentProcess().MainModule?.FileVersionInfo?.FileVersion ?? "0.0.0.0");
      if (string.IsNullOrEmpty(filename))
        return false;
      if (File.Exists(filename + ".tmp"))
        File.Delete(filename + ".tmp");
      Version newVer = await Program.GetInstallerVersionAsync();
      if (newVer <= ver)
        return false;
      byte[] newVerData = await Program.GetLatestVersionFileAsync();
      if (newVerData == null || newVerData.Length == 0)
        return false;
      Assembly newAssem = Assembly.Load(newVerData);
      if (newAssem.GetName().Version == ver)
        return false;
      try
      {
        File.Move(filename, filename + ".tmp");
        if (!File.Exists(filename))
          File.WriteAllBytes(filename, newVerData);
      }
      catch
      {
      }
      try
      {
        if (File.Exists(filename + ".tmp"))
          File.Delete(filename + ".tmp");
      }
      catch
      {
      }
      return true;
    }

    public static async Task<Version> GetInstallerVersionAsync()
    {
      Version versionAsync = await frmMain.GetVersionAsync("InstallerVersion.txt");
      return versionAsync;
    }

    public static async Task<byte[]> GetLatestVersionFileAsync()
    {
      byte[] dataAsync = await frmMain.GetDataAsync(frmMain.ServerPath + "StoreProcInstall.exe");
      return dataAsync;
    }
  }
}
