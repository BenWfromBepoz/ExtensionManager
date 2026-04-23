// Decompiled with JetBrains decompiler
// Type: StoreProcInstall.frmMain
// Assembly: StoreProcInstall, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8462C1C8-389B-466C-9DBD-096EED56ADA2
// Assembly location: C:\Users\BenWighton\Downloads\StoreProcInstall (1).exe

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable disable
namespace StoreProcInstall
{
  public class frmMain : Form
  {
    public const string DefaultPath = "c:\\bepoz\\programs\\";
    public static string ExecutePath = "c:\\bepoz\\programs\\";
    public static string FilePath = "c:\\bepoz\\programs\\";
    public static string LitePath = "c:\\bepoz\\programs\\";
    public static string VersionPath = "c:\\bepoz\\programs\\";
    public static string ServerPath = "https://storedproc-host.s3.ap-southeast-2.amazonaws.com/";
    private bool _auto;
    private bool _downgrade;
    private bool _test;
    private static bool _is47;
    private static bool _is48;
    public static string _currentVer;
    private static Version _nullVersion = new Version(0, 0, 0, 0);
    private Version _bepozVersion;
    private IContainer components = (IContainer) null;
    private ListBox lstProcesses;
    private Button cmdRefresh;
    private TextBox txtPath;
    private Button cmdPath;
    private GroupBox groupBox1;
    private Label lblCurrent;
    private Label lblLatest;
    private Button cmdDownload;
    private Label lblProcNames;
    private Label lblLite;
    private CheckBox chkDLTest;
    private Label lblTest;

    public frmMain(bool auto, bool downgrade, bool test)
    {
      this.InitializeComponent();
      this._auto = auto;
      this._downgrade = downgrade;
      this._test = test;
      this.chkDLTest.Checked = test;
      using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Backoffice"))
      {
        frmMain.ExecutePath = (string) registryKey?.GetValue(nameof (ExecutePath)) ?? "c:\\bepoz\\programs\\";
        frmMain.FilePath = frmMain.ExecutePath + "\\job_storproc.dll";
        frmMain.LitePath = frmMain.ExecutePath + "\\LiteDB.dll";
        frmMain.VersionPath = frmMain.ExecutePath + "\\version.txt";
      }
    }

    private async void frmMain_Load(object sender, EventArgs e)
    {
      await this.CheckVersions();
      if (this._auto)
      {
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer()
        {
          Interval = 60000
        };
        t.Tick += (EventHandler) ((o, args) => Application.Exit());
        t.Start();
        try
        {
          int num = await this.AutoUpdate() ? 1 : 0;
        }
        catch
        {
          t.Dispose();
        }
        Application.Exit();
      }
      else
      {
        this.txtPath.Text = frmMain.FilePath;
        this.cmdRefresh_Click((object) null, (EventArgs) null);
        await this.CheckVersions();
      }
    }

    private static void ShutdownSC()
    {
      Process process = new Process();
      process.StartInfo.FileName = frmMain.ExecutePath + "\\sccommand.exe";
      process.StartInfo.Arguments = "MESSAGE SHUTDOWNSC";
      process.Start();
      process.WaitForExit();
    }

    private static void StartSC()
    {
      new Process()
      {
        StartInfo = {
          FileName = (frmMain.ExecutePath + "\\smartcontrol.exe")
        }
      }.Start();
    }

    private async Task<bool> AutoUpdate()
    {
      this.Enabled = false;
      Version current = frmMain.GetCurrentVersion();
      Version latest;
      if (this._test)
        latest = await frmMain.GetLatestTestVersionAsync();
      else
        latest = await frmMain.GetLatestVersionAsync();
      if (latest <= frmMain._nullVersion)
        return false;
      if (this._downgrade)
      {
        if (latest == current)
          return false;
      }
      else if (latest <= current)
        return false;
      int counter = 0;
      Process[] sc = Process.GetProcessesByName("smartcontrol");
      if (sc.Length >= 1)
      {
        frmMain.ShutdownSC();
        while (!sc[0].HasExited)
        {
          if (counter > 15)
          {
            Process[] processArray = sc;
            for (int index = 0; index < processArray.Length; ++index)
            {
              Process scInstance = processArray[index];
              scInstance.Kill();
              scInstance = (Process) null;
            }
            processArray = (Process[]) null;
          }
          Thread.Sleep(1000);
          ++counter;
          Application.DoEvents();
        }
      }
      Process[] processArray1 = Process.GetProcessesByName("backoffice");
      for (int index = 0; index < processArray1.Length; ++index)
      {
        Process b = processArray1[index];
        b.Kill();
        b = (Process) null;
      }
      processArray1 = (Process[]) null;
      this.cmdRefresh_Click((object) null, (EventArgs) null);
      if (this.lstProcesses.Items.Count <= 0)
        await this.UpdateLatest(true);
      Thread.Sleep(500);
      frmMain.StartSC();
      return true;
    }

