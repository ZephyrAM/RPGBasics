using Godot;
using Godot.Collections;
using System;

namespace ZAM.Core
{
    public partial class GridBattleSystem : BattleSystem
    {
        [Export] private CharacterBody2D playerBody = null;
        [Export] private Node2D battleGrid = null;
        [Export] private PackedScene gridSquare = null;
        [Export] private int squareCount = 75;
        [Export] private int gridHeight = 5;
        [Export] private int gridWidth = 15;
        [Export] private Vector2 gridTopLeft = new(115, 350);

        private Array<ColorRect> gridArray = [];
        // private int gridColumn = 0;

        public override void _Ready()
        {
            BuildGrid();
            MovePlayer();
        }

        private void BuildGrid()
        {
            if (gridSquare != null)
            {
                gridArray = [];
                foreach (Node child in battleGrid.GetChildren())
                {
                    child.Free();
                }

                for (int row = 0; row < gridHeight; row++)
                {
                    for (int column = 0; column < gridWidth; column++)
                    {
                        ColorRect nextSquare = gridSquare.Instantiate() as ColorRect; // EDIT: Temporary? Design grid without nodes?
                        gridArray.Add(nextSquare);
                        battleGrid.AddChild(nextSquare);
                        nextSquare.Position = new Vector2(column * nextSquare.Size.X, row * nextSquare.Size.Y) + gridTopLeft;
                    }
                }
            }
        }

        private void MovePlayer()
        {
            ColorRect playerSquare = gridArray[14];
            playerBody.Position = new Vector2(playerSquare.Position.X + (playerSquare.Size.X / 2), playerSquare.Position.Y);
        }
    }
}