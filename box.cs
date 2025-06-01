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
        private TextBox[] commandTextBoxes = new TextBox[16];
        private TextBox[] descriptionTextBoxes = new TextBox[16];
        private Label[] textBoxLabels = new Label[16];
        private readonly string commandsFile = "commands.txt";
        private readonly string descriptionsFile = "descriptions.txt";
        private readonly string resetFlagFile = "reset.flag";
        private readonly string debugLogFile = "debug.log";

        public CommandBoard()
        {
            this.Text = "Command Board";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.Black;

            InitializeControls();
            LoadCommands();
            LoadDescriptions();
            this.Resize += new EventHandler(Form_Resize);
            this.FormClosing += new FormClosingEventHandler(Form_Closing);
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

        private void InitializeControls()
        {
            string defaultCommand = "ipconfig /all";

            for (int i = 0; i < 16; i++)
            {
                buttons[i] = new Button
                {
                    Text = (i + 1).ToString(),
                    Tag = i,
                    FlatStyle = FlatStyle.Standard,
                    BackColor = i < 4 ? Color.FromArgb(140, 10, 13) : Color.FromArgb(22, 181, 4), // #8C0A0D for 1-4, #16B504 for 5-16
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
                    ForeColor = Color.FromArgb(22, 181, 4) // #16B504 green
                };
                this.Controls.Add(commandTextBoxes[i]);
                LogDebug("Created command TextBox " + (i + 1).ToString());

                descriptionTextBoxes[i] = new TextBox
                {
                    Text = "Description " + (i + 1).ToString(),
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
            }
            catch (Exception ex)
            {
                LogDebug("Error saving commands/descriptions: " + ex.Message);
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
            int commandWidth = 200;
            int descriptionWidth = 200;
            int textBoxHeight = 30;
            int textBoxSpacing = 0;
            int labelWidth = 30;
            int commandSpacing = 10;

            int totalGridWidth = gridSize * buttonSize + (gridSize - 1) * buttonSpacing;
            int totalTextBoxHeight = 16 * textBoxHeight;

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
                LogDebug("Positioned button " + (i + 1).ToString() + " at (" + buttons[i].Location.X + "," + buttons[i].Location.Y + ")");
            }

            for (int i = 0; i < 16; i++)
            {
                textBoxLabels[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing,
                    startY + i * textBoxHeight + (textBoxHeight - textBoxLabels[i].Height) / 2
                );
                LogDebug("Positioned TextBox label " + (i + 1).ToString() + " at (" + textBoxLabels[i].Location.X + "," + textBoxLabels[i].Location.Y + ")");

                commandTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth,
                    startY + i * textBoxHeight
                );
                commandTextBoxes[i].Size = new Size(commandWidth, textBoxHeight);
                LogDebug("Positioned command TextBox " + (i + 1).ToString() + " at (" + commandTextBoxes[i].Location.X + "," + commandTextBoxes[i].Location.Y + ")");

                descriptionTextBoxes[i].Location = new Point(
                    startX + totalGridWidth + buttonSpacing + labelWidth + commandWidth + commandSpacing,
                    startY + i * textBoxHeight
                );
                descriptionTextBoxes[i].Size = new Size(descriptionWidth, textBoxHeight);
                LogDebug("Positioned description TextBox " + (i + 1).ToString() + " at (" + descriptionTextBoxes[i].Location.X + "," + descriptionTextBoxes[i].Location.Y + ")");
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