    private async Task CheckVersions()
    {
      this._bepozVersion = frmMain.GetBepozVersion();
      int num = this._bepozVersion.Major;
      string str1 = num.ToString();
      num = this._bepozVersion.Minor;
      string str2 = num.ToString();
      frmMain._currentVer = str1 + str2;
      if (this._bepozVersion >= new Version(4, 7, 0, 0))
        frmMain._is47 = true;
      this.Text = string.Format("StoredProc - Bepoz v{0} - {1}", (object) this._bepozVersion, (object) Application.ProductVersion);
      Version current = frmMain.GetCurrentVersion();
      Version latest = await frmMain.GetLatestVersionAsync();
      Version test = await frmMain.GetLatestTestVersionAsync();
      string p = Path.GetDirectoryName(frmMain.FilePath);
      this.lblLite.Text = System.IO.File.Exists(frmMain.LitePath) ? "LiteBD: Found" : "LiteBD: Not Found";
      this.lblCurrent.Text = "Current: " + current?.ToString();
      this.lblLatest.Text = "Latest: " + latest?.ToString();
      this.lblTest.Text = "Test: " + test?.ToString();
      current = (Version) null;
      latest = (Version) null;
      test = (Version) null;
      p = (string) null;
    }

    private void cmdRefresh_Click(object sender, EventArgs e)
    {
      this.lstProcesses.Items.Clear();
      foreach (Process process in FileUtil.WhoIsLocking(frmMain.FilePath))
        this.lstProcesses.Items.Add((object) string.Format("{0:D6} : {1}", (object) process.Id, (object) (process.MainModule?.ModuleName ?? process.ProcessName)));
    }

    public static Version GetBepozVersion()
    {
      try
      {
        Version result;
        return !System.IO.File.Exists(frmMain.VersionPath) ? frmMain._nullVersion : (Version.TryParse(System.IO.File.ReadAllLines(frmMain.VersionPath)[0], out result) ? result : frmMain._nullVersion);
      }
      catch
      {
        return frmMain._nullVersion;
      }
    }

    public static Version GetCurrentVersion()
    {
      try
      {
        return !System.IO.File.Exists(frmMain.FilePath) ? frmMain._nullVersion : new Version(FileVersionInfo.GetVersionInfo(frmMain.FilePath).FileVersion);
      }
      catch
      {
        return frmMain._nullVersion;
      }
    }

    public static async Task<Version> GetVersionAsync(string file)
    {
      WebClient client = new WebClient();
      try
      {
        string version = await client.DownloadStringTaskAsync(frmMain.ServerPath + file);
        return new Version(version);
      }
      catch
      {
        return frmMain._nullVersion;
      }
    }

    public static async Task<Version> GetLatestVersionAsync()
    {
      Version versionAsync = await frmMain.GetVersionAsync(frmMain._is47 ? "StoredProcVersion" + frmMain._currentVer + ".txt" : "StoredProcVersion.txt");
      return versionAsync;
    }

    public static async Task<Version> GetLatestTestVersionAsync()
    {
      Version versionAsync = await frmMain.GetVersionAsync(frmMain._is47 ? "StoredProcVersionTest" + frmMain._currentVer + ".txt" : "StoredProcVersionTest.txt");
      return versionAsync;
    }

    public static async Task<byte[]> GetDataAsync(string path)
    {
      WebClient client = new WebClient();
      try
      {
        byte[] version = await client.DownloadDataTaskAsync(path);
        return version;
      }
      catch
      {
        return (byte[]) null;
      }
    }

    public static async Task<byte[]> GetLatestVersionFileAsync(bool test)
    {
      byte[] dataAsync = await frmMain.GetDataAsync(frmMain.ServerPath + "job_storproc" + (test ? ".test" : "") + (frmMain._is47 ? "." + frmMain._currentVer : "") + ".dll");
      return dataAsync;
    }

