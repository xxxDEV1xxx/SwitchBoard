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
        private readonly string colorsFile = "colors.txt";
        private readonly string resetFlagFile = "reset.flag";
        private IntPtr keyboardHookId = IntPtr.Zero;
        private LowLevelKeyboardProc keyboardProc;
        private int currentPage = 1;
        private const int MaxPages = 5;

        public Color Button1BackColor = Color.FromArgb(195, 25, 21);
        public Color Button1ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button2BackColor = Color.FromArgb(11, 16, 150);
        public Color Button2ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button3BackColor = Color.FromArgb(11, 16, 150);
        public Color Button3ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button4BackColor = Color.FromArgb(11, 16, 150);
        public Color Button4ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button5BackColor = Color.FromArgb(11, 16, 150);
        public Color Button5ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button6BackColor = Color.FromArgb(11, 16, 150);
        public Color Button6ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button7BackColor = Color.FromArgb(11, 16, 150);
        public Color Button7ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button8BackColor = Color.FromArgb(11, 16, 150);
        public Color Button8ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button9BackColor = Color.FromArgb(11, 16, 150);
        public Color Button9ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button10BackColor = Color.FromArgb(11, 16, 150);
        public Color Button10ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button11BackColor = Color.FromArgb(11, 16, 150);
        public Color Button11ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button12BackColor = Color.FromArgb(11, 16, 150);
        public Color Button12ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button13BackColor = Color.FromArgb(11, 16, 150);
        public Color Button13ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button14BackColor = Color.FromArgb(11, 16, 150);
        public Color Button14ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button15BackColor = Color.FromArgb(11, 16, 150);
        public Color Button15ForeColor = Color.FromArgb(255, 255, 255);
        public Color Button16BackColor = Color.FromArgb(11, 16, 150);
        public Color Button16ForeColor = Color.FromArgb(255, 255, 255);
        public Color CommandTextBoxBackColor = Color.Black;
        public Color CommandTextBoxForeColor = Color.FromArgb(22, 181, 4);
        public Color DescriptionTextBoxBackColor = Color.Black;
        public Color DescriptionTextBoxForeColor = Color.FromArgb(22, 181, 4);
        public Color TextBoxLabelBackColor = Color.Black;
        public Color TextBoxLabelForeColor = Color.FromArgb(22, 181, 4);
        public Color PlusMinusEditButtonBackColor = Color.Black;
        public Color PlusMinusEditButtonForeColor = Color.FromArgb(22, 181, 4);
        public Color PageNumberLabelBackColor = Color.Black;
        public Color PageNumberLabelForeColor = Color.FromArgb(22, 181, 4);
        public Color PageDescriptionTextBoxBackColor = Color.Black;
        public Color PageDescriptionTextBoxForeColor = Color.FromArgb(22, 181, 4);
        public Color FormBackColor = Color.Black;
        public Color ControlBorderColor = Color.White;

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

            LoadColors();
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

        private void LoadColors()
        {
            try
            {
                if (File.Exists(colorsFile))
                {
                    string[] colorLines = File.ReadAllLines(colorsFile);
                    foreach (string line in colorLines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length != 2) continue;
                        string[] rgb = parts[1].Split(',');
                        if (rgb.Length != 3) continue;
                        int r, g, b;
                        if (!int.TryParse(rgb[0], out r) || !int.TryParse(rgb[1], out g) || !int.TryParse(rgb[2], out b)) continue;
                        Color color = Color.FromArgb(r, g, b);
                        switch (parts[0].Trim())
                        {
                            case "Button1BackColor": Button1BackColor = color; break;
                            case "Button1ForeColor": Button1ForeColor = color; break;
                            case "Button2BackColor": Button2BackColor = color; break;
                            case "Button2ForeColor": Button2ForeColor = color; break;
                            case "Button3BackColor": Button3BackColor = color; break;
                            case "Button3ForeColor": Button3ForeColor = color; break;
                            case "Button4BackColor": Button4BackColor = color; break;
                            case "Button4ForeColor": Button4ForeColor = color; break;
                            case "Button5BackColor": Button5BackColor = color; break;
                            case "Button5ForeColor": Button5ForeColor = color; break;
                            case "Button6BackColor": Button6BackColor = color; break;
                            case "Button6ForeColor": Button6ForeColor = color; break;
                            case "Button7BackColor": Button7BackColor = color; break;
                            case "Button7ForeColor": Button7ForeColor = color; break;
                            case "Button8BackColor": Button8BackColor = color; break;
                            case "Button8ForeColor": Button8ForeColor = color; break;
                            case "Button9BackColor": Button9BackColor = color; break;
                            case "Button9ForeColor": Button9ForeColor = color; break;
                            case "Button10BackColor": Button10BackColor = color; break;
                            case "Button10ForeColor": Button10ForeColor = color; break;
                            case "Button11BackColor": Button11BackColor = color; break;
                            case "Button11ForeColor": Button11ForeColor = color; break;
                            case "Button12BackColor": Button12BackColor = color; break;
                            case "Button12ForeColor": Button12ForeColor = color; break;
                            case "Button13BackColor": Button13BackColor = color; break;
                            case "Button13ForeColor": Button13ForeColor = color; break;
                            case "Button14BackColor": Button14BackColor = color; break;
                            case "Button14ForeColor": Button14ForeColor = color; break;
                            case "Button15BackColor": Button15BackColor = color; break;
                            case "Button15ForeColor": Button15ForeColor = color; break;
                            case "Button16BackColor": Button16BackColor = color; break;
                            case "Button16ForeColor": Button16ForeColor = color; break;
                            case "CommandTextBoxBackColor": CommandTextBoxBackColor = color; break;
                            case "CommandTextBoxForeColor": CommandTextBoxForeColor = color; break;
                            case "DescriptionTextBoxBackColor": DescriptionTextBoxBackColor = color; break;
                            case "DescriptionTextBoxForeColor": DescriptionTextBoxForeColor = color; break;
                            case "TextBoxLabelBackColor": TextBoxLabelBackColor = color; break;
                            case "TextBoxLabelForeColor": TextBoxLabelForeColor = color; break;
                            case "PlusMinusEditButtonBackColor": PlusMinusEditButtonBackColor = color; break;
                            case "PlusMinusEditButtonForeColor": PlusMinusEditButtonForeColor = color; break;
                            case "PageNumberLabelBackColor": PageNumberLabelBackColor = color; break;
                            case "PageNumberLabelForeColor": PageNumberLabelForeColor = color; break;
                            case "PageDescriptionTextBoxBackColor": PageDescriptionTextBoxBackColor = color; break;
                            case "PageDescriptionTextBoxForeColor": PageDescriptionTextBoxForeColor = color; break;
                            case "FormBackColor": FormBackColor = color; break;
                            case "ControlBorderColor": ControlBorderColor = color; break;
                        }
                    }
                    UpdateControlColors();
                }
            }
            catch
            {
                // Silent fail
            }
        }

