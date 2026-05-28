using FifaFootballGame.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using SharpDX.Direct3D9;
using System.Windows.Input;

namespace FifaFootballGame.Models
{
    class Defender : FootballPlayer
    {
        private int _width;
        private int _height;
        private Vector2 _startPosition;

        private Vector2 _smoothTargetPosition;
        private const float TARGET_DEAD_ZONE = 6f; //изменить на 6f
        private const float TARGET_SMOOTHING = 0.07f;

        public Defender(Texture2D texture, Vector2 startPosition, Ball ball)
            : base(texture, startPosition, ball)
        {
            _position = "Defender";
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
                DefendsMove();
        }

        public override void AttackMove()
        {
            Vector2 ballPos = _gameBall.GetPositionBall();

            float pushUpX = MathHelper.Clamp(
                ballPos.X - 220,
                _startPosition.X,
                _width * 0.45f
            );

            Vector2 desiredTarget = new Vector2(
                pushUpX,
                _startPosition.Y
            );

            // Если мяч рядом с его зоной — защитник немного страхует
            desiredTarget.Y = MathHelper.Lerp(
                desiredTarget.Y,
                ballPos.Y,
                0.18f
            );

            desiredTarget = ClampToField(desiredTarget, _width, _height);

            SmoothMove(desiredTarget);
        }

        public override void DefendsMove()
        {
            Vector2 ballPos = _gameBall.GetPositionBall();

            float danger = MathHelper.Clamp(
                1f - ballPos.X / (_width * 0.65f),
                0f,
                1f
            );

            float targetX = MathHelper.Lerp(
                _startPosition.X,
                ballPos.X - 70,
                danger
            );

            targetX = MathHelper.Clamp(
                targetX,
                _width * 0.08f,
                _startPosition.X + 35
            );

            Vector2 desiredTarget = new Vector2(
                targetX,
                MathHelper.Lerp(_startPosition.Y, ballPos.Y, 0.55f)
            );

            desiredTarget = ClampToField(desiredTarget, _width, _height);

            SmoothMove(desiredTarget);
        }

        private void SmoothMove(Vector2 desiredTarget)
        {
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

        public override void SmartMove(int width,int height,List<FootballPlayer> team,List<EnemyFootball> enemies,TeamTacticContext context)
        {
            _width = width;
            _height = height;

            if (context.BallIsFree && context.NearestToBall == this)
            {
                _targetPosition = context.BallPosition;
                MoveToTarget(_targetPosition);
                return;
            }

            if (context.HasBall)
            {
                Vector2 safeSupport = new Vector2(
                    MathHelper.Clamp(context.BallPosition.X - 210, _startPosition.X, width * 0.48f),
                    MathHelper.Lerp(_startPosition.Y, context.BallPosition.Y, 0.25f)
                );

                safeSupport = ClampToField(safeSupport, width, height);
                SmoothMove(safeSupport);
                return;
            }

            if (context.EnemyHasBall)
            {
                DefendsMove();
                return;
            }

            MovePlayers(width, height, enemies);
        }
    }
}