    public static async Task<byte[]> GetLiteDBAsync()
    {
      byte[] dataAsync = await frmMain.GetDataAsync(frmMain.ServerPath + "litedb.dll");
      return dataAsync;
    }

    private async Task UpdateLatest(bool suppress = false)
    {
      if (!suppress && System.IO.File.Exists(frmMain.FilePath) && MessageBox.Show("Are you sure you want to overwrite.\n", "File Exists", MessageBoxButtons.YesNo) == DialogResult.No)
        return;
      try
      {
        byte[] b = await frmMain.GetLatestVersionFileAsync(this.chkDLTest.Checked);
        if (b != null)
        {
          System.IO.File.WriteAllBytes(frmMain.FilePath, b);
          string path = Path.GetDirectoryName(frmMain.FilePath);
          if (!string.IsNullOrEmpty(path))
          {
            string[] extFiles = Directory.GetFiles(path, "apiext*.dll");
            string[] strArray = extFiles;
            for (int index = 0; index < strArray.Length; ++index)
            {
              string file = strArray[index];
              System.IO.File.Delete(file);
              file = (string) null;
            }
            strArray = (string[]) null;
            extFiles = (string[]) null;
          }
          if (!suppress)
          {
            int num = (int) MessageBox.Show("Done!");
          }
          await this.CheckVersions();
          path = (string) null;
        }
        else if (!suppress)
        {
          int num1 = (int) MessageBox.Show("Could not update file.");
        }
        b = (byte[]) null;
      }
      catch (Exception ex)
      {
        if (suppress)
          return;
        int num = (int) MessageBox.Show("Could not download file.\n" + ex.Message);
      }
    }

