using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace beeble_install
{
    public partial class baseWindow : Form
    {
        public baseWindow()
        {
            InitializeComponent();
            txtPath.ReadOnly = true;
            txtPath.Cursor = Cursors.Hand;
            txtPath.Click += txtPath_Click;
            txtPath.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "ROBLOX-beeble");
            RefreshButtons();

            txtPath.TextChanged += delegate (object s, EventArgs e)
            {
                RefreshButtons();
            };
        }

        private void RefreshButtons()
        {
            bool flag = File.Exists(Path.Combine(txtPath.Text.Trim(), "08launcher.exe"));
            btnInstall.Enabled = !flag;
            btnRepair.Enabled = flag;
            btnUninstall.Enabled = flag;
        }

        private void SetStatus(int pct)
        {
            progress.Value = Math.Max(0, Math.Min(100, pct));
            Application.DoEvents();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            Run(delegate ()
            {
                Install(txtPath.Text.Trim());
            }, "sucessfully installed");
        }

        private void btnRepair_Click(object sender, EventArgs e)
        {
            Run(delegate ()
            {
                Install(txtPath.Text.Trim());
            }, "sucessfully reapaired files");
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("uninstall client?", "bebl setoop",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Run(delegate ()
                {
                    Uninstall(txtPath.Text.Trim());
                }, "sucessfully uninstalled");
            }
        }

        private void link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://beeble.top");
        }

        private void txtPath_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtPath.Text))
            {
                folderBrowserDialog1.SelectedPath = txtPath.Text;
            }
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = folderBrowserDialog1.SelectedPath;
                RefreshButtons();
            }
        }

        private void captionInstall_Click(object sender, EventArgs e)
        {
        }

        private void captionRepair_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click_2(object sender, EventArgs e)
        {
        }

        private void promoBanner_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void progress_Click(object sender, EventArgs e)
        {
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {
        }

        private void Run(Action work, string ok)
        {
            btnInstall.Enabled = false;
            btnRepair.Enabled = false;
            btnUninstall.Enabled = false;

            try
            {
                work();
                SetStatus(100);
                MessageBox.Show(ok, "bebl setoop");
            }
            catch (Exception ex)
            {
                SetStatus(0);
                MessageBox.Show(ex.Message, "bebl setoop");
            }

            RefreshButtons();
        }

        private void Install(string app)
        {
            SetStatus(5);

            if (Directory.Exists(app))
            {
                Directory.Delete(app, true);
            }
            Directory.CreateDirectory(app);

            string zipPath = Path.Combine(Path.GetTempPath(), "client.zip");
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string resourceName = null;

            string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
            foreach (string name in manifestResourceNames)
            {
                if (name.EndsWith("client.zip"))
                {
                    resourceName = name;
                    break;
                }
            }

            if (resourceName == null)
            {
                throw new Exception("cannot find client build");
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(resourceName))
            using (FileStream destination = File.Create(zipPath))
            {
                stream.CopyTo(destination);
            }

            SetStatus(25);

            ZipFile.ExtractToDirectory(zipPath, app);

            try
            {
                File.Delete(zipPath);
            }
            catch
            {
            }

            string launcherPath = Path.Combine(app, "08launcher.exe");
            string dll = Path.Combine(app, "RobloxLauncher.dll");

            SetStatus(70);

            using (RegistryKey registryKey = Registry.LocalMachine.CreateSubKey("Software\\Classes\\r08"))
            {
                registryKey.SetValue("", "URL:beeble uri");
                registryKey.SetValue("URL Protocol", "");
            }

            using (RegistryKey registryKey2 = Registry.LocalMachine.CreateSubKey("Software\\Classes\\r08\\shell\\open\\command"))
            {
                registryKey2.SetValue("", "\"" + launcherPath + "\" \"%1\"");
            }

            SetStatus(85);

            Regsvr(dll, false);

            SetStatus(100);
        }

        private void Uninstall(string app)
        {
            SetStatus(20);

            Regsvr(Path.Combine(app, "RobloxLauncher.dll"), true);

            SetStatus(50);

            try
            {
                Registry.LocalMachine.DeleteSubKeyTree("Software\\Classes\\r08", false);
            }
            catch
            {
            }

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\r08", false);
            }
            catch
            {
            }

            SetStatus(80);

            if (Directory.Exists(app))
            {
                Directory.Delete(app, true);
            }

            SetStatus(100);
        }

        private static void Regsvr(string dll, bool uninstall)
        {
            string regsvrPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.Is64BitOperatingSystem ? "SysWOW64\\regsvr32.exe" : "System32\\regsvr32.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = File.Exists(regsvrPath) ? regsvrPath : "regsvr32.exe";
            startInfo.Arguments = (uninstall ? "/s /u " : "/s ") + "\"" + dll + "\"";
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Path.GetDirectoryName(dll);

            Process process = Process.Start(startInfo);
            if (process != null)
            {
                process.WaitForExit();
            }
        }
    }
}