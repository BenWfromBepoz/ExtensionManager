// Decompiled with JetBrains decompiler
// Type: StoreProcInstall.Telemetry
// Assembly: StoreProcInstall, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8462C1C8-389B-466C-9DBD-096EED56ADA2
// Assembly location: C:\Users\BenWighton\Downloads\StoreProcInstall (1).exe

using Microsoft.Win32;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;

#nullable disable
namespace StoreProcInstall
{
  public class Telemetry
  {
    private static string _path = "http://localhost:5094/";
    private static WebClient _client = new WebClient();
    private static int _systemID;

    public static async Task SendTelemetry(string path, BaseLog log)
    {
      try
      {
        NameValueCollection data = new NameValueCollection();
        data.Add("DateTime", DateTime.Now.ToString("O"));
        data.Add("SystemID", Telemetry.SystemID.ToString());
        data.Add("LogType", log.Type.ToString());
        data.Add("Filename", log.Filename);
        data.Add("Message", log.Message);
        byte[] result = await Telemetry._client.UploadValuesTaskAsync(Telemetry._path + path, data);
        data = (NameValueCollection) null;
        result = (byte[]) null;
      }
      catch
      {
      }
    }

    public static async Task InstallLog(LogType type, string message = null, string filename = null)
    {
      BaseLog item = new BaseLog(type, message, filename);
      await Telemetry.SendTelemetry("install", item);
      item = (BaseLog) null;
    }

    public static async Task UpdateLog(LogType type, string message = null, string filename = null)
    {
      BaseLog item = new BaseLog(type, message, filename);
      await Telemetry.SendTelemetry("update", item);
      item = (BaseLog) null;
    }

    public static int SystemID
    {
      get
      {
        if (Telemetry._systemID > 0)
          return Telemetry._systemID;
        using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Backoffice"))
        {
          object obj = registryKey?.GetValue(nameof (SystemID)) ?? (object) 0;
          return Telemetry._systemID = (int) obj;
        }
      }
    }
  }
}
