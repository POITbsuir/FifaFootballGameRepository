using FifaFootballGame.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Midfielder : FootballPlayer
{
    private int _width;
    private int _height;
    private Vector2 _startPosition;

    private Vector2 _smoothTargetPosition;
    private float _decisionTimer = 0f;
    private const float DECISION_INTERVAL = 40f;
    private const float TARGET_DEAD_ZONE = 7f;
    private const float TARGET_SMOOTHING = 0.08f;

    private int _laneSide;
    private Random _random = new Random();

    public Midfielder(Texture2D texture, Vector2 startPosition, Ball ball, int laneSide) : base(texture, startPosition, ball)
    {
        _position = "Midfielder";
        _speed = 4;
        _dribling = 85;
        _startPosition = startPosition;
        _smoothTargetPosition = startPosition;

        _laneSide = laneSide; // -1 верхний, 1 нижний
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

        _decisionTimer++;

        if (_decisionTimer >= DECISION_INTERVAL)
        {
            _decisionTimer = 0;

            // Иногда меняет коридор, но не каждый кадр
            if (_random.Next(0, 100) < 20)
                _laneSide *= -1;
        }

        float attackProgress = MathHelper.Clamp(ballPos.X / _width, 0f, 1f);

        float supportDistanceX = MathHelper.Lerp(90, 45, attackProgress);
        float supportDistanceY = MathHelper.Lerp(100, 145, attackProgress);

        Vector2 desiredTarget = new Vector2(
            ballPos.X - supportDistanceX,
            ballPos.Y + _laneSide * supportDistanceY
        );

        // Если мяч на фланге, полузащитник смещается ближе к центру,
        // чтобы не залипать у линии
        float centerY = _height / 2f;
        desiredTarget.Y = MathHelper.Lerp(desiredTarget.Y, centerY, 0.18f);

        // Не забегаем выше форварда слишком сильно
        desiredTarget.X = MathHelper.Clamp(
            desiredTarget.X,
            _width * 0.25f,
            _width * 0.72f
        );

        desiredTarget = ClampToField(desiredTarget, _width, _height);

        SmoothMove(desiredTarget);
    }

    public override void DefendsMove()
    {
        Vector2 ballPos = _gameBall.GetPositionBall();

        float defendX = MathHelper.Clamp(
            ballPos.X - 120,
            _width * 0.25f,
            _startPosition.X + 30
        );

        Vector2 desiredTarget = new Vector2(
            defendX,
            ballPos.Y + _laneSide * 75
        );

        // Полузащитник держит свою зону, а не просто летит за мячом
        desiredTarget.Y = MathHelper.Lerp(
            desiredTarget.Y,
            _startPosition.Y,
            0.35f
        );

        desiredTarget = ClampToField(desiredTarget, _width, _height);

        SmoothMove(desiredTarget);
    }

    private void SmoothMove(Vector2 desiredTarget)
    {
        float distanceToNewTarget = Vector2.Distance(_smoothTargetPosition, desiredTarget);

        if (distanceToNewTarget > 35f)
        {
            _smoothTargetPosition = Vector2.Lerp(
                _smoothTargetPosition,
                desiredTarget,
                0.035f
            );
        }

        if (Vector2.Distance(GetPosition(), _smoothTargetPosition) < 6f)
            return;

        _targetPosition = _smoothTargetPosition;
        MoveToTarget(_targetPosition);
    }

    private void CarryLikeMidfielder(int width, int height, List<EnemyFootball> enemies)
    {
        Vector2 ball = _gameBall.GetPositionBall();

        Vector2 target = new Vector2(
            MathHelper.Clamp(ball.X + 55, width * 0.35f, width * 0.72f),
            MathHelper.Clamp(ball.Y + _laneSide * 35, 40, height - 40)
        );

        target = AvoidEnemies(target, enemies, width, height);
        SmoothMove(target);
    }

    private Vector2 GetMidfielderSupportPoint(int width, int height, TeamTacticContext context)
    {
        float progress = MathHelper.Clamp(context.BallPosition.X / width, 0f, 1f);

        float x = MathHelper.Lerp(width * 0.35f, width * 0.68f, progress);
        float y;

        if (_laneSide < 0)
            y = MathHelper.Lerp(height * 0.28f, height * 0.38f, progress);
        else
            y = MathHelper.Lerp(height * 0.72f, height * 0.62f, progress);

        if (context.BallPosition.Y < height * 0.35f && _laneSide > 0)
            y = height * 0.58f;

        if (context.BallPosition.Y > height * 0.65f && _laneSide < 0)
            y = height * 0.42f;

        return ClampToField(new Vector2(x, y), width, height);
    }

    private Vector2 GetMidfielderDefendPoint(int width, int height, TeamTacticContext context)
    {
        float x = MathHelper.Clamp(
            context.BallPosition.X - 130,
            width * 0.22f,
            _startPosition.X + 45
        );

        float baseY = _laneSide < 0 ? height * 0.35f : height * 0.65f;

        float y = MathHelper.Lerp(
            baseY,
            context.BallPosition.Y,
            0.32f
        );

        return ClampToField(new Vector2(x, y), width, height);
    }

    private Vector2 AvoidEnemies(Vector2 target, List<EnemyFootball> enemies, int width, int height)
    {
        Vector2 result = target;

        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(result, enemy.Position());

            if (distance < 75f)
            {
                Vector2 away = result - enemy.Position();

                if (away.Length() > 1f)
                {
                    away.Normalize();
                    result += away * 35f;
                }
            }
        }

        return ClampToField(result, width, height);
    }

    public override void SmartMove(int width,int height,List<FootballPlayer> team,List<EnemyFootball> enemies, TeamTacticContext context)
    {
        _width = width;
        _height = height;

        if (context.BallIsFree && context.NearestToBall == this)
        {
            SmoothMove(context.BallPosition);
            return;
        }

        if (context.HasBall)
        {
            if (context.Owner == this)
            {
                CarryLikeMidfielder(width, height, enemies);
                return;
            }

            Vector2 supportPoint = GetMidfielderSupportPoint(width, height, context);

            supportPoint = AvoidEnemies(supportPoint, enemies, width, height);
            SmoothMove(supportPoint);
            return;
        }

        if (context.EnemyHasBall)
        {
            Vector2 defendPoint = GetMidfielderDefendPoint(width, height, context);
            SmoothMove(defendPoint);
            return;
        }

        SmoothMove(_startPosition);
    }
}