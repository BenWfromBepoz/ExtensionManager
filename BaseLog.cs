// Decompiled with JetBrains decompiler
// Type: StoreProcInstall.BaseLog
// Assembly: StoreProcInstall, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8462C1C8-389B-466C-9DBD-096EED56ADA2
// Assembly location: C:\Users\BenWighton\Downloads\StoreProcInstall (1).exe

#nullable disable
namespace StoreProcInstall
{
  public class BaseLog
  {
    public BaseLog(LogType type, string message, string filename)
    {
      this.Type = type;
      this.SystemID = this.SystemID;
      this.Message = message;
      this.Filename = filename;
    }

    public int SystemID { get; set; }

    public LogType Type { get; set; }

    public string Filename { get; set; }

    public string Message { get; set; }
  }
}
