using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using FifaFootballGame.Models;
using System.Windows.Input;
using MonoGame.Framework.WpfInterop.Input;
using System.Collections.Generic;

namespace FifaFootballGame.Models
{
    class Forward : FootballPlayer
    {
        private const int PRESSING_POSITION = 72;

        private Vector2 _smoothTargetPosition;
        private float _decisionTimer = 0f;
        private const float DECISION_INTERVAL = 45f; // примерно раз в 45 кадров
        private int _runDirection = 1;
        private Random _random = new Random();

        private const float TARGET_DEAD_ZONE = 8f;
        private const float TARGET_SMOOTHING = 0.08f;

        private int _width;
        private int _height;
        private Vector2 _startPosition;

        //движение вперед при атаке
        private bool _forwardDirection = false; //переменная для движения вперед при атаке
        private bool _backwardDirection = false; //переменная для плассировки

        public Forward(Texture2D texture, Vector2 startPosition, Ball ball) : base(texture, startPosition, ball)
        {
            _position = "Forward";
            _workingLeg = "left";
            _speed = 4;
            _dribling = 85;
            _startPosition = startPosition;
            _smoothTargetPosition = startPosition;
        }

        public override void MovePlayers(int width, int height, List<EnemyFootball> agents)
        {
            _width = width;
            _height = height;

            if (_gameBall.IsControlled())
                AttackMove();
            else
                DefendsMove(agents);
        }

        public override void AttackMove()
        {
            Vector2 ballPos = _gameBall.GetPositionBall();

            float squareBaseX = _width * 0.85f;
            float squareBaseY = _height / 2f;

            Vector2 desiredTarget;

            _decisionTimer++;

            if (ballPos.X < _width * 0.55f)
            {
                desiredTarget = new Vector2(squareBaseX, squareBaseY);

                _decisionTimer = 0;
                _smoothTargetPosition = desiredTarget;
            }
            else
            {
                if (_decisionTimer >= DECISION_INTERVAL)
                {
                    _decisionTimer = 0;

                    // Форвард не дергается каждый кадр, а выбирает новый рывок редко
                    _runDirection = _random.Next(0, 2) == 0 ? -1 : 1;
                }

                float attackProgress = MathHelper.Clamp(
                    (ballPos.X - _width * 0.55f) / (_width * 0.4f),
                    0f,
                    1f
                );

                float diagonalX = MathHelper.Lerp(squareBaseX - 70, squareBaseX + 40, attackProgress);
                float diagonalY = squareBaseY + _runDirection * MathHelper.Lerp(35, 85, attackProgress);

                // Если мяч у лицевой — открываемся ближе к воротам
                if (ballPos.X > _width * 0.88f)
                {
                    diagonalX = squareBaseX + 60;
                    diagonalY = MathHelper.Lerp(diagonalY, ballPos.Y, 0.35f);
                }

                desiredTarget = new Vector2(diagonalX, diagonalY);
            }

            desiredTarget = ClampToField(desiredTarget, _width, _height);

            // Не меняем цель, если отличие слишком маленькое
            if (Vector2.Distance(_smoothTargetPosition, desiredTarget) > TARGET_DEAD_ZONE)
            {
                _smoothTargetPosition = Vector2.Lerp(
                    _smoothTargetPosition,
                    desiredTarget,
                    TARGET_SMOOTHING
                );
            }

            _targetPosition = _smoothTargetPosition;
            MoveToTarget(_targetPosition);
        }

        // Обновленный метод защиты, принимающий список агентов для поиска ближайшего
        public void DefendsMove(List<EnemyFootball> agents)
        {
            EnemyFootball nearestAgent = null;
            float minDistance = float.MaxValue;

            // Находим ближайшего врага, которого нужно прессинговать или страховать
            foreach (var agent in agents)
            {
                float dist = Vector2.Distance(this.Position(), agent.Position());
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestAgent = agent;
                }
            }

            if (nearestAgent != null)
            {
                MoveForwardDefends(nearestAgent);
            }
            else
            {
                // Если врагов нет, возвращаемся на исходную
                _targetPosition = new Vector2(_startPosition.X, _startPosition.Y);
                MoveToTarget(_targetPosition);
            }
        }

        public new string GetPosition() => _position;

