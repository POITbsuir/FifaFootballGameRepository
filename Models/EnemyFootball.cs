using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop.Input;

namespace FifaFootballGame.Models
{
    public abstract class EnemyFootball
    {
        protected int _speed = 2;
        protected Texture2D _textureAI;
        protected Ball _ball;
        protected Vector2 _currentPositionAI;

        protected int _dribling = 80;
        protected int _headingGame = 75;

        protected string _position { get; set; }

        protected bool _isControlBall;

        public bool IsControlBall
        {
            get => _isControlBall;
            set => _isControlBall = value;
        }
        //вектор начальной позиции
        protected Vector2 _homePosition;
        public AITask CurrentTask { get; set; }

        public void SetTask(AITask task)
        {
            CurrentTask = task;
        }

        public EnemyFootball(Texture2D texture, Vector2 currentPositionAI, Ball ball, string position)
        {
            _textureAI = texture;
            _ball = ball;
            _currentPositionAI = currentPositionAI;
            _isControlBall = false;
            _position = position;
            _homePosition = currentPositionAI;
        }

        public virtual void MoveAI(int width, int height, List<FootballPlayer> players, List<EnemyFootball> enemies)
        {
        }
        //методы для возвращения в исходные позиции
        public void ResetToHome()
        {
            _currentPositionAI = _homePosition;
            IsControlBall = false;
        }

        public void SetPosition(Vector2 position)
        {
            _currentPositionAI = position;
        }

        public string GetRole() => _position;

        public void DrawAIPlayers(SpriteBatch spriteBatch)
        {
            Rectangle destination = new Rectangle(
                (int)_currentPositionAI.X,
                (int)_currentPositionAI.Y,
                32,
                32
            );

            spriteBatch.Draw(_textureAI, destination, Color.White);
        }

        public int GetValueDribling() => _dribling;
        public int GetValueSpeed() => _speed;
        public int GetValueHeadingGame() => _headingGame;
        public Vector2 Position() => _currentPositionAI;
    }
}