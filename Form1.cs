using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class Form1 : Form
    {
        // =============================
        // Minesweeper Game (WinForms)
        // Layers:
        // 1. UI Layer (Form, Buttons)
        // 2. Logic Layer (Game rules)
        // 3. Data Layer (Cell model)
        // =============================

        int rows = 10;
        int cols = 10;
        Cell[,] board;
        Button[,] buttons;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //UI - form
            this.Text = "Minesweeper";
            this.BackColor = Color.FromArgb(30, 30, 30); // dark theme
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            //UI - adding panel
            Panel topPanel = new Panel();
            topPanel.Height = 60;
            topPanel.Dock = DockStyle.Top;
            topPanel.BackColor = Color.FromArgb(45, 45, 45);

            this.Controls.Add(topPanel);

            //UI - adding title label
            Label title = new Label();
            title.Text = "MINESWEEPER";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            title.AutoSize = true;
            title.Left = 10;
            title.Top = 15;

            topPanel.Controls.Add(title);

            //actual logic
            InitializeBoard();
            PlaceMines(15);
            CalculateNumbers();
            CreateButtons();
        }

        // Logic: initialize board data structure
        private void InitializeBoard()
        {
            board = new Cell[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    board[i, j] = new Cell();
                }
            }
        }

        // Logic: randomly place mines
        private void PlaceMines(int mineCount)
        {
            Random rand = new Random();
            int placed = 0;

            while (placed < mineCount)
            {
                int r = rand.Next(rows);
                int c = rand.Next(cols);

                if (!board[r, c].IsMine)
                {
                    board[r, c].IsMine = true;
                    placed++;
                }
            }
        }

        // Logic: calculate adjacent mine counts
        private void CalculateNumbers()
        {
            int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (board[i, j].IsMine) continue;

                    int count = 0;

                    for (int k = 0; k < 8; k++)
                    {
                        int nr = i + dr[k];
                        int nc = j + dc[k];

                        if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                        {
                            if (board[nr, nc].IsMine)
                                count++;
                        }
                    }

                    board[i, j].AdjacentMines = count;
                }
            }
        }

        // UI: create visual grid of buttons
        private void CreateButtons()
        {
            int topOffset = 60 + 10; //60 = panel-height, 10 = padding
            int cellSize = 35;

            buttons = new Button[rows, cols];
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button btn = new Button();

                    // UI: size & position
                    btn.Width = cellSize;
                    btn.Height = cellSize;
                    btn.Left = j * cellSize;
                    btn.Top = topOffset + i * cellSize;

                    // UI: styling (modern flat look)
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = Color.Gray;

                    btn.BackColor = Color.FromArgb(70, 70, 70);
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                    // Logic: track position
                    btn.Tag = new Point(i, j);

                    // Logic: click handling
                    btn.MouseDown += Cell_MouseDown;

                    this.Controls.Add(btn);
                    buttons[i, j] = btn;
                }
            }
        }
        private Color GetNumberColor(int number)
        {
            switch (number)
            {
                case 1: return Color.LightBlue;
                case 2: return Color.LightGreen;
                case 3: return Color.Red;
                case 4: return Color.DarkBlue;
                case 5: return Color.Maroon;
                case 6: return Color.Teal;
                case 7: return Color.Black;
                case 8: return Color.Gray;
                default: return Color.White;
            }
        }

        // Logic: handle left/right click
        private void Cell_MouseDown(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            Point p = (Point)btn.Tag;

            int r = p.X;
            int c = p.Y;

            if (e.Button == MouseButtons.Left)
            {
                RevealCell(r, c);
            }
            else if (e.Button == MouseButtons.Right)
            {
                ToggleFlag(r, c);
            }
        }
        private void ToggleFlag(int r, int c)
        {
            Cell cell = board[r, c];
            Button btn = buttons[r, c];

            // Logic: cannot flag revealed cells
            if (cell.IsRevealed)
                return;

            cell.IsFlagged = !cell.IsFlagged;

            // UI: flag styling
            btn.Text = cell.IsFlagged ? "🚩" : "";
            btn.ForeColor = Color.Orange;
        }

        // Logic: reveal cell + recursion (flood fill)
        private void RevealCell(int r, int c)
        {
            if (r < 0 || r >= rows || c < 0 || c >= cols)
                return;

            Cell cell = board[r, c];

            if (cell.IsRevealed || cell.IsFlagged)
                return;

            cell.IsRevealed = true;
            Button btn = buttons[r, c];

            if (cell.IsMine)
            {
                RevealAllMines();
                MessageBox.Show("Game Over!");
                return;
            }

            // UI: revealed cell appearance (pressed look)
            btn.BackColor = Color.FromArgb(200, 200, 200);
            btn.FlatAppearance.BorderColor = Color.DarkGray;

            // Logic + UI: show number
            if (cell.AdjacentMines > 0)
            {
                btn.Text = cell.AdjacentMines.ToString();
                btn.ForeColor = GetNumberColor(cell.AdjacentMines);
            }
            else
            {
                btn.Text = "";
            }

            btn.Enabled = false;

            // If empty → expand
            if (cell.AdjacentMines == 0)
            {
                RevealCell(r - 1, c);
                RevealCell(r + 1, c);
                RevealCell(r, c - 1);
                RevealCell(r, c + 1);
            }
            if (CheckWin())
            {
                MessageBox.Show("You Win! 🎉");
            }
        }

        // Logic: check win condition
        private bool CheckWin()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Cell cell = board[i, j];

                    if (!cell.IsMine && !cell.IsRevealed)
                        return false;
                }
            }
            return true;
        }

        // UI: show all mines on game over
        private void RevealAllMines()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (board[i, j].IsMine)
                    {
                        Button btn = buttons[i, j];

                        // UI: mine styling
                        btn.Text = "💣";
                        btn.BackColor = Color.DarkRed;
                        btn.ForeColor = Color.White;
                    }
                }
            }
        }
    }
    public class Cell
    {
        public bool IsMine { get; set; }
        public int AdjacentMines { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }
    }
}
