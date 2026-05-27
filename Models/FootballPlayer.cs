using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using FifaFootballGame.Models;
using System.Windows.Input;
using MonoGame.Framework.WpfInterop.Input;
using System.Windows.Forms;

namespace FifaFootballGame.Models
{
    public abstract class FootballPlayer
    {
        protected string _position { get; set; }
        public string _workingLeg { get; set; }
        protected int _speed { get; set; }
        protected int _impactForce = 85; //сила удара
        protected int _headingGame { get; set; }
        protected int _dribling { get; set; }
        protected int _countFints { get; set; }
        protected int _bodyGame { get; set; }
        protected bool _isPlayHands { get; set; }
        protected string _colorTeam { get; set; }
        protected bool _isControllBall = false;
        protected bool _isInterceptionBall { get; set; } //проверяю отобрали ли мяч
        protected bool _isControllBallTeam { get; set; }
      
        public bool isActive = false;
        public bool IsControllBall
        {
            get => _isControllBall;
            set => _isControllBall = value;
        }

        public bool IsControllBallTeam
        {
            get => _isControllBallTeam;
            set => _isControllBallTeam = value;
        }
        protected int _directionPlay { get; set; }

        protected Vector2 _currentPointFootballPlayer;

        protected Texture2D _texturePlayer;
        protected Ball _gameBall;

        //поля для движения игроков
        protected Vector2 _homePosition;
        protected Vector2 _targetPosition;
        protected Random _random = new Random();

        protected float _decisionTimer = 0f;
        protected FootballPlayer(Texture2D texture, Vector2 startPosition, Ball ball) 
        {
            _currentPointFootballPlayer = startPosition;
            _texturePlayer = texture;
            _isControllBall = false;
            _gameBall = ball;

            _homePosition = startPosition;
            _targetPosition = startPosition;
        }

        public void MoveCurrentPlayer(WpfKeyboard keyboard)
        {
            //логика движения по кнопкам
            var state = keyboard.GetState();

            if (!isActive) 
                return;

            //словарь, в котором хранятся направления и делегат, выполняющий действия
            var moves = new Dictionary<Keys, Action>
            {
                { Keys.W, () => _currentPointFootballPlayer.Y -= _speed },
                { Keys.S, () => _currentPointFootballPlayer.Y += _speed },
                { Keys.A, () => _currentPointFootballPlayer.X -= _speed },
                { Keys.D, () => _currentPointFootballPlayer.X += _speed },
                { Keys.Space, () => Shoot(_gameBall) },
                
            };

            foreach (var move in moves)
                if (state.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)move.Key))
                    move.Value();
        }

        public void MoveAllPlayers(List<FootballPlayer> footballPlayers, int widht, int height, List<EnemyFootball> agents)
        {
            foreach (var player in footballPlayers)
            {
                if (!player.isActive)
                {
                    player.MovePlayers(widht, height, agents);
                }
            }
        }

        //методы отвечающие за передвижение игроков
        public virtual void MovePlayers(int widht, int height, List<EnemyFootball> agents) { }
        public virtual void AttackMove() { }
        public virtual void DefendsMove() { }
        //метод отрисовки текстурф футболиста
        public void DrawFootballPlayer(SpriteBatch spriteBatch)
        {
            Rectangle destination = new Rectangle((int)_currentPointFootballPlayer.X, (int)(_currentPointFootballPlayer.Y), 32, 32);
            spriteBatch.Draw(_texturePlayer, destination, Color.White);
        }
        //сделать общими
        public void Shoot(Ball ball)
        {
            if (!IsControllBall)
                return;

            ball.Shoot(new Vector2(1, 0), _impactForce / 5f);
        }
        //метод получения текстуры игрока
        public Texture2D GetTexture() => _texturePlayer;
        //метод получения позиции игрока
        public Vector2 GetPosition() => _currentPointFootballPlayer;

        //движение при атаке
        protected void MoveToTarget(Vector2 target)
        {
            Vector2 direction = target - _currentPointFootballPlayer;

            if (direction.Length() < 6f)
                return;

            direction.Normalize();
            _currentPointFootballPlayer += direction * (_speed * 0.42f);
        }

        protected Vector2 ClampToField(Vector2 position, int width, int height)
        {
            position.X = MathHelper.Clamp(position.X, 20, width - 40);
            position.Y = MathHelper.Clamp(position.Y, 20, height - 40);

            return position;
        }

        public virtual void SmartMove(
    int width,
    int height,
    List<FootballPlayer> team,
    List<EnemyFootball> enemies,
    TeamTacticContext context)
        {
            MovePlayers(width, height, enemies);
        }

        protected FootballPlayer FindNearestTeammate(List<FootballPlayer> team)
        {
            FootballPlayer nearest = null;
            float minDistance = float.MaxValue;

            foreach (var player in team)
            {
                if (player == this)
                    continue;

                float distance = Vector2.Distance(GetPosition(), player.GetPosition());

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = player;
                }
            }

            return nearest;
        }

        protected EnemyFootball FindNearestEnemy(List<EnemyFootball> enemies)
        {
            EnemyFootball nearest = null;
            float minDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(GetPosition(), enemy.Position());

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        protected bool IsNearBall(float radius)
        {
            return Vector2.Distance(GetPosition() + new Vector2(16, 16), _gameBall.GetPositionBall()) <= radius;
        }

        protected Vector2 GetOpenSpace(Vector2 basePoint, int width, int height, List<EnemyFootball> enemies)
        {
            Vector2 bestPoint = basePoint;
            float bestScore = float.MinValue;

            for (int i = 0; i < 8; i++)
            {
                Vector2 candidate = basePoint + new Vector2(
                    _random.Next(-80, 81),
                    _random.Next(-100, 101)
                );

                candidate = ClampToField(candidate, width, height);

                float enemyDistanceScore = 0;

                foreach (var enemy in enemies)
                {
                    enemyDistanceScore += Vector2.Distance(candidate, enemy.Position());
                }

                float fieldScore = candidate.X;

                float score = enemyDistanceScore * 0.6f + fieldScore * 0.4f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestPoint = candidate;
                }
            }

            return bestPoint;
        }
        //метод сброса позиций, чтобы вернуться в исходное состояние
        public void ResetToHome()
        {
            _currentPointFootballPlayer = _homePosition;
            _targetPosition = _homePosition;
            isActive = false;
        }
        public Vector2 Position() => _currentPointFootballPlayer;
        public int GetValueDribling() => _dribling;
        public int GetValueSpeed() => _speed;
        public int GetValueHeadingGame() => _headingGame;
        //метод для получения позиции при сервреной игре
        public void SetNetworkPosition(Vector2 position)
        {
            _currentPointFootballPlayer = position;
        }
    }


}