    private async void cmdDownload_Click(object sender, EventArgs e)
    {
      this.cmdRefresh_Click((object) null, (EventArgs) null);
      if (this.chkDLTest.Checked && MessageBox.Show("This will install the testing version.\nDo you want to continue?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
        return;
      if (this.lstProcesses.Items.Count > 0)
      {
        int num1 = (int) MessageBox.Show("Cannot update while processes are using the file.");
      }
      else
      {
        try
        {
          if (!System.IO.File.Exists(frmMain.LitePath))
          {
            byte[] dat = await frmMain.GetLiteDBAsync();
            if (dat != null && dat.Length != 0)
              System.IO.File.WriteAllBytes(frmMain.LitePath, dat);
            dat = (byte[]) null;
          }
        }
        catch (Exception ex)
        {
          int num2 = (int) MessageBox.Show("Could not download file.\n" + ex.Message);
        }
        await this.UpdateLatest();
      }
    }

    private async void cmdPath_Click(object sender, EventArgs e)
    {
      OpenFileDialog fop = new OpenFileDialog();
      fop.FileName = "job_storproc.dll";
      fop.Multiselect = false;
      string path = Path.GetDirectoryName(frmMain.FilePath);
      fop.InitialDirectory = path;
      if (fop.ShowDialog() == DialogResult.OK)
      {
        frmMain.FilePath = fop.FileName;
        this.txtPath.Text = frmMain.FilePath;
      }
      await this.CheckVersions();
      fop = (OpenFileDialog) null;
      path = (string) null;
    }

    private void chkDLTest_CheckedChanged(object sender, EventArgs e)
    {
      this.lblTest.Visible = this.chkDLTest.Checked;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.lstProcesses = new ListBox();
      this.cmdRefresh = new Button();
      this.txtPath = new TextBox();
      this.cmdPath = new Button();
      this.groupBox1 = new GroupBox();
      this.lblLite = new Label();
      this.cmdDownload = new Button();
      this.lblCurrent = new Label();
      this.lblLatest = new Label();
      this.lblProcNames = new Label();
      this.chkDLTest = new CheckBox();
      this.lblTest = new Label();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      this.lstProcesses.FormattingEnabled = true;
      this.lstProcesses.Location = new Point(12, 25);
      this.lstProcesses.Name = "lstProcesses";
      this.lstProcesses.Size = new Size(200, 251);
      this.lstProcesses.TabIndex = 0;
      this.cmdRefresh.Location = new Point(137, 282);
      this.cmdRefresh.Name = "cmdRefresh";
      this.cmdRefresh.Size = new Size(75, 23);
      this.cmdRefresh.TabIndex = 1;
      this.cmdRefresh.Text = "Refresh";
      this.cmdRefresh.UseVisualStyleBackColor = true;
      this.cmdRefresh.Click += new EventHandler(this.cmdRefresh_Click);
      this.txtPath.Location = new Point(218, 12);
      this.txtPath.Name = "txtPath";
      this.txtPath.Size = new Size(250, 20);
      this.txtPath.TabIndex = 2;
      this.cmdPath.Location = new Point(474, 12);
      this.cmdPath.Name = "cmdPath";
      this.cmdPath.Size = new Size(30, 20);
      this.cmdPath.TabIndex = 3;
      this.cmdPath.Text = "...";
      this.cmdPath.UseVisualStyleBackColor = true;
      this.cmdPath.Click += new EventHandler(this.cmdPath_Click);
      this.groupBox1.Controls.Add((Control) this.lblTest);
      this.groupBox1.Controls.Add((Control) this.lblLite);
      this.groupBox1.Controls.Add((Control) this.cmdDownload);
      this.groupBox1.Controls.Add((Control) this.lblCurrent);
      this.groupBox1.Controls.Add((Control) this.lblLatest);
      this.groupBox1.Location = new Point(218, 38);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(286, 267);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      this.lblLite.AutoSize = true;
      this.lblLite.Location = new Point(8, 41);
      this.lblLite.Name = "lblLite";
      this.lblLite.Size = new Size(42, 13);
      this.lblLite.TabIndex = 3;
      this.lblLite.Text = "LiteDB:";
      this.cmdDownload.Location = new Point(205, 238);
      this.cmdDownload.Name = "cmdDownload";
      this.cmdDownload.Size = new Size(75, 23);
      this.cmdDownload.TabIndex = 2;
      this.cmdDownload.Text = "Download";
      this.cmdDownload.UseVisualStyleBackColor = true;
      this.cmdDownload.Click += new EventHandler(this.cmdDownload_Click);
      this.lblCurrent.AutoSize = true;
      this.lblCurrent.Location = new Point(6, 28);
      this.lblCurrent.Name = "lblCurrent";
      this.lblCurrent.Size = new Size(44, 13);
      this.lblCurrent.TabIndex = 1;
      this.lblCurrent.Text = "Current:";
      this.lblLatest.AutoSize = true;
      this.lblLatest.Location = new Point(11, 15);
      this.lblLatest.Name = "lblLatest";
      this.lblLatest.Size = new Size(39, 13);
      this.lblLatest.TabIndex = 0;
      this.lblLatest.Text = "Latest:";
      this.lblProcNames.AutoSize = true;
      this.lblProcNames.Location = new Point(12, 9);
      this.lblProcNames.Name = "lblProcNames";
      this.lblProcNames.Size = new Size(97, 13);
      this.lblProcNames.TabIndex = 3;
      this.lblProcNames.Text = "Locking Processes";
      this.chkDLTest.AutoSize = true;
      this.chkDLTest.Location = new Point(12, 286);
      this.chkDLTest.Name = "chkDLTest";
      this.chkDLTest.Size = new Size(85, 17);
      this.chkDLTest.TabIndex = 5;
      this.chkDLTest.Text = "Test Version";
      this.chkDLTest.UseVisualStyleBackColor = true;
      this.chkDLTest.CheckedChanged += new EventHandler(this.chkDLTest_CheckedChanged);
      this.lblTest.AutoSize = true;
      this.lblTest.Location = new Point(19, 54);
      this.lblTest.Name = "lblTest";
      this.lblTest.Size = new Size(31, 13);
      this.lblTest.TabIndex = 4;
      this.lblTest.Text = "Test:";
      this.lblTest.Visible = false;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(515, 317);
      this.Controls.Add((Control) this.chkDLTest);
      this.Controls.Add((Control) this.lblProcNames);
      this.Controls.Add((Control) this.groupBox1);
      this.Controls.Add((Control) this.cmdPath);
      this.Controls.Add((Control) this.txtPath);
      this.Controls.Add((Control) this.cmdRefresh);
      this.Controls.Add((Control) this.lstProcesses);
      this.Name = nameof (frmMain);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "StoredProc";
      this.Load += new EventHandler(this.frmMain_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