        //реализация логики движения при обороне
        private void MoveForwardDefends(EnemyFootball agent)
        {
            //получаю текущее расстояние между игроком и мячом
            Vector2 ballPos = _gameBall.GetPositionBall();
            float distance = Vector2.Distance(this.Position(), ballPos); // Используем this.Position для текущей позиции форварда

            if (distance <= PRESSING_POSITION)
            {
                //начинается прессинг и отбор
                if ((_dribling > agent.GetValueDribling() || (_speed > agent.GetValueSpeed())) && _headingGame > agent.GetValueHeadingGame())
                {
                    _isInterceptionBall = true;
                    _isControllBall = true;
                    isActive = true;

                    // Двигаемся к мячу
                    _targetPosition = ballPos;
                }
                else
                {
                    _isInterceptionBall = false; //нас обвели
                    // Отступаем/держим позицию
                    _targetPosition = new Vector2(ballPos.X - 20, ballPos.Y);
                }
            }
            else
            {
                _isInterceptionBall = false;
                _isControllBall = false;
                isActive = false;

                if (_startPosition.X > _width / 4)
                {
                    //плассируемся до тех пор, пока не дойдем до штрафной
                    // Логика плавного возврата на позицию или смещения относительно мяча
                    // Цель: быть между мячом и своими воротами, но не слишком глубоко

                    float targetX = MathHelper.Clamp(ballPos.X - 100, _width / 4, _startPosition.X);
                    float targetY = ballPos.Y; // Следим за мячом по высоте

                    _targetPosition = new Vector2(targetX, targetY);
                }
                else
                {
                    //начинаем жесткий прессинг и отбор мяча (в сранение идет сила сила дриблинга и скорость)
                    // Если мы уже в своей штрафной или близко к ней, и мяч рядом - прессингуем
                    if ((_dribling > agent.GetValueDribling() || (_speed > agent.GetValueSpeed())) && _headingGame > agent.GetValueHeadingGame())
                    {
                        _isInterceptionBall = true;
                        _isControllBall = true;
                        isActive = true;
                        _targetPosition = ballPos;
                    }
                    else
                    {
                        //высчитть вероятность сфолить
                        int foul = CalculateProbabilityFoul();
                        if (foul == 1)
                        {
                            _isInterceptionBall = true;
                            //но тут логика с фолом будет
                            _targetPosition = ballPos; // Идем в отбор (фол)
                        }
                        if (foul == 0)
                        {
                            _isInterceptionBall = false;
                            //нас обвели идет опасная атака
                            // Отступаем к воротам
                            _targetPosition = new Vector2(this.Position().X - 10, this.Position().Y);
                        }
                    }
                }
            }

            MoveToTarget(_targetPosition);
        }

        //рассчет вероятности фола
        private int CalculateProbabilityFoul()
        {
            Random random = new Random();
            return random.Next(0, 2);
        }
        public override void SmartMove(int width,
        int height,
        List<FootballPlayer> team,
        List<EnemyFootball> enemies,
        TeamTacticContext context)
        {
            _width = width;
            _height = height;

            if (context.BallIsFree && context.NearestToBall == this)
            {
                SmoothForwardMove(context.BallPosition);
                return;
            }

            if (context.HasBall)
            {
                if (context.Owner == this)
                {
                    CarryAsForward(width, height, enemies);
                    return;
                }

                Vector2 runPoint = GetForwardRunPoint(width, height, context, enemies);
                SmoothForwardMove(runPoint);
                return;
            }

            if (context.EnemyHasBall)
            {
                Vector2 pressPoint = GetForwardPressPoint(width, height, context);
                SmoothForwardMove(pressPoint);
                return;
            }

            SmoothForwardMove(_startPosition);
        }
        private void CarryAsForward(int width, int height, List<EnemyFootball> enemies)
        {
            Vector2 ball = _gameBall.GetPositionBall();

            Vector2 target = new Vector2(
                MathHelper.Clamp(ball.X + 70, width * 0.58f, width * 0.88f),
                MathHelper.Clamp(ball.Y, 60, height - 60)
            );

            target = AvoidEnemiesForward(target, enemies, width, height);
            SmoothForwardMove(target);
        }

        private Vector2 GetForwardRunPoint(
            int width,
            int height,
            TeamTacticContext context,
            List<EnemyFootball> enemies)
        {
            _decisionTimer++;

            if (_decisionTimer >= DECISION_INTERVAL)
            {
                _decisionTimer = 0;

                if (context.BallPosition.Y < height / 2f)
                    _runDirection = 1;
                else
                    _runDirection = -1;
            }

            float progress = MathHelper.Clamp(context.BallPosition.X / width, 0f, 1f);

            float x = MathHelper.Lerp(width * 0.62f, width * 0.86f, progress);
            float y = height / 2f + _runDirection * MathHelper.Lerp(45, 95, progress);

            if (context.BallPosition.X > width * 0.70f)
            {
                x = width * 0.88f;
                y = MathHelper.Lerp(y, context.BallPosition.Y, 0.25f);
            }

            Vector2 target = new Vector2(x, y);
            target = AvoidEnemiesForward(target, enemies, width, height);

            return ClampToField(target, width, height);
        }

        private Vector2 GetForwardPressPoint(int width, int height, TeamTacticContext context)
        {
            Vector2 ball = context.BallPosition;

            float x = MathHelper.Clamp(
                ball.X - 40,
                width * 0.42f,
                width * 0.72f
            );

            float y = MathHelper.Lerp(
                height / 2f,
                ball.Y,
                0.45f
            );

            return ClampToField(new Vector2(x, y), width, height);
        }

        private Vector2 AvoidEnemiesForward(Vector2 target, List<EnemyFootball> enemies, int width, int height)
        {
            Vector2 result = target;

            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(result, enemy.Position());

                if (distance < 85f)
                {
                    Vector2 away = result - enemy.Position();

                    if (away.Length() > 1f)
                    {
                        away.Normalize();
                        result += away * 40f;
                    }
                }
            }

            return ClampToField(result, width, height);
        }

        private void SmoothForwardMove(Vector2 desiredTarget)
        {
            if (Vector2.Distance(_smoothTargetPosition, desiredTarget) > 35f)
            {
                _smoothTargetPosition = Vector2.Lerp(
                    _smoothTargetPosition,
                    desiredTarget,
                    0.035f
                );
            }

            if (Vector2.Distance(Position(), _smoothTargetPosition) < 6f)
                return;

            _targetPosition = _smoothTargetPosition;
            MoveToTarget(_targetPosition);
        }
    }

}
