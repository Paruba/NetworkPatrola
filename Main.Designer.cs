using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace NetworkMonitor
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private BackgroundWorker backgroundWorker;
        private static string logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs.txt");

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Main));
            reportBox = new TextBox();
            backgroundWorker = new BackgroundWorker();
            notifyIcon1 = new NotifyIcon(components);
            SuspendLayout();
            // 
            // reportBox
            // 
            reportBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            reportBox.Location = new Point(12, 12);
            reportBox.Multiline = true;
            reportBox.Name = "reportBox";
            reportBox.ScrollBars = ScrollBars.Vertical;
            reportBox.Size = new Size(776, 426);
            reportBox.TabIndex = 0;
            reportBox.WordWrap = false;
            
            // 
            // notifyIcon1
            // 
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            notifyIcon1.MouseUp += notifyIcon1_MouseUp;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(reportBox);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Main";
            Text = "Network status";
            TopMost = true;
            FormClosing += Main_FormClosing;
            ResumeLayout(false);
            PerformLayout();

            // 
            // backgroundWorker
            // 
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            backgroundWorker.DoWork += CheackBeat;
            backgroundWorker.RunWorkerAsync();
        }

        private void CheackBeat(object sender, DoWorkEventArgs e)
        {
            int tested = 0;
            while (true)
            {
                if (!CheckConnectivity())
                {
                    try
                    {
                        if (tested > 4)
                        {
                            RestartNetworkAdapter();
                            tested = 0;
                        }
                        else
                        {
                            tested += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        var netowrkAdapterErr = ex.Message;
                        try { 
                            SaveMessage(netowrkAdapterErr);
                        } catch (Exception ex2)
                        {
                            Console.WriteLine(ex2);
                        }
                    }
                }

                System.Threading.Thread.Sleep(5000);
            }
        }

        private bool CheckConnectivity()
        {
            Ping ping = new Ping();
            try
            {
                UpdateTextBox($"{DateTime.Now} - Checking connectivity...");
                PingReply reply = ping.Send("8.8.8.8", 1000);
                return (reply.Status == IPStatus.Success);
            }
            catch
            {
                UpdateTextBox($"{DateTime.Now} - Connection not succed...");
                return false;
            }
        }

        private void RestartNetworkAdapter()
        {
            UpdateTextBox("Restarting network adapter...");

            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach(var adapter in adapters) { 
                Process.Start("netsh", $"interface set interface \"{adapter.Name}\" admin=disable");
                System.Threading.Thread.Sleep(5000);
                Process.Start("netsh", $"interface set interface \"{adapter.Name}\" admin=enable");
            }
            RunCommand("ipconfig /renew");

            string logMessage = $"{DateTime.Now}: Network adapter restarted.";
            File.AppendAllText(logPath, logMessage + Environment.NewLine);

            UpdateTextBox("Network adapter restarted.");
        }

        private static string RunCommand(string command)
        {
            Process process = new Process();

            // Set the start info for the process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            process.StartInfo = startInfo;

            // Start the process
            process.Start();

            process.StandardInput.WriteLine(command);

            // End the input stream
            process.StandardInput.Close();

            // Read the output of the PowerShell commands
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private void UpdateTextBox(string text)
        {
            if (reportBox.InvokeRequired)
            {
                reportBox.Invoke(new Action<string>(UpdateTextBox), text);
            }
            else
            {
                reportBox.AppendText(text + Environment.NewLine);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            this.Hide();
        }

        private void notifyIcon1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                ToolStripMenuItem exitToolStripMenuItem = new ToolStripMenuItem("Exit");
                exitToolStripMenuItem.Click += new EventHandler(Exit_Click);
                contextMenuStrip.Items.Add(exitToolStripMenuItem);
                notifyIcon1.ContextMenuStrip = contextMenuStrip;
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void Exit_Click(object Sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void SaveMessage(string message)
        {
            if (!File.Exists(logPath))
            {
                using (FileStream fs = File.Create(logPath))
                {
                    fs.Close();
                }
            }
            File.AppendAllText(logPath, message);
        }

        #endregion

        private TextBox reportBox;
        private NotifyIcon notifyIcon1;
    }
}