using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq; // Added for Contains
using System.ComponentModel; // Added for BackgroundWorker

namespace ButtonCommandBoard
{
    public class CommandBoard : Form
    {
        private Button[] buttons = new Button[16];
        private TextBox[] commandTextBoxes = new TextBox[16];
        private TextBox[] descriptionTextBoxes = new TextBox[16];
        private Label[] textBoxLabels = new Label[16];
        private Button plusButton;
        private Button minusButton;
        private Label pageNumberLabel;
        private TextBox pageDescriptionTextBox;
        private readonly string commandsFile = "commands.txt";
        private readonly string descriptionsFile = "descriptions.txt";
        private readonly string resetFlagFile = "reset.flag";
        private readonly string debugLogFile = "debug.log";
        private IntPtr keyboardHookId = IntPtr.Zero;
        private LowLevelKeyboardProc keyboardProc;
        private int currentPage = 1; // 1-based: Page 1 (buttons 1–16), Page 2 (17–32), etc.
        private const int MaxPages = 5; // Up to 80 buttons (5 pages * 16)

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
            this.MinimumSize = new Size(100, 100);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.Black;
            this.TopMost = true;

            InitializeControls();
            LoadCommands();
            LoadDescriptions();
            this.Resize += new EventHandler(Form_Resize);
            this.FormClosing += new FormClosingEventHandler(Form_Closing);
            this.Resize += new EventHandler(Form_WindowStateChanged);

            keyboardProc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, GetModuleHandle(null), 0);
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
                            LogDebug(String.Format("F{0} pressed, triggered button {1} on page {2}", index + 1, (currentPage - 1) * 16 + index + 1, currentPage));
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

            for (int i = 0; i < 16; i++)
            {
                buttons[i] = new Button
                {
                    Text = ((currentPage - 1) * 16 + i + 1).ToString(),
                    Tag = i,
                    FlatStyle = FlatStyle.Standard,
                    BackColor = new int[] { 0, 2, 5, 7, 8, 10, 13, 15 }.Contains(i) ? Color.FromArgb(140, 10, 13) : Color.FromArgb(22, 181, 4),
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
                    Text = defaultCommand,
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = Color.Black,
                    ForeColor = Color.FromArgb(22, 181, 4)
                };
                this.Controls.Add(commandTextBoxes[i]);
                LogDebug("Created command TextBox " + (i + 1).ToString());

                descriptionTextBoxes[i] = new TextBox
                {
                    Text = "Description " + ((currentPage - 1) * 16 + i + 1).ToString(),
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = Color.Black,
                    ForeColor = Color.White
                };
                this.Controls.Add(descriptionTextBoxes[i]);
                LogDebug("Created description TextBox " + (i + 1).ToString());

                textBoxLabels[i] = new Label
                {
                    Text = ((currentPage - 1) * 16 + i + 1).ToString(),
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    AutoSize = true,
                    ForeColor = Color.White
                };
                this.Controls.Add(textBoxLabels[i]);
                LogDebug("Created TextBox label " + (i + 1).ToString());
            }

            // Plus and Minus buttons
            minusButton = new Button
            {
                Text = "-",
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Standard,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            minusButton.Click += new EventHandler(MinusButton_Click);
            this.Controls.Add(minusButton);

            plusButton = new Button
            {
                Text = "+",
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Standard,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            plusButton.Click += new EventHandler(PlusButton_Click);
            this.Controls.Add(plusButton);

            // Page number label
            pageNumberLabel = new Label
            {
                Text = currentPage.ToString(),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.White
            };
            this.Controls.Add(pageNumberLabel);

            // Page description TextBox
            pageDescriptionTextBox = new TextBox
            {
                Text = "Page " + currentPage.ToString() + " Description",
                Font = new Font("Arial", 10),
                Width = 200,
                BackColor = Color.Black,
                ForeColor = Color.White
            };
            this.Controls.Add(pageDescriptionTextBox);
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (currentPage < MaxPages)
            {
                SaveCurrentPageData();
                currentPage++;
                UpdatePage();
                LogDebug("Switched to page " + currentPage.ToString());
            }
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                SaveCurrentPageData();
                currentPage--;
                UpdatePage();
                LogDebug("Switched to page " + currentPage.ToString());
            }
        }

        private void UpdatePage()
        {
            pageNumberLabel.Text = currentPage.ToString();
            pageDescriptionTextBox.Text = "Page " + currentPage.ToString() + " Description";

            for (int i = 0; i < 16; i++)
            {
                int buttonNumber = (currentPage - 1) * 16 + i + 1;
                buttons[i].Text = buttonNumber.ToString();
                textBoxLabels[i].Text = buttonNumber.ToString();
                commandTextBoxes[i].Text = "ipconfig /all";
                descriptionTextBoxes[i].Text = "Description " + buttonNumber.ToString();
            }

            LoadCommands();
            LoadDescriptions();
            LayoutControls();
        }

