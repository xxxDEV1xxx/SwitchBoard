using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ButtonCommandBoard
{
    public class CommandBoard : Form
    {
        private Button[] buttons = new Button[16];
        private TextBox[] textBoxes = new TextBox[16];
        private Label[] textBoxLabels = new Label[16];
        private readonly string commandsFile = "commands.txt";

        public CommandBoard()
        {
            this.Text = "Command Board";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.Black; // Changed to black

            InitializeControls();
            LoadCommands();
            this.Resize += new EventHandler(Form_Resize);
            this.FormClosing += new FormClosingEventHandler(Form_Closing);
            LayoutControls();
        }

        private void InitializeControls()
        {
            string defaultCommand = "plink -batch -l root -pwfile C:\\Users\\DEV1\\.ssh\\pw.txt 192.168.1.1 \"setsid tcpdump -i any src host 192.168.1.7 -w /tmp/output.pcap &\"";

            for (int i = 0; i < 16; i++)
            {
                buttons[i] = new Button
                {
                    Text = (i + 1).ToString(),
                    Tag = i,
                    FlatStyle = FlatStyle.Standard, // 3D raised effect
                    BackColor = Color.LightBlue, // Contrast with black
                    ForeColor = Color.Black,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                buttons[i].FlatAppearance.BorderSize = 2;
                buttons[i].FlatAppearance.MouseOverBackColor = Color.Cyan;
                buttons[i].FlatAppearance.MouseDownBackColor = Color.SkyBlue;
                buttons[i].Click += new EventHandler(Button_Click);
                this.Controls.Add(buttons[i]);
                Console.WriteLine("Created button " + (i + 1).ToString());

                textBoxes[i] = new TextBox
                {
                    Text = defaultCommand,
                    Font = new Font("Arial", 10),
                    Width = 200,
                    BackColor = Color.White, // Readable on black
                    ForeColor = Color.Black
                };
                this.Controls.Add(textBoxes[i]);
                Console.WriteLine("Created TextBox " + (i + 1).ToString());

                textBoxLabels[i] = new Label
                {
                    Text = (i + 1).ToString(),
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    AutoSize = true,
                    ForeColor = Color.White // Visible on black
                };
                this.Controls.Add(textBoxLabels[i]);
                Console.WriteLine("Created TextBox label " + (i + 1).ToString());
            }
        }

        private void LoadCommands()
        {
            try
            {
                if (File.Exists(commandsFile))
                {
                    string[] commands = File.ReadAllLines(commandsFile);
                    for (int i = 0; i < Math.Min(commands.Length, 16); i++)
                    {
                        textBoxes[i].Text = commands[i];
                        Console.WriteLine("Loaded command " + (i + 1).ToString() + ": " + commands[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading commands: " + ex.Message);
            }
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            try
            {
                string[] commands = new string[16];
                for (int i = 0; i < 16; i++)
                {
                    commands[i] = textBoxes[i].Text;
                }
                File.WriteAllLines(commandsFile, commands);
                Console.WriteLine("Saved commands to " + commandsFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving commands: " + ex.Message);
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            LayoutControls();
        }

        private void LayoutControls()
        {
            int margin = 20;
            int gridSize = 4;
            int buttonSize = 50;
            int buttonSpacing = 30;
            int textBoxWidth = 200;
            int textBoxHeight = 30;
            int textBoxSpacing = 0;
            int labelWidth = 30;

            int totalGridWidth = gridSize * buttonSize + (gridSize - 1) * buttonSpacing;
            int totalGridHeight = gridSize * buttonSize + (gridSize - 1) * buttonSpacing;
            int totalTextBoxHeight = 16 * textBoxHeight;

            int startX = (this.ClientSize.Width - (totalGridWidth + buttonSpacing + labelWidth + textBoxWidth)) / 2;
            int startY = (this.ClientSize.Height - Math.Max(totalGridHeight, totalTextBoxHeight)) / 2;

            if (startX < margin) startX = margin;
            if (startY < margin) startY = margin;

            for (int i = 0; i < 16; i++)
            {
                int row = i / gridSize;
                int col = i % gridSize;

                buttons[i].Size = new Size(buttonSize, buttonSize);
                buttons[i].Location = new Point(
                    startX + col * (buttonSize + buttonSpacing),
                    startY + row * (buttonSize + buttonSpacing)
                );
                Console.WriteLine("Positioned button " + (i + 1).ToString() + " at (" + buttons[i].Location.X + "," + buttons[i].Location.Y + ")");
            }

            for (int i = 0; i < 16; i++)
            {
                textBoxLabels[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing,
                    startY + i * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                );
                Console.WriteLine("Positioned TextBox label " + (i + 1).ToString() + " at (" + textBoxLabels[i].Location.X + "," + textBoxLabels[i].Location.Y + ")");

                textBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth,
                    startY + i * textBoxHeight
                );
                textBoxes[i].Size = new Size(textBoxWidth, textBoxHeight);
                Console.WriteLine("Positioned TextBox " + (i + 1).ToString() + " at (" + textBoxes[i].Location.X + "," + textBoxes[i].Location.Y + ")");
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int index = (int)btn.Tag;
            string command = textBoxes[index].Text.Trim();

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