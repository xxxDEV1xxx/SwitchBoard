using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ButtonCommandBoard
{
    public class CommandBoard : Form
    {
        private Button[] buttons = new Button[16];
        private TextBox[] commandTextBoxes = new TextBox[16];
        private TextBox[] descriptionTextBoxes = new TextBox[16];
        private Label[] textBoxLabels = new Label[16];
        private readonly string commandsFile = "commands.txt";
        private readonly string descriptionsFile = "descriptions.txt";
        private readonly string resetFlagFile = "reset.flag";
        private readonly string debugLogFile = "debug.log";
        private IntPtr keyboardHookId = IntPtr.Zero;
        private LowLevelKeyboardProc keyboardProc;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        public CommandBoard()
        {
            this.Text = "Command Board";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(0, 0);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.Black;
            this.TopMost = true;

            InitializeControls();
            LoadCommands();
            LoadDescriptions();
            this.Resize += new EventHandler(Form_Resize);
            this.FormClosing += new FormClosingEventHandler(Form_Closing); // Fixed delegate
            this.Resize += new EventHandler(Form_WindowStateChanged);

            keyboardProc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
                LogDebug("Keyboard hook set: " + keyboardHookId.ToString());
            }

            LayoutControls();
        }

        private void LogDebug(string message)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(debugLogFile))
                {
                    writer.WriteLine("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message);
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (key >= Keys.F1 && key <= Keys.F12)
                {
                    int index = key - Keys.F1;
                    if (index < 12)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            buttons[index].PerformClick();
                            LogDebug(String.Format("F{0} pressed, triggered button {0}", index + 1));
                        });
                        return new IntPtr(1);
                    }
                }
            }
            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }

        private void InitializeControls()
        {
            string defaultCommand = "ipconfig /all";
            string openWrtCommand = "plink -l root -i C:\\Users\\DEV1\\.ssh\\id_rsa.ppk 192.168.1.1 \"conntrack -D -s 192.168.1.7 >> /tmp/clear_192.168.1.7.log 2>&1 & killall -HUP dnsmasq >> /tmp/clear_192.168.1.7.log 2>&1 & arp -d 192.168.1.7 >> /tmp/clear_192.168.1.7.log 2>&1 &\"";

            for (int i = 0; i < 16; i++)
            {
                buttons[i] = new Button
                {
                    Text = (i + 1).ToString(),
                    Tag = i,
                    FlatStyle = FlatStyle.Standard,
                    BackColor = (i < 4 || (i >= 8 && i < 12)) ? Color.FromArgb(140, 10, 13) : Color.FromArgb(22, 181, 4),
                    ForeColor = Color.Black,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                buttons[i].FlatAppearance.BorderSize = 2;
                buttons[i].FlatAppearance.MouseOverBackColor = Color.Cyan;
                buttons[i].FlatAppearance.MouseDownBackColor = Color.SkyBlue;
                buttons[i].Click += new EventHandler(Button_Click);
                this.Controls.Add(buttons[i]);
                LogDebug("Created button " + (i + 1).ToString());

                commandTextBoxes[i] = new TextBox
                {
                    Text = (i == 1) ? openWrtCommand : defaultCommand,
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = Color.Black,
                    ForeColor = Color.FromArgb(22, 181, 4)
                };
                this.Controls.Add(commandTextBoxes[i]);
                LogDebug("Created command TextBox " + (i + 1).ToString());

                descriptionTextBoxes[i] = new TextBox
                {
                    Text = (i == 1) ? "Clear connections and caches for 192.168.1.7 (simultaneous)" : "Description " + (i + 1).ToString(),
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = Color.Black,
                    ForeColor = Color.White
                };
                this.Controls.Add(descriptionTextBoxes[i]);
                LogDebug("Created description TextBox " + (i + 1).ToString());

                textBoxLabels[i] = new Label
                {
                    Text = (i + 1).ToString(),
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    AutoSize = true,
                    ForeColor = Color.White
                };
                this.Controls.Add(textBoxLabels[i]);
                LogDebug("Created TextBox label " + (i + 1).ToString());
            }
        }

        private void LoadCommands()
        {
            try
            {
                if (File.Exists(resetFlagFile) || !File.Exists(commandsFile))
                {
                    string defaultCommand = "ipconfig /all";
                    string openWrtCommand = "plink -l root -i C:\\Users\\DEV1\\.ssh\\id_rsa.ppk 192.168.1.1 \"conntrack -D -s 192.168.1.7 >> /tmp/clear_192.168.1.7.log 2>&1 & killall -HUP dnsmasq >> /tmp/clear_192.168.1.7.log 2>&1 & arp -d 192.168.1.7 >> /tmp/clear_192.168.1.7.log 2>&1 &\"";
                    for (int i = 0; i < 16; i++)
                    {
                        commandTextBoxes[i].Text = (i == 1) ? openWrtCommand : defaultCommand;
                    }
                    if (File.Exists(resetFlagFile))
                    {
                        File.Delete(resetFlagFile);
                        LogDebug("Reset commands to default due to reset.flag");
                    }
                    return;
                }

                string[] commands = File.ReadAllLines(commandsFile);
                for (int i = 0; i < Math.Min(commands.Length, 16); i++)
                {
                    commandTextBoxes[i].Text = commands[i];
                    LogDebug("Loaded command " + (i + 1).ToString() + ": " + commands[i]);
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error loading commands: " + ex.Message);
            }
        }

        private void LoadDescriptions()
        {
            try
            {
                if (File.Exists(descriptionsFile))
                {
                    string[] descriptions = File.ReadAllLines(descriptionsFile);
                    for (int i = 0; i < Math.Min(descriptions.Length, 16); i++)
                    {
                        descriptionTextBoxes[i].Text = descriptions[i];
                        LogDebug("Loaded description " + (i + 1).ToString() + ": " + descriptions[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error loading descriptions: " + ex.Message);
            }
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            try
            {
                string[] commands = new string[16];
                string[] descriptions = new string[16];
                for (int i = 0; i < 16; i++)
                {
                    commands[i] = commandTextBoxes[i].Text;
                    descriptions[i] = descriptionTextBoxes[i].Text;
                }
                File.WriteAllLines(commandsFile, commands);
                File.WriteAllLines(descriptionsFile, descriptions);
                LogDebug("Saved commands to " + commandsFile);
                LogDebug("Saved descriptions to " + descriptionsFile);

                if (keyboardHookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(keyboardHookId);
                    LogDebug("Keyboard hook removed");
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error saving commands/descriptions or removing hook: " + ex.Message);
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            LayoutControls();
        }

        private void Form_WindowStateChanged(object sender, EventArgs e)
        {
            this.TopMost = (this.WindowState != FormWindowState.Minimized);
            LogDebug("Window state changed to " + this.WindowState.ToString() + ", TopMost = " + this.TopMost.ToString());
        }

        private void LayoutControls()
        {
            int margin = 20;
            int gridSize = 4;
            int buttonSize = 50;
            int buttonSpacing = 30;
            int commandWidth = 200;
            int descriptionWidth = 200;
            int textBoxHeight = 30;
            int labelWidth = 30;
            int commandSpacing = 10;

            int totalGridWidth = gridSize * buttonSize + (gridSize - 1) * buttonSpacing;

            int startX = margin;
            int startY = margin;

            for (int i = 0; i < 16; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;

                buttons[i].Size = new Size(buttonSize, buttonSize);
                buttons[i].Location = new Point(
                    startX + col * (buttonSize + buttonSpacing),
                    startY + row * (buttonSize + buttonSpacing)
                );
                LogDebug("Positioned button " + (i + 1).ToString() + " at (" + buttons[i].Location.X.ToString() + "," + buttons[i].Location.Y.ToString() + ")");
            }

            for (int i = 0; i < 16; i++)
            {
                textBoxLabels[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing,
                    startY + i * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                );
                LogDebug("Positioned TextBox label " + (i + 1).ToString() + " at (" + textBoxLabels[i].Location.X.ToString() + "," + textBoxLabels[i].Location.Y.ToString() + ")");

                commandTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth,
                    startY + i * textBoxHeight
                );
                commandTextBoxes[i].Size = new Size(commandWidth, textBoxHeight);
                LogDebug("Positioned command TextBox " + (i + 1).ToString() + " at (" + commandTextBoxes[i].Location.X.ToString() + "," + textBoxLabels[i].Location.Y.ToString() + ")");

                descriptionTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth + commandWidth + commandSpacing,
                    startY + i * textBoxHeight
                );
                descriptionTextBoxes[i].Size = new Size(descriptionWidth, textBoxHeight);
                LogDebug("Positioned description TextBox " + (i + 1).ToString() + " at (" + descriptionTextBoxes[i].Location.X.ToString() + "," + descriptionTextBoxes[i].Location.Y.ToString() + ")");
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int index = (int)btn.Tag;
            string command = commandTextBoxes[index].Text.Trim();

            if (string.IsNullOrEmpty(command))
            {
                MessageBox.Show("Please enter a command in the TextBox.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    string result = string.IsNullOrEmpty(error) ? output : "Error: " + error;
                    MessageBox.Show(result, "Output of Command " + (index + 1).ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute command: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CommandBoard());
        }
    }
}