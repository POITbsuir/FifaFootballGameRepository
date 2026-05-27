using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FifaFootballGame.Models
{
    public class Ball
    {
        private Texture2D _textureBall;
        private Vector2 _positionBall;
        private float _radiusBall;

        private FootballPlayer _owner;
        private EnemyFootball _enemyOwner;

        private FootballPlayer _passTarget;

        private bool _isControlled;
        private bool _isPassing;
        private bool _isShooting;

        private float _passSpeed = 16f; //изменить на 6f

        private Vector2 _shootDirection;
        private float _shootSpeed;

        private int _windowWidth = 900;
        private int _windowHeight = 600;

        public event Action<FootballPlayer> OwnerChanged;

        public Ball(Texture2D texture, Vector2 startPosition, float radius)
        {
            _textureBall = texture;
            _positionBall = startPosition;
            _radiusBall = radius;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float diameter = _radiusBall * 2;

            Rectangle destination = new Rectangle(
                (int)(_positionBall.X - _radiusBall),
                (int)(_positionBall.Y - _radiusBall),
                (int)diameter,
                (int)diameter
            );

            spriteBatch.Draw(_textureBall, destination, Color.White);
        }

        public Vector2 GetPositionBall() => _positionBall;

        public bool IsControlled() => _isControlled;
        public bool IsPassing() => _isPassing;
        public bool IsShooting() => _isShooting;

        public bool IsPlayerOwner() => _owner != null;
        public bool IsEnemyOwner() => _enemyOwner != null;

        public FootballPlayer GetOwner() => _owner;
        public EnemyFootball GetEnemyOwner() => _enemyOwner;

        public void StartPositionBall()
        {
            _positionBall = new Vector2(_windowWidth / 2 - 16, _windowHeight / 2 - 16);
            _isPassing = false;
            _isShooting = false;
        }

        public void DropOwner()
        {
            if (_owner != null)
                _owner.IsControllBall = false;

            if (_enemyOwner != null)
                _enemyOwner.IsControlBall = false;

            _owner = null;
            _enemyOwner = null;

            _passTarget = null;

            _isControlled = false;
        }

        public void SetOwner(FootballPlayer player)
        {
            DropOwner();

            _owner = player;
            _owner.IsControllBall = true;

            _isControlled = true;
            _isPassing = false;
            _isShooting = false;

            OwnerChanged?.Invoke(player);
        }

        public void SetEnemyOwner(EnemyFootball enemy)
        {
            DropOwner();

            _enemyOwner = enemy;
            _enemyOwner.IsControlBall = true;

            _isControlled = true;
            _isPassing = false;
            _isShooting = false;
        }

        public void StartPass(FootballPlayer target)
        {
            if (_owner == null || target == null)
                return;

            if (_owner != null)
                _owner.IsControllBall = false;

            _owner = null;
            _enemyOwner = null;

            _passTarget = target;

            _isControlled = false;
            _isPassing = true;
            _isShooting = false;
        }

        public void Shoot(Vector2 direction, float speed)
        {
            if (direction == Vector2.Zero)
                return;

            direction.Normalize();

            DropOwner();

            _isControlled = false;
            _isPassing = false;
            _isShooting = true;

            _shootDirection = direction;
            _shootSpeed = speed;
        }

        public void Update()
        {
            if (_isControlled && _owner != null)
            {
                Vector2 playerPos = _owner.GetPosition();
                _positionBall = new Vector2(playerPos.X + 24, playerPos.Y + 16);
                return;
            }

            if (_isControlled && _enemyOwner != null)
            {
                Vector2 enemyPos = _enemyOwner.Position();
                _positionBall = new Vector2(enemyPos.X - 8, enemyPos.Y + 16);
                return;
            }

            if (_isPassing && _passTarget != null)
            {
                Vector2 targetPos = _passTarget.GetPosition() + new Vector2(16, 16);
                Vector2 direction = targetPos - _positionBall;

                if (direction.Length() < _passSpeed)
                {
                    SetOwner(_passTarget);
                    return;
                }

                direction.Normalize();
                _positionBall += direction * _passSpeed;
                return;
            }
            //тут удар 
            if (_isShooting)
            {
                _positionBall += _shootDirection * _shootSpeed;
                _shootSpeed *= 0.98f;

                if (_shootSpeed < 0.4f)
                    _isShooting = false;
            }

            //ClampToField();
            ClampOnlyY();
        }
        //метод возвращаюсщий позицию
        public void SetPosition(Vector2 position)
        {
            _positionBall = position;
        }

        private void ClampToField()
        {
            _positionBall.X = MathHelper.Clamp(_positionBall.X, _radiusBall, _windowWidth - _radiusBall);
            _positionBall.Y = MathHelper.Clamp(_positionBall.Y, _radiusBall, _windowHeight - _radiusBall);
        }
        //метод используемый для оценки удара
        private void ClampOnlyY()
        {
            _positionBall.Y = MathHelper.Clamp(_positionBall.Y, _radiusBall, _windowHeight - _radiusBall);
        }
    }
}