namespace ChessScoreboard;

using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class MainForm : Form
{
    private readonly PrivateFontCollection _fontCollection = new PrivateFontCollection();

    private Label _labelWinsHeading = null!;
    private Label _labelLossesHeading = null!;
    private Label _labelDrawsHeading = null!;

    private Label _labelWins = null!;
    private Label _labelLosses = null!;
    private Label _labelDraws = null!;

    private Button _buttonWinPlus = null!;
    private Button _buttonWinMinus = null!;

    private Button _buttonLossPlus = null!;
    private Button _buttonLossMinus = null!;

    private Button _buttonDrawPlus = null!;
    private Button _buttonDrawMinus = null!;

    private Button _buttonSave = null!;

    private int _wins;
    private int _losses;
    private int _draws;

    private const string SaveFile = "scores.txt";

    public MainForm()
    {
        LoadEmbeddedFont();
        InitializeUI();
        LoadSavedScores();
    }

    // -------------------------------------------------------------
    // LOAD EMBEDDED FONT
    // -------------------------------------------------------------
    private void LoadEmbeddedFont()
    {
        using Stream? fontStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("ChessScoreboard.font.ttf");

        if (fontStream == null)
        {
            MessageBox.Show("Failed to load font.ttf.");
            return;
        }

        byte[] fontData = new byte[fontStream.Length];
        int read = fontStream.Read(fontData, 0, fontData.Length);
        if (read != fontData.Length)
        {
            MessageBox.Show("Error reading font file.");
            return;
        }

        IntPtr mem = Marshal.AllocCoTaskMem(fontData.Length);
        Marshal.Copy(fontData, 0, mem, fontData.Length);
        _fontCollection.AddMemoryFont(mem, fontData.Length);
        Marshal.FreeCoTaskMem(mem);
    }

    // -------------------------------------------------------------
    // UI SETUP (FIXED 800x800)
    // -------------------------------------------------------------
    private void InitializeUI()
    {
        Text = "Chess Scoreboard";
        Size = new Size(800, 800);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        BackColor = Color.FromArgb(20, 20, 20);
        MaximizeBox = false;
        MinimizeBox = false;

        var fontLarge = new Font(_fontCollection.Families[0], 90, FontStyle.Regular);
        var fontHeading = new Font("Segoe UI", 28, FontStyle.Bold);

        // -------------------------------------------------------------
        // HEADINGS (centered above each column)
        // -------------------------------------------------------------
        _labelWinsHeading = CreateHeading("WINS", 75, 120);
        _labelLossesHeading = CreateHeading("LOSSES", 292, 120);
        _labelDrawsHeading = CreateHeading("DRAWS", 517, 120);

        Controls.Add(_labelWinsHeading);
        Controls.Add(_labelLossesHeading);
        Controls.Add(_labelDrawsHeading);

        // -------------------------------------------------------------
        // SCORE NUMBERS (aligned directly under headings)
        // -------------------------------------------------------------
        _labelWins = CreateScoreLabel(fontLarge, 80, 150);
        Controls.Add(_labelWins);

        _labelLosses = CreateScoreLabel(fontLarge, 300, 150);
        Controls.Add(_labelLosses);

        _labelDraws = CreateScoreLabel(fontLarge, 520, 150);
        Controls.Add(_labelDraws);

        // -------------------------------------------------------------
        // + AND - BUTTONS (aligned under each number)
        // -------------------------------------------------------------
        _buttonWinPlus = CreateButton("+", 80, 310);
        _buttonWinPlus.Click += (_, _) => Adjust(ref _wins, _labelWins, +1);
        Controls.Add(_buttonWinPlus);

        _buttonWinMinus = CreateButton("-", 160, 310);
        _buttonWinMinus.Click += (_, _) => Adjust(ref _wins, _labelWins, -1);
        Controls.Add(_buttonWinMinus);

        _buttonLossPlus = CreateButton("+", 300, 310);
        _buttonLossPlus.Click += (_, _) => Adjust(ref _losses, _labelLosses, +1);
        Controls.Add(_buttonLossPlus);

        _buttonLossMinus = CreateButton("-", 380, 310);
        _buttonLossMinus.Click += (_, _) => Adjust(ref _losses, _labelLosses, -1);
        Controls.Add(_buttonLossMinus);

        _buttonDrawPlus = CreateButton("+", 520, 310);
        _buttonDrawPlus.Click += (_, _) => Adjust(ref _draws, _labelDraws, +1);
        Controls.Add(_buttonDrawPlus);

        _buttonDrawMinus = CreateButton("-", 600, 310);
        _buttonDrawMinus.Click += (_, _) => Adjust(ref _draws, _labelDraws, -1);
        Controls.Add(_buttonDrawMinus);

        // -------------------------------------------------------------
        // SAVE BUTTON
        // -------------------------------------------------------------
        _buttonSave = new Button
        {
            Text = "Save Scores",
            Width = 200,
            Height = 40,
            Left = (800 - 200) / 2,
            Top = 700,
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _buttonSave.Click += SaveScores;
        Controls.Add(_buttonSave);
    }

    // Utility: headings
    private Label CreateHeading(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Color.White,
            Width = 200,
            Height = 50,
            Left = left,
            Top = top,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(20, 20, 20)
        };
    }

    // Utility: score numbers
    private Label CreateScoreLabel(Font font, int left, int top)
    {
        return new Label
        {
            Text = "0",
            Font = font,
            ForeColor = Color.FromArgb(255, 160, 215),
            BackColor = Color.FromArgb(20, 20, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Width = 200,
            Height = 150,
            Left = left,
            Top = top
        };
    }

    // Utility: + and - buttons
    private Button CreateButton(string text, int left, int top)
    {
        return new Button
        {
            Text = text,
            Width = 60,
            Height = 60,
            Left = left,
            Top = top,
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
    }

    // -------------------------------------------------------------
    // SCORE ADJUSTMENT
    // -------------------------------------------------------------
    private void Adjust(ref int value, Label label, int delta)
    {
        value += delta;
        if (value < 0) value = 0;
        label.Text = value.ToString();
    }

    // -------------------------------------------------------------
    // LOAD SAVED SCORES
    // -------------------------------------------------------------
    private void LoadSavedScores()
    {
        if (!File.Exists(SaveFile)) return;

        foreach (string line in File.ReadAllLines(SaveFile))
        {
            var parts = line.Split('=');
            if (parts.Length != 2) continue;

            if (parts[0] == "wins" && int.TryParse(parts[1], out int w))
                _wins = w;

            if (parts[0] == "losses" && int.TryParse(parts[1], out int l))
                _losses = l;

            if (parts[0] == "draws" && int.TryParse(parts[1], out int d))
                _draws = d;
        }

        _labelWins.Text = _wins.ToString();
        _labelLosses.Text = _losses.ToString();
        _labelDraws.Text = _draws.ToString();
    }

    // -------------------------------------------------------------
    // SAVE SCORES
    // -------------------------------------------------------------
    private void SaveScores(object? sender, EventArgs e)
    {
        File.WriteAllLines(SaveFile, new[]
        {
            $"wins={_wins}",
            $"losses={_losses}",
            $"draws={_draws}"
        });
    }
}
