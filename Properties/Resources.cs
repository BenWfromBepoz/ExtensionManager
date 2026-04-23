// Decompiled with JetBrains decompiler
// Type: StoreProcInstall.Properties.Resources
// Assembly: StoreProcInstall, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8462C1C8-389B-466C-9DBD-096EED56ADA2
// Assembly location: C:\Users\BenWighton\Downloads\StoreProcInstall (1).exe

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

#nullable disable
namespace StoreProcInstall.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (StoreProcInstall.Properties.Resources.resourceMan == null)
          StoreProcInstall.Properties.Resources.resourceMan = new ResourceManager("StoreProcInstall.Properties.Resources", typeof (StoreProcInstall.Properties.Resources).Assembly);
        return StoreProcInstall.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get => StoreProcInstall.Properties.Resources.resourceCulture;
      set => StoreProcInstall.Properties.Resources.resourceCulture = value;
    }
  }
}
