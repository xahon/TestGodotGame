using System;
using System.Collections.Generic;
using Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay
{
    public class PlaygroundGrid : Node2D
    {
        [Export] private NodePath TileMapPath { get; set; }
        [Export] private NodePath GameUiPath { get; set; }
        [Export] private PackedScene PlayerScene { get; set; }
        [Export] private PackedScene ObstacleScene { get; set; }
        [Export] private PackedScene EnemyScene { get; set; }
        [Export] private PackedScene MoveIconScene { get; set; }
        [Export] private PackedScene AttackIconScene { get; set; }
        [Export] private PackedScene TeleportIconScene { get; set; }
        [Export] private float MoveAnimationSpeed { get; set; } = 100.0f;

        [Signal] public delegate void OnLoseGame();

        private TileMap TileMap { get; set; }
        private GameUi GameUi { get; set; }
        private Entity PlayerObject { get; set; }
        private List<Entity> ObstacleObjects { get; set; } = new List<Entity>();
        private List<Entity> EnemyObjects { get; set; } = new List<Entity>();

        private class Coordinates
        {
            public int x;
            public int y;

            public Coordinates() { }
            public Coordinates(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
        private List<Coordinates> ValidMoves { get; set; } = new List<Coordinates>();

        private enum ObjectType
        {
            Player = 1 << 0,
            Enemy = 1 << 1,
            Obstacle = 1 << 2,
        }

        private class Entity
        {
            public Coordinates coordinates;
            public Node2D obj;
        }
        
        private PlaygroundGridState PlaygroundGridState { get; set; }
        private Tween PlayerAnimationTween { get; set; }
        private Tween EnemyAnimationTween { get; set; }
        private Array<Node2D> Icons { get; set; } = new Array<Node2D>();

        public override void _EnterTree()
        {
            TileMap = this.GetNodeChecked<TileMap>(TileMapPath);
            GameUi = this.GetNodeChecked<GameUi>(GameUiPath);

            int sizeX = 0;
            int sizeY = 0;
            
            while (true)
            {
                int cellId = TileMap.GetCell(sizeX, 0);
                if (cellId == TileMap.InvalidCell)
                    break;
                sizeX++;
            }

            while (true)
            {
                int cellId = TileMap.GetCell(0, sizeY);
                if (cellId == TileMap.InvalidCell)
                    break;
                sizeY++;
            }

            PlaygroundGridState = new PlaygroundGridState(sizeX, sizeY);

            GD.Print("GridSizeX: " + PlaygroundGridState.SizeX + ", GridSizeY: " + PlaygroundGridState.SizeY);
        }

        public override void _Ready()
        {
            GameUi.Connect(nameof(GameUi.OnDieCaptured), this, nameof(_OnDieCaptured));
            GameUi.Connect(nameof(GameUi.OnDieReleased), this, nameof(_OnDieReleased));
            GameUi.Connect(nameof(GameUi.OnReroll), this, nameof(_OnReroll));

            PlayerAnimationTween = new Tween();
            AddChild(PlayerAnimationTween);

            EnemyAnimationTween = new Tween();
            AddChild(EnemyAnimationTween);
        }

        public void RandomizeLevel()
        {
            Coordinates playerCoordinates = new Coordinates();
            playerCoordinates.x = (int)(GD.Randi() % PlaygroundGridState.SizeX);
            playerCoordinates.y = (int)(GD.Randi() % PlaygroundGridState.SizeY);
            PlayerObject = new Entity();
            PlayerObject.coordinates = playerCoordinates;
            PlayerObject.obj = PlayerScene.Instance<Player>();
            PlayerObject.obj.Position = GetCellPosition(playerCoordinates.x, playerCoordinates.y);
            AddChild(PlayerObject.obj);
            PlaygroundGridState.SetCell(playerCoordinates.x, playerCoordinates.y, (int) ObjectType.Player);

            int obstaclesCount = 3 + (int)(GD.Randi() % 4); // 3-6 obstacles
            for (int i = 0; i < obstaclesCount; i++)
            {
                Coordinates obstacleCoordinates = new Coordinates();

                while (true)
                {
                    obstacleCoordinates.x = (int)(GD.Randi() % PlaygroundGridState.SizeX);
                    obstacleCoordinates.y = (int)(GD.Randi() % PlaygroundGridState.SizeY);

                    if (PlaygroundGridState.CellEmpty(obstacleCoordinates.x, obstacleCoordinates.y))
                        break;
                }

                Entity obstacleObject = new Entity();
                obstacleObject.coordinates = obstacleCoordinates;
                obstacleObject.obj = ObstacleScene.Instance<Node2D>();
                obstacleObject.obj.Position = GetCellPosition(obstacleCoordinates.x, obstacleCoordinates.y);
                AddChild(obstacleObject.obj);
                PlaygroundGridState.SetCell(obstacleCoordinates.x, obstacleCoordinates.y, (int)ObjectType.Obstacle);
                
                ObstacleObjects.Add(obstacleObject);
            }

            int enemiesCount = 3 + (int)(GD.Randi() % 4); // 3-6 enemies
            for (int i = 0; i < enemiesCount; i++)
            {
                Coordinates enemyCoordinates = new Coordinates();

                while (true)
                {
                    enemyCoordinates.x = (int)(GD.Randi() % PlaygroundGridState.SizeX);
                    enemyCoordinates.y = (int)(GD.Randi() % PlaygroundGridState.SizeY);

                    if (PlaygroundGridState.CellEmpty(enemyCoordinates.x, enemyCoordinates.y))
                        break;
                }

                Entity enemyObject = new Entity();
                enemyObject.coordinates = enemyCoordinates;
                enemyObject.obj = EnemyScene.Instance<Node2D>();
                enemyObject.obj.Position = GetCellPosition(enemyCoordinates.x, enemyCoordinates.y);
                AddChild(enemyObject.obj);
                PlaygroundGridState.SetCell(enemyCoordinates.x, enemyCoordinates.y, (int)ObjectType.Enemy);
                
                EnemyObjects.Add(enemyObject);
            }
        }

        public async void MakeTurn()
        {
            GameUi.SetInteractable(false);

            foreach (Entity enemyObject in EnemyObjects)
            {
                int origX = enemyObject.coordinates.x;
                int origY = enemyObject.coordinates.y;

                int toPlayerGlobalX = PlayerObject.coordinates.x - enemyObject.coordinates.x;
                int toPlayerGlobalY = PlayerObject.coordinates.y - enemyObject.coordinates.y;

                if (Mathf.Abs(toPlayerGlobalX) == 1 && toPlayerGlobalY == 0 || Mathf.Abs(toPlayerGlobalY) == 1 && toPlayerGlobalX == 0)
                {
                    GD.Print("Game lost");
                    EmitSignal(nameof(OnLoseGame));
                    break;
                }
                
                int toPlayerX = Mathf.Sign(toPlayerGlobalX);
                int toPlayerY = Mathf.Sign(toPlayerGlobalY);

                if (toPlayerX != 0 && PlaygroundGridState.CellEmpty(origX + toPlayerX, origY))
                {
                    PlaygroundGridState.ClearCell(origX, origY);
                    PlaygroundGridState.SetCell(origX + toPlayerX, origY, (int)ObjectType.Enemy);
                    enemyObject.coordinates.x = origX + toPlayerX;
                }
                else if (toPlayerY != 0 && PlaygroundGridState.CellEmpty(origX, origY + toPlayerY))
                {
                    PlaygroundGridState.ClearCell(origX, origY);
                    PlaygroundGridState.SetCell(origX, origY + toPlayerY, (int)ObjectType.Enemy);
                    enemyObject.coordinates.y = origY + toPlayerY;
                }
                else continue;

                Vector2 startPos = GetCellPosition(origX, origY);
                Vector2 endPos = GetCellPosition(enemyObject.coordinates.x, enemyObject.coordinates.y);
                float distance = startPos.DistanceTo(endPos);

                EnemyAnimationTween.InterpolateProperty(
                    enemyObject.obj,
                    "position",
                    enemyObject.obj.Position,
                    GetCellPosition(enemyObject.coordinates.x, enemyObject.coordinates.y),
                    distance / MoveAnimationSpeed
                );
            }

            EnemyAnimationTween.Start();
            await ToSignal(EnemyAnimationTween, "tween_all_completed");

            GameUi.SetInteractable(true);
        }

        private Vector2 GetCellPosition(int x, int y)
        {
            return TileMap.MapToWorld(new Vector2(x, y), ignoreHalfOfs: true);
        }

        private (int x, int y) GetCellPositionFromVector2(Vector2 position)
        {
            Vector2 result = TileMap.WorldToMap(TileMap.ToLocal(position));
            return ((int x, int y))(result.x, result.y);
        }
        
        private void SetCellIcon(int x, int y, Node2D icon)
        {
            if (!PlaygroundGridState.IsCellWithinBounds(x, y))
                return;

            icon.Position = GetCellPosition(x, y);
            CallDeferred("add_child", icon);

            Icons.Add(icon);
        }

        private void ClearIcons()
        {
            foreach (Node2D icon in Icons)
                RemoveChild(icon);
            Icons.Clear();
        }

        private async void OnPlayerMove(DieControl dieControl, int x, int y)
        {
            GameUi.ConsumeDie(dieControl);
            GameUi.SetInteractable(false);

            PlayerAnimationTween.ResetAll();

            if (dieControl.DieColor == GameUi.DieColor.Red)
            {
                PlaygroundGridState.ClearPath(PlayerObject.coordinates.x, PlayerObject.coordinates.y, x, y, ignoreSelf: true);
                ((Player)PlayerObject.obj).SetAttackMode(true);
            }

            PlaygroundGridState.ClearCell(PlayerObject.coordinates.x, PlayerObject.coordinates.y);
            PlaygroundGridState.SetCell(x, y, (int)ObjectType.Player);
            PlayerObject.coordinates.x = x;
            PlayerObject.coordinates.y = y;

            switch (dieControl.DieColor)
            {
                case GameUi.DieColor.Red:
                case GameUi.DieColor.Yellow:
                {
                    Vector2 startPos = PlayerObject.obj.Position;
                    Vector2 endPos = GetCellPosition(x, y);
                    float distance = startPos.DistanceTo(endPos);
                    PlayerAnimationTween.InterpolateProperty(PlayerObject.obj, "position", startPos, endPos, distance / MoveAnimationSpeed);
                    PlayerAnimationTween.Start();
                    await ToSignal(PlayerAnimationTween, "tween_all_completed");
                    break;
                }
                case GameUi.DieColor.Blue:
                    PlayerObject.obj.Position = GetCellPosition(x, y);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ((Player)PlayerObject.obj).SetAttackMode(false);

            GameUi.SetInteractable(true);
        }

        private void _OnDieCaptured(DieControl dieControl)
        {
            PackedScene iconAsset;
            switch (dieControl.DieColor)
            {
                case GameUi.DieColor.Red:
                {
                    iconAsset = AttackIconScene;
                    break;
                }
                case GameUi.DieColor.Blue:
                {
                    iconAsset = TeleportIconScene;
                    break;
                }
                case GameUi.DieColor.Yellow:
                {
                    iconAsset = MoveIconScene;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            int playerX = PlayerObject.coordinates.x;
            int playerY = PlayerObject.coordinates.y;

            int rightX = playerX + dieControl.Value;
            int leftX = playerX - dieControl.Value;
            int upY = playerY - dieControl.Value;
            int downY = playerY + dieControl.Value;

            ValidMoves.Clear();

            switch (dieControl.DieColor)
            {
                case GameUi.DieColor.Red:
                {
                    if (PlaygroundGridState.PathIsNoneOf(playerX, playerY, rightX, playerY, ignoreSelf: true, (int)ObjectType.Obstacle))
                    {
                        SetCellIcon(rightX, playerY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(rightX, playerY));
                    }
                    if (PlaygroundGridState.PathIsNoneOf(playerX, playerY, leftX, playerY, ignoreSelf: true, (int)ObjectType.Obstacle))
                    {
                        SetCellIcon(leftX, playerY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(leftX, playerY));
                    }
                    if (PlaygroundGridState.PathIsNoneOf(playerX, playerY, playerX, upY, ignoreSelf: true, (int)ObjectType.Obstacle))
                    {
                        SetCellIcon(playerX, upY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(playerX, upY));
                    }
                    if (PlaygroundGridState.PathIsNoneOf(playerX, playerY, playerX, downY, ignoreSelf: true, (int)ObjectType.Obstacle))
                    {
                        SetCellIcon(playerX, downY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(playerX, downY));
                    }
                    break;
                }
                case GameUi.DieColor.Yellow:
                {
                    if (PlaygroundGridState.PathEmpty(playerX, playerY, rightX, playerY, ignoreSelf: true))
                    {
                        SetCellIcon(rightX, playerY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(rightX, playerY));
                    }
                    if (PlaygroundGridState.PathEmpty(playerX, playerY, leftX, playerY, ignoreSelf: true))
                    {
                        SetCellIcon(leftX, playerY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(leftX, playerY));
                    }
                    if (PlaygroundGridState.PathEmpty(playerX, playerY, playerX, upY, ignoreSelf: true))
                    {
                        SetCellIcon(playerX, upY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(playerX, upY));
                    }
                    if (PlaygroundGridState.PathEmpty(playerX, playerY, playerX, downY, ignoreSelf: true))
                    {
                        SetCellIcon(playerX, downY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(playerX, downY));
                    }
                    break;
                }
                case GameUi.DieColor.Blue:
                {
                    if (PlaygroundGridState.CellEmpty(rightX, playerY))
                    {
                        SetCellIcon(rightX, playerY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(rightX, playerY));
                    }
                    if (PlaygroundGridState.CellEmpty(leftX, playerY))
                    {
                        SetCellIcon(leftX, playerY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(leftX, playerY));
                    }
                    if (PlaygroundGridState.CellEmpty(playerX, upY))
                    {
                        SetCellIcon(playerX, upY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(playerX, upY));
                    }
                    if (PlaygroundGridState.CellEmpty(playerX, downY))
                    {
                        SetCellIcon(playerX, downY, iconAsset.Instance<Node2D>());
                        ValidMoves.Add(new Coordinates(playerX, downY));
                    }
                    break;
                }
            }
        }

        private void _OnDieReleased(DieControl dieControl)
        {
            (int x, int y) = GetCellPositionFromVector2(GetGlobalMousePosition());

            if (PlaygroundGridState.IsCellWithinBounds(x, y))
            {
                foreach (Coordinates validMove in ValidMoves)
                {
                    if (validMove.x == x && validMove.y == y)
                    {
                        OnPlayerMove(dieControl, x, y);
                    }
                }
            }
            
            ClearIcons();
        }

        private void _OnReroll()
        {
            MakeTurn();
        }
    }
}