private void SaveColors()
{
    try
    {
        string[] colorLines = new string[]
        {
            "Button1BackColor=" + Button1BackColor.R + "," + Button1BackColor.G + "," + Button1BackColor.B,
            "Button1ForeColor=" + Button1ForeColor.R + "," + Button1ForeColor.G + "," + Button1ForeColor.B,
            "Button2BackColor=" + Button2BackColor.R + "," + Button2BackColor.G + "," + Button2BackColor.B,
            "Button2ForeColor=" + Button2ForeColor.R + "," + Button2ForeColor.G + "," + Button2ForeColor.B,
            "Button3BackColor=" + Button3BackColor.R + "," + Button3BackColor.G + "," + Button3BackColor.B,
            "Button3ForeColor=" + Button3ForeColor.R + "," + Button3ForeColor.G + "," + Button3ForeColor.B,
            "Button4BackColor=" + Button4BackColor.R + "," + Button4BackColor.G + "," + Button4BackColor.B,
            "Button4ForeColor=" + Button4ForeColor.R + "," + Button4ForeColor.G + "," + Button4ForeColor.B,
            "Button5BackColor=" + Button5BackColor.R + "," + Button5BackColor.G + "," + Button5BackColor.B,
            "Button5ForeColor=" + Button5ForeColor.R + "," + Button5ForeColor.G + "," + Button5ForeColor.B,
            "Button6BackColor=" + Button6BackColor.R + "," + Button6BackColor.G + "," + Button6BackColor.B,
            "Button6ForeColor=" + Button6ForeColor.R + "," + Button6ForeColor.G + "," + Button6ForeColor.B,
            "Button7BackColor=" + Button7BackColor.R + "," + Button7BackColor.G + "," + Button7BackColor.B,
            "Button7ForeColor=" + Button7ForeColor.R + "," + Button7ForeColor.G + "," + Button7ForeColor.B,
            "Button8BackColor=" + Button8BackColor.R + "," + Button8BackColor.G + "," + Button8BackColor.B,
            "Button8ForeColor=" + Button8ForeColor.R + "," + Button8ForeColor.G + "," + Button8ForeColor.B,
            "Button9BackColor=" + Button9BackColor.R + "," + Button9BackColor.G + "," + Button9BackColor.B,
            "Button9ForeColor=" + Button9ForeColor.R + "," + Button9ForeColor.G + "," + Button9ForeColor.B,
            "Button10BackColor=" + Button10BackColor.R + "," + Button10BackColor.G + "," + Button10BackColor.B,
            "Button10ForeColor=" + Button10ForeColor.R + "," + Button10ForeColor.G + "," + Button10ForeColor.B,
            "Button11BackColor=" + Button11BackColor.R + "," + Button11BackColor.G + "," + Button11BackColor.B,
            "Button11ForeColor=" + Button11ForeColor.R + "," + Button11ForeColor.G + "," + Button11ForeColor.B,
            "Button12BackColor=" + Button12BackColor.R + "," + Button12BackColor.G + "," + Button12BackColor.B,
            "Button12ForeColor=" + Button12ForeColor.R + "," + Button12ForeColor.G + "," + Button12ForeColor.B,
            "Button13BackColor=" + Button13BackColor.R + "," + Button13BackColor.G + "," + Button13BackColor.B,
            "Button13ForeColor=" + Button13ForeColor.R + "," + Button13ForeColor.G + "," + Button13ForeColor.B,
            "Button14BackColor=" + Button14BackColor.R + "," + Button14BackColor.G + "," + Button14BackColor.B,
            "Button14ForeColor=" + Button14ForeColor.R + "," + Button14ForeColor.G + "," + Button14ForeColor.B,
            "Button15BackColor=" + Button15BackColor.R + "," + Button15BackColor.G + "," + Button15BackColor.B,
            "Button15ForeColor=" + Button15ForeColor.R + "," + Button15ForeColor.G + "," + Button15ForeColor.B,
            "Button16BackColor=" + Button16BackColor.R + "," + Button16BackColor.G + "," + Button16BackColor.B,
            "Button16ForeColor=" + Button16ForeColor.R + "," + Button16ForeColor.G + "," + Button16ForeColor.B,
            "CommandTextBoxBackColor=" + CommandTextBoxBackColor.R + "," + CommandTextBoxBackColor.G + "," + CommandTextBoxBackColor.B,
            "CommandTextBoxForeColor=" + CommandTextBoxForeColor.R + "," + CommandTextBoxForeColor.G + "," + CommandTextBoxForeColor.B,
            "DescriptionTextBoxBackColor=" + DescriptionTextBoxBackColor.R + "," + DescriptionTextBoxBackColor.G + "," + DescriptionTextBoxBackColor.B,
            "DescriptionTextBoxForeColor=" + DescriptionTextBoxForeColor.R + "," + DescriptionTextBoxForeColor.G + "," + DescriptionTextBoxForeColor.B,
            "TextBoxLabelBackColor=" + TextBoxLabelBackColor.R + "," + TextBoxLabelBackColor.G + "," + TextBoxLabelBackColor.B,
            "TextBoxLabelForeColor=" + TextBoxLabelForeColor.R + "," + TextBoxLabelForeColor.G + "," + TextBoxLabelForeColor.B,
            "PlusMinusEditButtonBackColor=" + PlusMinusEditButtonBackColor.R + "," + PlusMinusEditButtonBackColor.G + "," + PlusMinusEditButtonBackColor.B,
            "PlusMinusEditButtonForeColor=" + PlusMinusEditButtonForeColor.R + "," + PlusMinusEditButtonForeColor.G + "," + PlusMinusEditButtonForeColor.B,
            "PageNumberLabelBackColor=" + PageNumberLabelBackColor.R + "," + PageNumberLabelBackColor.G + "," + PageNumberLabelBackColor.B,
            "PageNumberLabelForeColor=" + PageNumberLabelForeColor.R + "," + PageNumberLabelForeColor.G + "," + PageNumberLabelForeColor.B,
            "PageDescriptionTextBoxBackColor=" + PageDescriptionTextBoxBackColor.R + "," + PageDescriptionTextBoxBackColor.G + "," + PageDescriptionTextBoxBackColor.B,
            "PageDescriptionTextBoxForeColor=" + PageDescriptionTextBoxForeColor.R + "," + PageDescriptionTextBoxForeColor.G + "," + PageDescriptionTextBoxForeColor.B,
            "FormBackColor=" + FormBackColor.R + "," + FormBackColor.G + "," + FormBackColor.B,
            "ControlBorderColor=" + ControlBorderColor.R + "," + ControlBorderColor.G + "," + ControlBorderColor.B
        };
        File.WriteAllLines(colorsFile, colorLines);
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
                            this.Invoke(new MethodInvoker(delegate
                            {
                                buttons[index].PerformClick();
                            }));
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
                int buttonNumber = (currentPage - 1) * 16 + i + 1;
                buttons[i] = new Button
                {
                    Text = Convert.ToString(buttonNumber), // Default button text is button number
                    Tag = i,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = buttonBackColors[i],
                    ForeColor = buttonForeColors[i],
                    Font = new Font("Calibri", 11, FontStyle.Bold | FontStyle.Italic)
                };
                buttons[i].FlatAppearance.BorderSize = 2;
                buttons[i].FlatAppearance.BorderColor = ControlBorderColor;
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
                    ForeColor = CommandTextBoxForeColor,
                    BorderStyle = BorderStyle.FixedSingle
                };
                commandTextBoxes[i].Paint += new PaintEventHandler(CommandTextBox_Paint);
                this.Controls.Add(commandTextBoxes[i]);

                descriptionTextBoxes[i] = new TextBox
                {
                    Text = Convert.ToString(buttonNumber), // Default to button number
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = DescriptionTextBoxBackColor,
                    ForeColor = DescriptionTextBoxForeColor,
                    BorderStyle = BorderStyle.FixedSingle
                };
                int index = i; // Capture index for event handler
                descriptionTextBoxes[i].TextChanged += new EventHandler(delegate(object s, EventArgs e)
                {
                    UpdateButtonTextFromDescription(index); // Update button text when description changes
                });
                descriptionTextBoxes[i].Paint += new PaintEventHandler(DescriptionTextBox_Paint);
                this.Controls.Add(descriptionTextBoxes[i]);

                textBoxLabels[i] = new Label
                {
                    Text = Convert.ToString(buttonNumber),
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
                FlatStyle = FlatStyle.Flat,
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
                FlatStyle = FlatStyle.Flat,
                BackColor = PlusMinusEditButtonBackColor,
                ForeColor = PlusMinusEditButtonForeColor,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            plusButton.Click += new EventHandler(PlusButton_Click);
            this.Controls.Add(plusButton);

            editButton = new Button
            {
                Text = "Edit",
                Size = new Size(70, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = PlusMinusEditButtonBackColor,
                ForeColor = PlusMinusEditButtonForeColor,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            editButton.Click += new EventHandler(EditButton_Click);
            this.Controls.Add(editButton);

            pageNumberLabel = new Label
            {
                Text = Convert.ToString(currentPage),
                Font = new Font("Arial", 12, FontStyle.Bold),
                AutoSize = true,
                BackColor = PageNumberLabelBackColor,
                ForeColor = PageNumberLabelForeColor
            };
            this.Controls.Add(pageNumberLabel);

            pageDescriptionTextBox = new TextBox
            {
                Text = "Page " + Convert.ToString(currentPage) + " Description",
                Font = new Font("Arial", 10),
                Width = 200,
                BackColor = PageDescriptionTextBoxBackColor,
                ForeColor = PageDescriptionTextBoxForeColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            pageDescriptionTextBox.Paint += new PaintEventHandler(PageDescriptionTextBox_Paint);
            this.Controls.Add(pageDescriptionTextBox);
        }

        private void CommandTextBox_Paint(object sender, PaintEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            using (Pen pen = new Pen(ControlBorderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, tb.Width - 1, tb.Height - 1);
            }
        }

        private void DescriptionTextBox_Paint(object sender, PaintEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            using (Pen pen = new Pen(ControlBorderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, tb.Width - 1, tb.Height - 1);
            }
        }

        private void PageDescriptionTextBox_Paint(object sender, PaintEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            using (Pen pen = new Pen(ControlBorderColor, 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, tb.Width - 1, tb.Height - 1);
            }
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
                    ControlBorderColor = paletteForm.ControlBorderColor;

                    UpdateControlColors();
                    SaveColors();
                    LayoutControls();
                }
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void UpdateControlColors()
        {
            this.BackColor = FormBackColor;
            for (int i = 0; i < 16; i++)
            {
                buttons[i].BackColor = new Color[] {
                    Button1BackColor, Button2BackColor, Button3BackColor, Button4BackColor,
                    Button5BackColor, Button6BackColor, Button7BackColor, Button8BackColor,
                    Button9BackColor, Button10BackColor, Button11BackColor, Button12BackColor,
                    Button13BackColor, Button14BackColor, Button15BackColor, Button16BackColor
                }[i];
                buttons[i].ForeColor = new Color[] {
                    Button1ForeColor, Button2ForeColor, Button3ForeColor, Button4ForeColor,
                    Button5ForeColor, Button6ForeColor, Button7ForeColor, Button8ForeColor,
                    Button9ForeColor, Button10ForeColor, Button11ForeColor, Button12ForeColor,
                    Button13ForeColor, Button14ForeColor, Button15ForeColor, Button16ForeColor
                }[i];
                buttons[i].FlatAppearance.BorderColor = ControlBorderColor;
                commandTextBoxes[i].BackColor = CommandTextBoxBackColor;
                commandTextBoxes[i].ForeColor = CommandTextBoxForeColor;
                commandTextBoxes[i].Invalidate();
                descriptionTextBoxes[i].BackColor = DescriptionTextBoxBackColor;
                descriptionTextBoxes[i].ForeColor = DescriptionTextBoxForeColor;
                descriptionTextBoxes[i].Invalidate();
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
            pageDescriptionTextBox.Invalidate();
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
            pageNumberLabel.Text = Convert.ToString(currentPage);
            pageDescriptionTextBox.Text = "Page " + Convert.ToString(currentPage) + " Description";

            for (int i = 0; i < 16; i++)
            {
                int buttonNumber = (currentPage - 1) * 16 + i + 1;
                textBoxLabels[i].Text = Convert.ToString(buttonNumber);
                commandTextBoxes[i].Text = "ipconfig /all";
                descriptionTextBoxes[i].Text = Convert.ToString(buttonNumber); // Set default description
                UpdateButtonTextFromDescription(i); // Ensure button text matches description
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
                        if (index < descriptions.Length && !string.IsNullOrEmpty(descriptions[index]))
                        {
                            descriptionTextBoxes[i].Text = descriptions[index];
                        }
                        else
                        {
                            descriptionTextBoxes[i].Text = Convert.ToString((currentPage - 1) * 16 + i + 1);
                        }
                        UpdateButtonTextFromDescription(i); // Ensure button text matches description
                    }
                    int pageDescIndex = MaxPages * 16 + currentPage - 1;
                    if (pageDescIndex < descriptions.Length)
                    {
                        pageDescriptionTextBox.Text = descriptions[pageDescIndex];
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        descriptionTextBoxes[i].Text = Convert.ToString((currentPage - 1) * 16 + i + 1);
                        UpdateButtonTextFromDescription(i); // Ensure button text matches description
                    }
                }
            }
            catch
            {
                // Silent fail
                for (int i = 0; i < 16; i++)
                {
                    descriptionTextBoxes[i].Text = Convert.ToString((currentPage - 1) * 16 + i + 1);
                    UpdateButtonTextFromDescription(i); // Ensure button text matches description
                }
            }
        }

        private void UpdateButtonTextFromDescription(int index)
        {
            if (index >= 0 && index < 16)
            {
                string description = descriptionTextBoxes[index].Text;
                buttons[index].Text = string.IsNullOrEmpty(description)
                    ? Convert.ToString((currentPage - 1) * 16 + index + 1)
                    : description;
            }
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            SaveCurrentPageData();
            SaveColors();

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
            int margin = 7;
            int gridSize = 4;
            int buttonSize = 70;
            int buttonSpacing = 10;
            int commandWidth = 200;
            int descriptionWidth = 200;
            int textBoxHeight = 30;
            int labelWidth = 30;
            int commandSpacing = 10;

            int totalGridWidth = gridSize * buttonSize + buttonSpacing;

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
                    startX + totalGridWidth + buttonSpacing + 25,
                    startY + i * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                );

                commandTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth +20,
                    startY + i * textBoxHeight
                );
                commandTextBoxes[i].Size = new Size(commandWidth, textBoxHeight);

                descriptionTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth + commandWidth + commandSpacing + 20,
                    startY + i * textBoxHeight
                );
                descriptionTextBoxes[i].Size = new Size(descriptionWidth, textBoxHeight);
            }

            int gridBottom = startY + gridSize * (buttonSize + buttonSpacing) - buttonSpacing;
            minusButton.Location = new Point(startX, gridBottom + 10);
            plusButton.Location = new Point(startX + 40, gridBottom + 10);
            editButton.Location = new Point(startX, gridBottom + 50);

            pageNumberLabel.Location = new Point(startX + 73, gridBottom + 15);
            pageDescriptionTextBox.Location = new Point(startX + 70 + pageNumberLabel.Width + 5, gridBottom + 10);
            pageDescriptionTextBox.Size = new Size(160, textBoxHeight);
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

            // Run the command asynchronously
            System.Threading.Tasks.Task.Factory.StartNew(delegate
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
                            Verb = isSFC ? "runas" : ""
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
                        if (!(isSFC || isPowerShellScript))
                        {
                            // For non-SFC and non-PowerShell commands, capture output
                            string output = "";
                            string error = "";
                            process.OutputDataReceived += new DataReceivedEventHandler(delegate(object s, DataReceivedEventArgs args)
                            {
                                if (args.Data != null) output += args.Data + Environment.NewLine;
                            });
                            process.ErrorDataReceived += new DataReceivedEventHandler(delegate(object s, DataReceivedEventArgs args)
                            {
                                if (args.Data != null) error += args.Data + Environment.NewLine;
                            });

                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            process.WaitForExit();

                            string result = string.IsNullOrEmpty(error) ? output : "Error: " + error;

                            if (!string.IsNullOrEmpty(result))
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    MessageBox.Show(result, "Output of Command " + Convert.ToString((currentPage - 1) * 16 + index + 1),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }));
                            }
                        }
                        else
                        {
                            // For SFC or PowerShell, just start the process
                            process.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
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

            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            string[] paths = pathEnv != null ? pathEnv.Split(';') : new string[0];
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
        private TextBox redTextBox;
        private TextBox greenTextBox;
        private TextBox blueTextBox;
        private Label redLabel;
        private Label greenLabel;
        private Label blueLabel;
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
            FormBackColor, ControlBorderColor;

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
            ControlBorderColor = parentForm.ControlBorderColor;
        }

        private void InitializeComponents()
        {
            this.Text = "Color Palette Selector";
            this.Size = new Size(650, 500);
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
                "FormBackColor", "ControlBorderColor"
            };
            colorVariableButtons = new Button[variableNames.Length];
            Panel leftPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(150, 400),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            int buttonHeight = 30;
            int buttonSpacing = 35;
            int totalContentHeight = variableNames.Length * buttonSpacing + 10;
            for (int i = 0; i < variableNames.Length; i++)
            {
                colorVariableButtons[i] = new Button
                {
                    Text = variableNames[i],
                    Size = new Size(130, buttonHeight),
                    Location = new Point(10, 10 + i * buttonSpacing),
                    Tag = i
                };
                colorVariableButtons[i].Click += new EventHandler(VariableButton_Click);
                leftPanel.Controls.Add(colorVariableButtons[i]);
            }
            leftPanel.AutoScrollMinSize = new Size(0, totalContentHeight);
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
                    swatch.Click += new EventHandler(ColorSwatch_Click);
                    colorGrid.Controls.Add(swatch);
                }
            }
            this.Controls.Add(colorGrid);

            redLabel = new Label
            {
                Text = "R:",
                Location = new Point(420, 10),
                Size = new Size(30, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(redLabel);

            redTextBox = new TextBox
            {
                Location = new Point(450, 10),
                Size = new Size(50, 20),
                Text = "255"
            };
            redTextBox.TextChanged += new EventHandler(RGBTextBox_TextChanged);
            this.Controls.Add(redTextBox);

            greenLabel = new Label
            {
                Text = "G:",
                Location = new Point(420, 40),
                Size = new Size(30, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(greenLabel);

            greenTextBox = new TextBox
            {
                Location = new Point(450, 40),
                Size = new Size(50, 20),
                Text = "255"
            };
            greenTextBox.TextChanged += new EventHandler(RGBTextBox_TextChanged);
            this.Controls.Add(greenTextBox);

            blueLabel = new Label
            {
                Text = "B:",
                Location = new Point(420, 70),
                Size = new Size(30, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            this.Controls.Add(blueLabel);

            blueTextBox = new TextBox
            {
                Location = new Point(450, 70),
                Size = new Size(50, 20),
                Text = "255"
            };
            blueTextBox.TextChanged += new EventHandler(RGBTextBox_TextChanged);
            this.Controls.Add(blueTextBox);

            brightnessBar = new TrackBar
            {
                Location = new Point(170, 260),
                Size = new Size(240, 45),
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10
            };
            brightnessBar.Scroll += new EventHandler(BrightnessBar_Scroll);
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
            okButton.Click += new EventHandler(OkButton_Click);
            this.Controls.Add(okButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(255, 350),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            cancelButton.Click += new EventHandler(CancelButton_Click);
            this.Controls.Add(cancelButton);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void VariableButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            selectedVariableIndex = (int)btn.Tag;
            if (selectedVariableIndex >= 0)
            {
                Color currentColor = GetColorByIndex(selectedVariableIndex);
                redTextBox.Text = Convert.ToString(currentColor.R);
                greenTextBox.Text = Convert.ToString(currentColor.G);
                blueTextBox.Text = Convert.ToString(currentColor.B);
                selectedColor = currentColor;
                UpdateSelectedColor();
            }
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
            redTextBox.Text = Convert.ToString(selectedColor.R);
            greenTextBox.Text = Convert.ToString(selectedColor.G);
            blueTextBox.Text = Convert.ToString(selectedColor.B);
            UpdateSelectedColor();
            AssignSelectedColor();
        }

        private void RGBTextBox_TextChanged(object sender, EventArgs e)
        {
            if (selectedVariableIndex == -1)
            {
                return;
            }

            int r, g, b;
            bool validR = int.TryParse(redTextBox.Text, out r) && r >= 0 && r <= 255;
            bool validG = int.TryParse(greenTextBox.Text, out g) && g >= 0 && g <= 255;
            bool validB = int.TryParse(blueTextBox.Text, out b) && b >= 0 && b <= 255;

            if (validR && validG && validB)
            {
                selectedColor = Color.FromArgb(r, g, b);
                UpdateSelectedColor();
                AssignSelectedColor();
            }
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
                case 45: ControlBorderColor = adjustedColor; break;
            }
        }

        private Color GetColorByIndex(int index)
        {
            switch (index)
            {
                case 0: return Button1BackColor;
                case 1: return Button1ForeColor;
                case 2: return Button2BackColor;
                case 3: return Button2ForeColor;
                case 4: return Button3BackColor;
                case 5: return Button3ForeColor;
                case 6: return Button4BackColor;
                case 7: return Button4ForeColor;
                case 8: return Button5BackColor;
                case 9: return Button5ForeColor;
                case 10: return Button6BackColor;
                case 11: return Button6ForeColor;
                case 12: return Button7BackColor;
                case 13: return Button7ForeColor;
                case 14: return Button8BackColor;
                case 15: return Button8ForeColor;
                case 16: return Button9BackColor;
                case 17: return Button9ForeColor;
                case 18: return Button10BackColor;
                case 19: return Button10ForeColor;
                case 20: return Button11BackColor;
                case 21: return Button11ForeColor;
                case 22: return Button12BackColor;
                case 23: return Button12ForeColor;
                case 24: return Button13BackColor;
                case 25: return Button13ForeColor;
                case 26: return Button14BackColor;
                case 27: return Button14ForeColor;
                case 28: return Button15BackColor;
                case 29: return Button15ForeColor;
                case 30: return Button16BackColor;
                case 31: return Button16ForeColor;
                case 32: return CommandTextBoxBackColor;
                case 33: return CommandTextBoxForeColor;
                case 34: return DescriptionTextBoxBackColor;
                case 35: return DescriptionTextBoxForeColor;
                case 36: return TextBoxLabelBackColor;
                case 37: return TextBoxLabelForeColor;
                case 38: return PlusMinusEditButtonBackColor;
                case 39: return PlusMinusEditButtonForeColor;
                case 40: return PageNumberLabelBackColor;
                case 41: return PageNumberLabelForeColor;
                case 42: return PageDescriptionTextBoxBackColor;
                case 43: return PageDescriptionTextBoxForeColor;
                case 44: return FormBackColor;
                case 45: return ControlBorderColor;
                default: return Color.Black;
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
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, (int)(r * 255f))),
                Math.Max(0, Math.Min(255, (int)(g * 255f))),
                Math.Max(0, Math.Min(255, (int)(b * 255f))));
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