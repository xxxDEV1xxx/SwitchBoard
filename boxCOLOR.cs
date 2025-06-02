using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
using System.ComponentModel;

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
        private Button editButton;
        private Label pageNumberLabel;
        private TextBox pageDescriptionTextBox;
        private readonly string commandsFile = "commands.txt";
        private readonly string descriptionsFile = "descriptions.txt";
        private readonly string resetFlagFile = "reset.flag";
        private IntPtr keyboardHookId = IntPtr.Zero; // Fixed: Changed IntPtr to IntPtr.Zero
        private LowLevelKeyboardProc keyboardProc;
        private int currentPage = 1;
        private const int MaxPages = 5;

        public Color Button1BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button1ForeColor = Color.Black;
        public Color Button2BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button2ForeColor = Color.Black;
        public Color Button3BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button3ForeColor = Color.Black;
        public Color Button4BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button4ForeColor = Color.Black;
        public Color Button5BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button5ForeColor = Color.Black;
        public Color Button6BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button6ForeColor = Color.Black;
        public Color Button7BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button7ForeColor = Color.Black;
        public Color Button8BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button8ForeColor = Color.Black;
        public Color Button9BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button9ForeColor = Color.White;
        public Color Button10BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button10ForeColor = Color.Black;
        public Color Button11BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button11ForeColor = Color.Black;
        public Color Button12BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button12ForeColor = Color.Black;
        public Color Button13BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button13ForeColor = Color.Black;
        public Color Button14BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button14ForeColor = Color.Black;
        public Color Button15BackColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color Button15ForeColor = Color.Black;
        public Color Button16BackColor = Color.FromArgb(140, 10, 13); // Fixed: FromRgb to FromArgb
        public Color Button16ForeColor = Color.Black;

        public Color CommandTextBoxBackColor = Color.Black;
        public Color CommandTextBoxForeColor = Color.FromArgb(22, 181, 4); // Fixed: FromRgb to FromArgb
        public Color DescriptionTextBoxBackColor = Color.Black;
        public Color DescriptionTextBoxForeColor = Color.White;
        public Color TextBoxLabelBackColor = Color.Black;
        public Color TextBoxLabelForeColor = Color.White;
        public Color PlusMinusEditButtonBackColor = Color.Gray;
        public Color PlusMinusEditButtonForeColor = Color.White;
        public Color PageNumberLabelBackColor = Color.Black;
        public Color PageNumberLabelForeColor = Color.White;
        public Color PageDescriptionTextBoxBackColor = Color.Black;
        public Color PageDescriptionTextBoxForeColor = Color.White;
        public Color FormBackColor = Color.Black;

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
            this.BackColor = FormBackColor;
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
            }

            LayoutControls();
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
            Color[] buttonBackColors = new Color[]
            {
                Button1BackColor, Button2BackColor, Button3BackColor, Button4BackColor,
                Button5BackColor, Button6BackColor, Button7BackColor, Button8BackColor,
                Button9BackColor, Button10BackColor, Button11BackColor, Button12BackColor,
                Button13BackColor, Button14BackColor, Button15BackColor, Button16BackColor
            };
            Color[] buttonForeColors = new Color[]
            {
                Button1ForeColor, Button2ForeColor, Button3ForeColor, Button4ForeColor,
                Button5ForeColor, Button6ForeColor, Button7ForeColor, Button8ForeColor,
                Button9ForeColor, Button10ForeColor, Button11ForeColor, Button12ForeColor,
                Button13ForeColor, Button14ForeColor, Button15ForeColor, Button16ForeColor
            };

            for (int i = 0; i < 16; i++)
            {
                buttons[i] = new Button
                {
                    Text = ((currentPage - 1) * 16 + i + 1).ToString(),
                    Tag = i,
                    FlatStyle = FlatStyle.Standard,
                    BackColor = buttonBackColors[i],
                    ForeColor = buttonForeColors[i],
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                buttons[i].FlatAppearance.BorderSize = 2;
                buttons[i].FlatAppearance.MouseOverBackColor = Color.Cyan;
                buttons[i].FlatAppearance.MouseDownBackColor = Color.SkyBlue;
                buttons[i].Click += new EventHandler(Button_Click);
                this.Controls.Add(buttons[i]);

                commandTextBoxes[i] = new TextBox
                {
                    Text = defaultCommand,
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = CommandTextBoxBackColor,
                    ForeColor = CommandTextBoxForeColor
                };
                this.Controls.Add(commandTextBoxes[i]);

                descriptionTextBoxes[i] = new TextBox
                {
                    Text = "Description " + ((currentPage - 1) * 16 + i + 1).ToString(),
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = DescriptionTextBoxBackColor,
                    ForeColor = DescriptionTextBoxForeColor
                };
                this.Controls.Add(descriptionTextBoxes[i]);

                textBoxLabels[i] = new Label
                {
                    Text = ((currentPage - 1) * 16 + i + 1).ToString(),
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    AutoSize = true,
                    BackColor = TextBoxLabelBackColor,
                    ForeColor = TextBoxLabelForeColor
                };
                this.Controls.Add(textBoxLabels[i]);
            }

            minusButton = new Button
            {
                Text = "-",
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Standard,
                BackColor = PlusMinusEditButtonBackColor,
                ForeColor = PlusMinusEditButtonForeColor,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            minusButton.Click += new EventHandler(MinusButton_Click);
            this.Controls.Add(minusButton);

            plusButton = new Button
            {
                Text = "+",
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Standard,
                BackColor = PlusMinusEditButtonBackColor,
                ForeColor = PlusMinusEditButtonForeColor,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            plusButton.Click += new EventHandler(PlusButton_Click);
            this.Controls.Add(plusButton);

            editButton = new Button
            {
                Text = "Edit",
                Size = new Size(50, 30),
                FlatStyle = FlatStyle.Standard,
                BackColor = PlusMinusEditButtonBackColor,
                ForeColor = PlusMinusEditButtonForeColor,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            editButton.Click += new EventHandler(EditButton_Click);
            this.Controls.Add(editButton);

            pageNumberLabel = new Label
            {
                Text = currentPage.ToString(),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                BackColor = PageNumberLabelBackColor,
                ForeColor = PageNumberLabelForeColor
            };
            this.Controls.Add(pageNumberLabel);

            pageDescriptionTextBox = new TextBox
            {
                Text = "Page " + currentPage.ToString() + " Description",
                Font = new Font("Arial", 10),
                Width = 200,
                BackColor = PageDescriptionTextBoxBackColor,
                ForeColor = PageDescriptionTextBoxForeColor
            };
            this.Controls.Add(pageDescriptionTextBox);
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            using (ColorPaletteForm paletteForm = new ColorPaletteForm(this))
            {
                if (paletteForm.ShowDialog() == DialogResult.OK)
                {
                    Button1BackColor = paletteForm.Button1BackColor;
                    Button1ForeColor = paletteForm.Button1ForeColor;
                    Button2BackColor = paletteForm.Button2BackColor;
                    Button2ForeColor = paletteForm.Button2ForeColor;
                    Button3BackColor = paletteForm.Button3BackColor;
                    Button3ForeColor = paletteForm.Button3ForeColor;
                    Button4BackColor = paletteForm.Button4BackColor;
                    Button4ForeColor = paletteForm.Button4ForeColor;
                    Button5BackColor = paletteForm.Button5BackColor;
                    Button5ForeColor = paletteForm.Button5ForeColor;
                    Button6BackColor = paletteForm.Button6BackColor;
                    Button6ForeColor = paletteForm.Button6ForeColor;
                    Button7BackColor = paletteForm.Button7BackColor;
                    Button7ForeColor = paletteForm.Button7ForeColor;
                    Button8BackColor = paletteForm.Button8BackColor;
                    Button8ForeColor = paletteForm.Button8ForeColor;
                    Button9BackColor = paletteForm.Button9BackColor;
                    Button9ForeColor = paletteForm.Button9ForeColor;
                    Button10BackColor = paletteForm.Button10BackColor;
                    Button10ForeColor = paletteForm.Button10ForeColor;
                    Button11BackColor = paletteForm.Button11BackColor;
                    Button11ForeColor = paletteForm.Button11ForeColor;
                    Button12BackColor = paletteForm.Button12BackColor;
                    Button12ForeColor = paletteForm.Button12ForeColor;
                    Button13BackColor = paletteForm.Button13BackColor;
                    Button13ForeColor = paletteForm.Button13ForeColor;
                    Button14BackColor = paletteForm.Button14BackColor;
                    Button14ForeColor = paletteForm.Button14ForeColor;
                    Button15BackColor = paletteForm.Button15BackColor;
                    Button15ForeColor = paletteForm.Button15ForeColor;
                    Button16BackColor = paletteForm.Button16BackColor;
                    Button16ForeColor = paletteForm.Button16ForeColor;

                    CommandTextBoxBackColor = paletteForm.CommandTextBoxBackColor;
                    CommandTextBoxForeColor = paletteForm.CommandTextBoxForeColor;
                    DescriptionTextBoxBackColor = paletteForm.DescriptionTextBoxBackColor;
                    DescriptionTextBoxForeColor = paletteForm.DescriptionTextBoxForeColor;
                    TextBoxLabelBackColor = paletteForm.TextBoxLabelBackColor;
                    TextBoxLabelForeColor = paletteForm.TextBoxLabelForeColor;
                    PlusMinusEditButtonBackColor = paletteForm.PlusMinusEditButtonBackColor;
                    PlusMinusEditButtonForeColor = paletteForm.PlusMinusEditButtonForeColor;
                    PageNumberLabelBackColor = paletteForm.PageNumberLabelBackColor;
                    PageNumberLabelForeColor = paletteForm.PageNumberLabelForeColor;
                    PageDescriptionTextBoxBackColor = paletteForm.PageDescriptionTextBoxBackColor;
                    PageDescriptionTextBoxForeColor = paletteForm.PageDescriptionTextBoxForeColor;
                    FormBackColor = paletteForm.FormBackColor;

                    UpdateControlColors();
                }
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void UpdateControlColors()
        {
            this.BackColor = FormBackColor;
            for (int i = 0; i < 16; i++)
            {
                buttons[i].BackColor = new Color[] { Button1BackColor, Button2BackColor, Button3BackColor, Button4BackColor,
                    Button5BackColor, Button6BackColor, Button7BackColor, Button8BackColor,
                    Button9BackColor, Button10BackColor, Button11BackColor, Button12BackColor,
                    Button13BackColor, Button14BackColor, Button15BackColor, Button16BackColor }[i];
                buttons[i].ForeColor = new Color[] { Button1ForeColor, Button2ForeColor, Button3ForeColor, Button4ForeColor,
                    Button5ForeColor, Button6ForeColor, Button7ForeColor, Button8ForeColor,
                    Button9ForeColor, Button10ForeColor, Button11ForeColor, Button12ForeColor,
                    Button13ForeColor, Button14ForeColor, Button15ForeColor, Button16ForeColor }[i];
                commandTextBoxes[i].BackColor = CommandTextBoxBackColor;
                commandTextBoxes[i].ForeColor = CommandTextBoxForeColor;
                descriptionTextBoxes[i].BackColor = DescriptionTextBoxBackColor;
                descriptionTextBoxes[i].ForeColor = DescriptionTextBoxForeColor;
                textBoxLabels[i].BackColor = TextBoxLabelBackColor;
                textBoxLabels[i].ForeColor = TextBoxLabelForeColor;
            }
            minusButton.BackColor = PlusMinusEditButtonBackColor;
            minusButton.ForeColor = PlusMinusEditButtonForeColor;
            plusButton.BackColor = PlusMinusEditButtonBackColor;
            plusButton.ForeColor = PlusMinusEditButtonForeColor;
            editButton.BackColor = PlusMinusEditButtonBackColor;
            editButton.ForeColor = PlusMinusEditButtonForeColor;
            pageNumberLabel.BackColor = PageNumberLabelBackColor;
            pageNumberLabel.ForeColor = PageNumberLabelForeColor;
            pageDescriptionTextBox.BackColor = PageDescriptionTextBoxBackColor;
            pageDescriptionTextBox.ForeColor = PageDescriptionTextBoxForeColor;
        }

        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (currentPage < MaxPages)
            {
                SaveCurrentPageData();
                currentPage++;
                UpdatePage();
            }
        }

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                SaveCurrentPageData();
                currentPage--;
                UpdatePage();
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
            }
            catch
            {
                // Silent fail
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
                    }
                }
            }
            catch
            {
                // Silent fail
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
                        }
                    }
                    int pageDescIndex = MaxPages * 16 + currentPage - 1;
                    if (pageDescIndex < descriptions.Length)
                    {
                        pageDescriptionTextBox.Text = descriptions[pageDescIndex];
                    }
                }
            }
            catch
            {
                // Silent fail
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
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            LayoutControls();
        }

        private void Form_WindowStateChanged(object sender, EventArgs e)
        {
            this.TopMost = (this.WindowState != FormWindowState.Minimized);
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
            }

            for (int i = 0; i < 16; i++)
            {
                textBoxLabels[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing,
                    startY + i * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                );

                commandTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth,
                    startY + i * textBoxHeight
                );
                commandTextBoxes[i].Size = new Size(commandWidth, textBoxHeight);

                descriptionTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth + commandWidth + commandSpacing,
                    startY + i * textBoxHeight
                );
                descriptionTextBoxes[i].Size = new Size(descriptionWidth, textBoxHeight);
            }

            int gridBottom = startY + gridSize * (buttonSize + buttonSpacing) - buttonSpacing;
            minusButton.Location = new Point(startX, gridBottom + 10);
            plusButton.Location = new Point(startX + 40, gridBottom + 10);
            editButton.Location = new Point(startX, gridBottom + 50);

            pageNumberLabel.Location = new Point(startX + 70, gridBottom + 15);
            pageDescriptionTextBox.Location = new Point(startX + 70 + pageNumberLabel.Width + 5, gridBottom + 10);
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

            btn.Enabled = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, args) =>
            {
                try
                {
                    string resolvedCmd = ResolveCommand(cmd);
                    ProcessStartInfo psi;
                    bool isPowerShellScript = resolvedCmd.ToLower().EndsWith(".ps1");
                    bool isSFC = resolvedCmd.ToLower().Contains("sfc") || resolvedCmd.ToLower().Contains("scannow");

                    if (isSFC || isPowerShellScript)
                    {
                        string arguments;
                        if (isPowerShellScript)
                        {
                            arguments = "/K powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"" + resolvedCmd + "\"";
                        }
                        else
                        {
                            arguments = "/K " + resolvedCmd;
                        }

                        psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = arguments,
                            UseShellExecute = true,
                            CreateNoWindow = false,
                            Verb = "runas"
                        };
                    }
                    else
                    {
                        psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/C " + resolvedCmd,
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

                        if (isSFC || isPowerShellScript)
                        {
                            process.WaitForExit();
                            result = "Command executed in external window.";
                        }
                        else
                        {
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
                    if (!(cmd.ToLower().Contains("sfc") || cmd.ToLower().Contains("scannow") || cmd.ToLower().EndsWith(".ps1")))
                    {
                        MessageBox.Show(output, "Output of Command " + ((currentPage - 1) * 16 + resultIndex + 1).ToString(),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (!success)
                {
                    MessageBox.Show(output, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    public class ColorPaletteForm : Form
    {
        private CommandBoard parentForm;
        private Panel colorGrid;
        private TrackBar brightnessBar;
        private Label selectedColorLabel;
        private Button okButton;
        private Button cancelButton;
        private Button[] colorVariableButtons;
        private int selectedVariableIndex = -1;
        private Color selectedColor;

        public Color Button1BackColor, Button1ForeColor, Button2BackColor, Button2ForeColor,
            Button3BackColor, Button3ForeColor, Button4BackColor, Button4ForeColor,
            Button5BackColor, Button5ForeColor, Button6BackColor, Button6ForeColor,
            Button7BackColor, Button7ForeColor, Button8BackColor, Button8ForeColor,
            Button9BackColor, Button9ForeColor, Button10BackColor, Button10ForeColor,
            Button11BackColor, Button11ForeColor, Button12BackColor, Button12ForeColor,
            Button13BackColor, Button13ForeColor, Button14BackColor, Button14ForeColor,
            Button15BackColor, Button15ForeColor, Button16BackColor, Button16ForeColor,
            CommandTextBoxBackColor, CommandTextBoxForeColor, DescriptionTextBoxBackColor, DescriptionTextBoxForeColor,
            TextBoxLabelBackColor, TextBoxLabelForeColor, PlusMinusEditButtonBackColor, PlusMinusEditButtonForeColor,
            PageNumberLabelBackColor, PageNumberLabelForeColor, PageDescriptionTextBoxBackColor, PageDescriptionTextBoxForeColor,
            FormBackColor;

        public ColorPaletteForm(CommandBoard parent)
        {
            parentForm = parent;
            InitializeColors();
            InitializeComponents();
        }

        private void InitializeColors()
        {
            Button1BackColor = parentForm.Button1BackColor;
            Button1ForeColor = parentForm.Button1ForeColor;
            Button2BackColor = parentForm.Button2BackColor;
            Button2ForeColor = parentForm.Button2ForeColor;
            Button3BackColor = parentForm.Button3BackColor;
            Button3ForeColor = parentForm.Button3ForeColor;
            Button4BackColor = parentForm.Button4BackColor;
            Button4ForeColor = parentForm.Button4ForeColor;
            Button5BackColor = parentForm.Button5BackColor;
            Button5ForeColor = parentForm.Button5ForeColor;
            Button6BackColor = parentForm.Button6BackColor;
            Button6ForeColor = parentForm.Button6ForeColor;
            Button7BackColor = parentForm.Button7BackColor;
            Button7ForeColor = parentForm.Button7ForeColor;
            Button8BackColor = parentForm.Button8BackColor;
            Button8ForeColor = parentForm.Button8ForeColor;
            Button9BackColor = parentForm.Button9BackColor;
            Button9ForeColor = parentForm.Button9ForeColor;
            Button10BackColor = parentForm.Button10BackColor;
            Button10ForeColor = parentForm.Button10ForeColor;
            Button11BackColor = parentForm.Button11BackColor;
            Button11ForeColor = parentForm.Button11ForeColor;
            Button12BackColor = parentForm.Button12BackColor;
            Button12ForeColor = parentForm.Button12ForeColor;
            Button13BackColor = parentForm.Button13BackColor;
            Button13ForeColor = parentForm.Button13ForeColor;
            Button14BackColor = parentForm.Button14BackColor;
            Button14ForeColor = parentForm.Button14ForeColor;
            Button15BackColor = parentForm.Button15BackColor;
            Button15ForeColor = parentForm.Button15ForeColor;
            Button16BackColor = parentForm.Button16BackColor;
            Button16ForeColor = parentForm.Button16ForeColor;
            CommandTextBoxBackColor = parentForm.CommandTextBoxBackColor;
            CommandTextBoxForeColor = parentForm.CommandTextBoxForeColor;
            DescriptionTextBoxBackColor = parentForm.DescriptionTextBoxBackColor;
            DescriptionTextBoxForeColor = parentForm.DescriptionTextBoxForeColor;
            TextBoxLabelBackColor = parentForm.TextBoxLabelBackColor;
            TextBoxLabelForeColor = parentForm.TextBoxLabelForeColor;
            PlusMinusEditButtonBackColor = parentForm.PlusMinusEditButtonBackColor;
            PlusMinusEditButtonForeColor = parentForm.PlusMinusEditButtonForeColor;
            PageNumberLabelBackColor = parentForm.PageNumberLabelBackColor;
            PageNumberLabelForeColor = parentForm.PageNumberLabelForeColor;
            PageDescriptionTextBoxBackColor = parentForm.PageDescriptionTextBoxBackColor;
            PageDescriptionTextBoxForeColor = parentForm.PageDescriptionTextBoxForeColor;
            FormBackColor = parentForm.FormBackColor;
        }

        private void InitializeComponents()
        {
            this.Text = "Color Palette Selector";
            this.Size = new Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            string[] variableNames = new string[]
            {
                "Button1BackColor", "Button1ForeColor", "Button2BackColor", "Button2ForeColor",
                "Button3BackColor", "Button3ForeColor", "Button4BackColor", "Button4ForeColor",
                "Button5BackColor", "Button5ForeColor", "Button6BackColor", "Button6ForeColor",
                "Button7BackColor", "Button7ForeColor", "Button8BackColor", "Button8ForeColor",
                "Button9BackColor", "Button9ForeColor", "Button10BackColor", "Button10ForeColor",
                "Button11BackColor", "Button11ForeColor", "Button12BackColor", "Button12ForeColor",
                "Button13BackColor", "Button13ForeColor", "Button14BackColor", "Button14ForeColor",
                "Button15BackColor", "Button15ForeColor", "Button16BackColor", "Button16ForeColor",
                "CommandTextBoxBackColor", "CommandTextBoxForeColor",
                "DescriptionTextBoxBackColor", "DescriptionTextBoxForeColor",
                "TextBoxLabelBackColor", "TextBoxLabelForeColor",
                "PlusMinusEditButtonBackColor", "PlusMinusEditButtonForeColor",
                "PageNumberLabelBackColor", "PageNumberLabelForeColor",
                "PageDescriptionTextBoxBackColor", "PageDescriptionTextBoxForeColor",
                "FormBackColor"
            };
            colorVariableButtons = new Button[variableNames.Length];
            Panel leftPanel = new Panel { Location = new Point(10, 10), Size = new Size(150, 420), AutoScroll = true };
            for (int i = 0; i < variableNames.Length; i++)
            {
                colorVariableButtons[i] = new Button
                {
                    Text = variableNames[i],
                    Size = new Size(130, 30),
                    Location = new Point(10, 10 + i * 35),
                    Tag = i
                };
                colorVariableButtons[i].Click += VariableButton_Click;
                leftPanel.Controls.Add(colorVariableButtons[i]);
            }
            this.Controls.Add(leftPanel);

            colorGrid = new Panel { Location = new Point(170, 10), Size = new Size(240, 240) };
            int gridRows = 8, gridCols = 8;
            for (int row = 0; row < gridRows; row++)
            {
                for (int col = 0; col < gridCols; col++)
                {
                    float hue = (col / (float)gridCols) * 360f;
                    float saturation = 1f;
                    float lightness = (row / (float)gridRows) * 0.8f + 0.2f;
                    Color color = HSLToRGB(hue, saturation, lightness);
                    Panel swatch = new Panel
                    {
                        Size = new Size(30, 30),
                        Location = new Point(col * 30, row * 30),
                        BackColor = color,
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = color
                    };
                    swatch.Click += ColorSwatch_Click;
                    colorGrid.Controls.Add(swatch);
                }
            }
            this.Controls.Add(colorGrid);

            brightnessBar = new TrackBar
            {
                Location = new Point(170, 260),
                Size = new Size(240, 45),
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10
            };
            brightnessBar.Scroll += BrightnessBar_Scroll;
            this.Controls.Add(brightnessBar);

            selectedColorLabel = new Label
            {
                Location = new Point(170, 310),
                Size = new Size(100, 30),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            this.Controls.Add(selectedColorLabel);

            okButton = new Button
            {
                Text = "OK",
                Location = new Point(170, 350),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            okButton.Click += (s, e) => this.Close();
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(255, 350),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            cancelButton.Click += (s, e) => this.Close();
            this.Controls.Add(cancelButton);
        }

        private void VariableButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            selectedVariableIndex = (int)btn.Tag;
        }

        private void ColorSwatch_Click(object sender, EventArgs e)
        {
            if (selectedVariableIndex == -1)
            {
                MessageBox.Show("Please select a color variable first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Panel swatch = (Panel)sender;
            selectedColor = (Color)swatch.Tag;
            UpdateSelectedColor();
            AssignSelectedColor();
        }

        private void BrightnessBar_Scroll(object sender, EventArgs e)
        {
            if (selectedVariableIndex != -1)
            {
                UpdateSelectedColor();
                AssignSelectedColor();
            }
        }

        private void UpdateSelectedColor()
        {
            float brightness = brightnessBar.Value / 100f;
            int r = (int)(selectedColor.R * brightness);
            int g = (int)(selectedColor.G * brightness);
            int b = (int)(selectedColor.B * brightness);
            Color adjustedColor = Color.FromArgb(r, g, b);
            selectedColorLabel.BackColor = adjustedColor;
        }

        private void AssignSelectedColor()
        {
            float brightness = brightnessBar.Value / 100f;
            int r = (int)(selectedColor.R * brightness);
            int g = (int)(selectedColor.G * brightness);
            int b = (int)(selectedColor.B * brightness);
            Color adjustedColor = Color.FromArgb(r, g, b);

            switch (selectedVariableIndex)
            {
                case 0: Button1BackColor = adjustedColor; break;
                case 1: Button1ForeColor = adjustedColor; break;
                case 2: Button2BackColor = adjustedColor; break;
                case 3: Button2ForeColor = adjustedColor; break;
                case 4: Button3BackColor = adjustedColor; break;
                case 5: Button3ForeColor = adjustedColor; break;
                case 6: Button4BackColor = adjustedColor; break;
                case 7: Button4ForeColor = adjustedColor; break;
                case 8: Button5BackColor = adjustedColor; break;
                case 9: Button5ForeColor = adjustedColor; break;
                case 10: Button6BackColor = adjustedColor; break;
                case 11: Button6ForeColor = adjustedColor; break;
                case 12: Button7BackColor = adjustedColor; break;
                case 13: Button7ForeColor = adjustedColor; break;
                case 14: Button8BackColor = adjustedColor; break;
                case 15: Button8ForeColor = adjustedColor; break;
                case 16: Button9BackColor = adjustedColor; break;
                case 17: Button9ForeColor = adjustedColor; break;
                case 18: Button10BackColor = adjustedColor; break;
                case 19: Button10ForeColor = adjustedColor; break;
                case 20: Button11BackColor = adjustedColor; break;
                case 21: Button11ForeColor = adjustedColor; break;
                case 22: Button12BackColor = adjustedColor; break;
                case 23: Button12ForeColor = adjustedColor; break;
                case 24: Button13BackColor = adjustedColor; break;
                case 25: Button13ForeColor = adjustedColor; break;
                case 26: Button14BackColor = adjustedColor; break;
                case 27: Button14ForeColor = adjustedColor; break;
                case 28: Button15BackColor = adjustedColor; break;
                case 29: Button15ForeColor = adjustedColor; break;
                case 30: Button16BackColor = adjustedColor; break;
                case 31: Button16ForeColor = adjustedColor; break;
                case 32: CommandTextBoxBackColor = adjustedColor; break;
                case 33: CommandTextBoxForeColor = adjustedColor; break;
                case 34: DescriptionTextBoxBackColor = adjustedColor; break;
                case 35: DescriptionTextBoxForeColor = adjustedColor; break;
                case 36: TextBoxLabelBackColor = adjustedColor; break;
                case 37: TextBoxLabelForeColor = adjustedColor; break;
                case 38: PlusMinusEditButtonBackColor = adjustedColor; break;
                case 39: PlusMinusEditButtonForeColor = adjustedColor; break;
                case 40: PageNumberLabelBackColor = adjustedColor; break;
                case 41: PageNumberLabelForeColor = adjustedColor; break;
                case 42: PageDescriptionTextBoxBackColor = adjustedColor; break;
                case 43: PageDescriptionTextBoxForeColor = adjustedColor; break;
                case 44: FormBackColor = adjustedColor; break;
            }
        }

        private Color HSLToRGB(float h, float s, float l)
        {
            float r, g, b;
            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
                float p = 2f * l - q;
                r = HueToRGB(p, q, h / 360f + 1f / 3f);
                g = HueToRGB(p, q, h / 360f);
                b = HueToRGB(p, q, h / 360f - 1f / 3f);
            }
            return Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
        }

        private float HueToRGB(float p, float q, float t)
        {
            if (t < 0f) t += 1f;
            if (t > 1f) t -= 1f;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }
    }
}