        private void SaveCurrentPageData()
        {
            try
            {
                string[] allCommands = File.Exists(commandsFile) ? File.ReadAllLines(commandsFile) : new string[MaxPages * 16];
                string[] allDescriptions = File.Exists(descriptionsFile) ? File.ReadAllLines(descriptionsFile) : new string[MaxPages * 16 + MaxPages];

                if (allCommands.Length < MaxPages * 16)
                    Array.Resize(ref allCommands, MaxPages * 16);
                if (allDescriptions.Length < MaxPages * 16 + MaxPages)
                    Array.Resize(ref allDescriptions, MaxPages * 16 + MaxPages);

                for (int i = 0; i < 16; i++)
                {
                    int index = (currentPage - 1) * 16 + i;
                    allCommands[index] = commandTextBoxes[i].Text;
                    allDescriptions[index] = descriptionTextBoxes[i].Text;
                }
                allDescriptions[MaxPages * 16 + currentPage - 1] = pageDescriptionTextBox.Text;

                File.WriteAllLines(commandsFile, allCommands);
                File.WriteAllLines(descriptionsFile, allDescriptions);
                LogDebug("Saved data for page " + currentPage.ToString());
            }
            catch (Exception ex)
            {
                LogDebug("Error saving page data: " + ex.Message);
            }
        }

