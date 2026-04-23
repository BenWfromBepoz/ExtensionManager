// Decompiled with JetBrains decompiler
// Type: StoreProcInstall.LogType
// Assembly: StoreProcInstall, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8462C1C8-389B-466C-9DBD-096EED56ADA2
// Assembly location: C:\Users\BenWighton\Downloads\StoreProcInstall (1).exe

#nullable disable
namespace StoreProcInstall
{
  public enum LogType
  {
    None,
    Error,
    Start,
    Exit,
    Restart,
    Download,
    DownloadFailed,
    CouldNotShutdownSC,
    CouldNotStartSC,
    CouldNotWriteFile,
    NewVer,
    ManualUpdate,
    ScreenConnect,
  }
}
