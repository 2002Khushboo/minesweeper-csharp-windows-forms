using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class Form1 : Form
    {
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
            InitializeBoard();
            PlaceMines(15);
            CalculateNumbers();
            CreateButtons();
        }
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
        private void CreateButtons()
        {
            buttons = new Button[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button btn = new Button();
                    btn.Width = 30;
                    btn.Height = 30;
                    btn.Left = j * 30;
                    btn.Top = i * 30;
                    btn.ForeColor = Color.Black;
                    btn.Tag = new Point(i, j);
                    btn.MouseDown += Cell_MouseDown;

                    this.Controls.Add(btn);
                    buttons[i, j] = btn;
                }
            }
        }
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

            if (cell.IsRevealed)
                return;

            cell.IsFlagged = !cell.IsFlagged;

            btn.Text = cell.IsFlagged ? "🚩" : "";
        }
        /*private void Cell_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Point p = (Point)btn.Tag;

            int r = p.X;
            int c = p.Y;

            RevealCell(r, c);
        }*/
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

            btn.Text = cell.AdjacentMines > 0 ? cell.AdjacentMines.ToString() : "";
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
        private void RevealAllMines()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (board[i, j].IsMine)
                    {
                        buttons[i, j].Text = "💣";
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