        private void LoadCommands()
        {
            try
            {
                if (File.Exists(resetFlagFile) || !File.Exists(commandsFile))
                {
                    for (int i = 0; i < 16; i++)
                    {
                        commandTextBoxes[i].Text = "ipconfig /all";
                    }
                    if (File.Exists(resetFlagFile))
                    {
                        File.Delete(resetFlagFile);
                        LogDebug("Reset commands to default due to reset.flag");
                    }
                    return;
                }

                string[] commands = File.ReadAllLines(commandsFile);
                for (int i = 0; i < 16; i++)
                {
                    int index = (currentPage - 1) * 16 + i;
                    if (index < commands.Length)
                    {
                        commandTextBoxes[i].Text = commands[index];
                        LogDebug("Loaded command " + (index + 1).ToString() + ": " + commands[index]);
                    }
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
                    for (int i = 0; i < 16; i++)
                    {
                        int index = (currentPage - 1) * 16 + i;
                        if (index < descriptions.Length)
                        {
                            descriptionTextBoxes[i].Text = descriptions[index];
                            LogDebug("Loaded description " + (index + 1).ToString() + ": " + descriptions[index]);
                        }
                    }
                    int pageDescIndex = MaxPages * 16 + currentPage - 1;
                    if (pageDescIndex < descriptions.Length)
                    {
                        pageDescriptionTextBox.Text = descriptions[pageDescIndex];
                        LogDebug("Loaded page description for page " + currentPage.ToString() + ": " + descriptions[pageDescIndex]);
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
            SaveCurrentPageData();

            try
            {
                if (keyboardHookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(keyboardHookId);
                    LogDebug("Keyboard hook removed");
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error removing hook: " + ex.Message);
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

            // Button grid (4x4)
            for (int i = 0; i < 16; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;

                buttons[i].Size = new Size(buttonSize, buttonSize);
                buttons[i].Location = new Point(
                    startX + col * (buttonSize + buttonSpacing),
                    startY + row * (buttonSize + buttonSpacing)
                );
                LogDebug("Positioned button " + ((currentPage - 1) * 16 + i + 1).ToString() + " at (" + buttons[i].Location.X.ToString() + "," + buttons[i].Location.Y.ToString() + ")");
            }

            // TextBox labels, command TextBoxes, description TextBoxes
            for (int i = 0; i < 16; i++)
            {
                textBoxLabels[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing,
                    startY + i * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                );
                LogDebug("Positioned TextBox label " + ((currentPage - 1) * 16 + i + 1).ToString() + " at (" + textBoxLabels[i].Location.X.ToString() + "," + textBoxLabels[i].Location.Y.ToString() + ")");

                commandTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth,
                    startY + i * textBoxHeight
                );
                commandTextBoxes[i].Size = new Size(commandWidth, textBoxHeight);
                LogDebug("Positioned command TextBox " + ((currentPage - 1) * 16 + i + 1).ToString() + " at (" + commandTextBoxes[i].Location.X.ToString() + "," + commandTextBoxes[i].Location.Y.ToString() + ")");

                descriptionTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth + commandWidth + commandSpacing,
                    startY + i * textBoxHeight
                );
                descriptionTextBoxes[i].Size = new Size(descriptionWidth, textBoxHeight);
                LogDebug("Positioned description TextBox " + ((currentPage - 1) * 16 + i + 1).ToString() + " at (" + descriptionTextBoxes[i].Location.X.ToString() + "," + descriptionTextBoxes[i].Location.Y.ToString() + ")");
            }

            // Plus/Minus buttons below button 13 (bottom-left of grid)
            int gridBottom = startY + 4 * (buttonSize + buttonSpacing) - buttonSpacing;
            minusButton.Location = new Point(startX, gridBottom + 10);
            plusButton.Location = new Point(startX + 40, gridBottom + 10);

            // Page number and description (tightly against page number)
            pageNumberLabel.Location = new Point(startX + 80, gridBottom + 15);
            pageDescriptionTextBox.Location = new Point(startX + 80 + pageNumberLabel.Width + 5, gridBottom + 10);
            pageDescriptionTextBox.Size = new Size(200, textBoxHeight);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int index = (int)btn.Tag;
            string cmd = commandTextBoxes[index].Text.Trim();

            if (string.IsNullOrEmpty(cmd))
            {
                MessageBox.Show("Please enter a command in the TextBox.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Disable the button to prevent multiple clicks
            btn.Enabled = false;

            // Set up BackgroundWorker
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, args) =>
            {
                try
                {
                    string resolvedCmd = ResolveCommand(cmd);
                    ProcessStartInfo psi;

                    // For sfc /scannow, use a visible cmd window
                    if (resolvedCmd.ToLower().Contains("sfc") || resolvedCmd.ToLower().Contains("scannow"))
                    {
                        psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/K " + resolvedCmd, // /K keeps the window open
                            UseShellExecute = true, // Required for visible window
                            CreateNoWindow = false,
                            Verb = "runas" // Request elevation for sfc
                        };
                    }
                    else
                    {
                        // For other commands, capture output
                        psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/C " + resolvedCmd, // /C closes the window after execution
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                    }

                    using (Process process = new Process { StartInfo = psi })
                    {
                        process.Start();
                        string result;

                        if (resolvedCmd.ToLower().Contains("sfc") || resolvedCmd.ToLower().Contains("scannow"))
                        {
                            // Wait for the process to exit (user will close the cmd window)
                            process.WaitForExit();
                            result = "Command executed in external window.";
                        }
                        else
                        {
                            // Capture output for non-sfc commands
                            string output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();
                            process.WaitForExit();
                            result = string.IsNullOrEmpty(error) ? output : "Error: " + error;
                        }

                        args.Result = new Tuple<string, int, bool>(result, index, true);
                    }
                }
                catch (Exception ex)
                {
                    args.Result = new Tuple<string, int, bool>("Error: " + ex.Message, index, false);
                }
            };

            worker.RunWorkerCompleted += (s, args) =>
            {
                Tuple<string, int, bool> result = (Tuple<string, int, bool>)args.Result;
                string output = result.Item1;
                int resultIndex = result.Item2;
                bool success = result.Item3;

                btn.Enabled = true;

                if (success && !string.IsNullOrEmpty(output))
                {
                    // Show output only for non-sfc commands
                    if (!(cmd.ToLower().Contains("sfc") || cmd.ToLower().Contains("scannow")))
                    {
                        MessageBox.Show(output, "Output of Command " + ((currentPage - 1) * 16 + resultIndex + 1).ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    LogDebug("Command " + ((currentPage - 1) * 16 + resultIndex + 1).ToString() + " executed");
                }
                else if (success)
                {
                    LogDebug("Command " + ((currentPage - 1) * 16 + resultIndex + 1).ToString() + " executed with no output");
                }
                else
                {
                    MessageBox.Show(output, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogDebug("Error executing command: " + output);
                }
            };

            worker.RunWorkerAsync();
        }

        private string ResolveCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return cmd;

            string[] parts = cmd.Split(new[] { ' ' }, 2);
            string executable = parts[0];
            string arguments = parts.Length > 1 ? parts[1] : "";

            if (File.Exists(executable))
                return cmd;

            string[] paths = Environment.GetEnvironmentVariable("PATH").Split(';');
            foreach (string path in paths)
            {
                string fullPath = Path.Combine(path, executable);
                if (File.Exists(fullPath))
                    return arguments.Length > 0 ? "\"" + fullPath + "\" " + arguments : fullPath;
                fullPath = Path.Combine(path, executable + ".exe");
                if (File.Exists(fullPath))
                    return arguments.Length > 0 ? "\"" + fullPath + "\" " + arguments : fullPath;
            }

            return cmd;
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