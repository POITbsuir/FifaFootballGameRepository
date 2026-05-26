using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FifaFootballGame.Models
{
    public class AIFootballAgent : EnemyFootball
    {
        private Random _random = new Random();

        private Vector2 _smoothTarget;
        private Vector2 _lastDecisionTarget;

        private float _actionCooldown;
        private float _decisionCooldown;

        private const float TARGET_SMOOTHING = 0.055f;
        private const float TARGET_CHANGE_DEADZONE = 22f;

        public AIFootballAgent(Texture2D texture, Vector2 currentPositionAI, Ball ball, string position)
            : base(texture, currentPositionAI, ball, position)
        {
            _smoothTarget = currentPositionAI;
            _lastDecisionTarget = currentPositionAI;

            if (position == "forward")
            {
                _speed = 3;
                _dribling = 86;
            }
            else if (position == "goalkeeper")
            {
                _speed = 3;
                _dribling = 55;
            }
            else if (position == "defender")
            {
                _speed = 2;
                _dribling = 74;
            }
            else
            {
                _speed = 2;
                _dribling = 82;
            }
        }

        public override void MoveAI(
            int width,
            int height,
            List<FootballPlayer> players,
            List<EnemyFootball> enemies)
        {
            _actionCooldown--;
            _decisionCooldown--;

            if (CurrentTask == null)
                return;

            if (_position == "goalkeeper")
            {
                PlayGoalkeeper(width, height);
                return;
            }

            ExecuteTask(width, height, players, enemies);
        }

        private void ExecuteTask(
            int width,
            int height,
            List<FootballPlayer> players,
            List<EnemyFootball> enemies)
        {
            if (CurrentTask.Type == AITaskType.Press)
            {
                PressOwner(width, height, players);
                return;
            }

            if (CurrentTask.Type == AITaskType.ChaseBall)
            {
                ChaseBall(width, height);
                return;
            }

            if (CurrentTask.Type == AITaskType.CarryBall)
            {
                PlayWithBall(width, height, players, enemies);
                return;
            }

            if (CurrentTask.Type == AITaskType.SupportPass)
            {
                SupportAndMaybePass(width, height, players, enemies);
                return;
            }

            if (CurrentTask.Type == AITaskType.RunIntoSpace || CurrentTask.Type == AITaskType.ThirdManRun)
            {
                RunIntoSpace(width, height, players);
                return;
            }

            MoveSmartTo(CurrentTask.Target, width, height);
        }

        private void PlayWithBall(
            int width,
            int height,
            List<FootballPlayer> players,
            List<EnemyFootball> enemies)
        {
            if (!IsControlBall)
            {
                MoveSmartTo(CurrentTask.Target, width, height);
                return;
            }

            float shootScore = EvaluateShot(width, height, players);
            EnemyFootball bestPass = FindBestPassTarget(width, height, players, enemies, out float passScore);
            float carryScore = EvaluateCarry(width, height, players);

            if (_actionCooldown <= 0)
            {
                if (shootScore > 0.72f)
                {
                    ShootAtGoal(width, height);
                    return;
                }

                if (bestPass != null && passScore > shootScore && passScore > 0.58f)
                {
                    PassToRunner(bestPass, width, height);
                    return;
                }
            }

            Vector2 carryTarget = GetDribbleTarget(width, height, players);
            MoveSmartTo(carryTarget, width, height);
        }

        private float EvaluateShot(int width, int height, List<FootballPlayer> players)
        {
            Vector2 goal = new Vector2(10, height / 2f);

            float distance = Vector2.Distance(Position(), goal);
            float distanceScore = 1f - MathHelper.Clamp(distance / 420f, 0f, 1f);

            float angleScore = 1f - MathHelper.Clamp(Math.Abs(Position().Y - height / 2f) / 260f, 0f, 1f);

            float pressure = GetPressure(players, 95f);
            float pressurePenalty = MathHelper.Clamp(pressure / 3f, 0f, 1f);

            float score =
                distanceScore * 0.50f +
                angleScore * 0.35f -
                pressurePenalty * 0.30f;

            return MathHelper.Clamp(score, 0f, 1f);
        }

        private float EvaluateCarry(int width, int height, List<FootballPlayer> players)
        {
            float progressScore = 1f - MathHelper.Clamp(Position().X / width, 0f, 1f);
            float pressure = GetPressure(players, 80f);

            float score = progressScore * 0.65f - pressure * 0.12f;

            return MathHelper.Clamp(score, 0f, 1f);
        }

        private EnemyFootball FindBestPassTarget(
            int width,
            int height,
            List<FootballPlayer> players,
            List<EnemyFootball> enemies,
            out float bestScore)
        {
            EnemyFootball best = null;
            bestScore = 0f;

            foreach (var mate in enemies)
            {
                if (mate == this)
                    continue;

                if (mate.GetRole() == "goalkeeper")
                    continue;

                Vector2 predictedPoint = PredictRunPoint(mate, width, height);
                float score = PassPerceptronScore(mate, predictedPoint, players, width, height);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = mate;
                }
            }

            return best;
        }

        private float PassPerceptronScore(
            EnemyFootball target,
            Vector2 predictedPoint,
            List<FootballPlayer> players,
            int width,
            int height)
        {
            float distance = Vector2.Distance(Position(), target.Position());
            float distanceFeature = 1f - MathHelper.Clamp(Math.Abs(distance - 150f) / 220f, 0f, 1f);

            float forwardFeature = MathHelper.Clamp((Position().X - predictedPoint.X) / width, -1f, 1f);

            float openness = GetOpenSpaceScore(predictedPoint, players);

            float laneSafety = GetPassingLaneSafety(Position(), predictedPoint, players);

            float roleBonus = target.GetRole() == "forward" ? 0.12f : 0f;

            float raw =
                distanceFeature * 0.28f +
                forwardFeature * 0.25f +
                openness * 0.30f +
                laneSafety * 0.32f +
                roleBonus -
                0.15f;

            return Sigmoid(raw * 3.2f);
        }

        private float Sigmoid(float x)
        {
            return 1f / (1f + (float)Math.Exp(-x));
        }

        private Vector2 PredictRunPoint(EnemyFootball mate, int width, int height)
        {
            Vector2 pos = mate.Position();

            float runX = pos.X - 75f;

            if (mate.GetRole() == "forward")
                runX -= 45f;

            float runY = pos.Y;

            if (mate.GetRole() == "leftMidfielder")
                runY -= 35f;

            if (mate.GetRole() == "rightMidfielder")
                runY += 35f;

            return new Vector2(
                MathHelper.Clamp(runX, 35, width - 45),
                MathHelper.Clamp(runY, 35, height - 45)
            );
        }

        private float GetOpenSpaceScore(Vector2 point, List<FootballPlayer> players)
        {
            float nearest = float.MaxValue;

            foreach (var player in players)
            {
                float d = Vector2.Distance(point, player.GetPosition());

                if (d < nearest)
                    nearest = d;
            }

            return MathHelper.Clamp(nearest / 170f, 0f, 1f);
        }

        private float GetPassingLaneSafety(Vector2 from, Vector2 to, List<FootballPlayer> players)
        {
            float safety = 1f;

            foreach (var player in players)
            {
                float distanceToLine = DistancePointToSegment(player.GetPosition(), from, to);

                if (distanceToLine < 45f)
                    safety -= 0.28f;
            }

            return MathHelper.Clamp(safety, 0f, 1f);
        }

        private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;

            float abLengthSquared = ab.LengthSquared();

            if (abLengthSquared <= 0.001f)
                return Vector2.Distance(p, a);

            float t = Vector2.Dot(p - a, ab) / abLengthSquared;
            t = MathHelper.Clamp(t, 0f, 1f);

            Vector2 projection = a + ab * t;

            return Vector2.Distance(p, projection);
        }

        private void ShootAtGoal(int width, int height)
        {
            Vector2 goal = new Vector2(10, height / 2f);

            Vector2 dir = goal - Position();
            dir.Y += _random.Next(-30, 31);

            _ball.Shoot(dir, 9.5f);

            IsControlBall = false;
            _actionCooldown = 120;
        }

        private void PassToRunner(EnemyFootball target, int width, int height)
        {
            Vector2 predicted = PredictRunPoint(target, width, height);
            Vector2 dir = predicted - Position();

            _ball.Shoot(dir, 5.8f);

            IsControlBall = false;
            _actionCooldown = 110; //парог распосовки
        }

        private Vector2 GetDribbleTarget(int width, int height, List<FootballPlayer> players)
        {
            Vector2 target = Position() + new Vector2(-65, 0);

            FootballPlayer nearest = FindNearestPlayer(players);

            if (nearest != null)
            {
                Vector2 away = Position() - nearest.GetPosition();

                if (away.Length() > 1f)
                {
                    away.Normalize();
                    target += away * 45f;
                }
            }

            if (Position().Y < height * 0.25f)
                target.Y += 30;

            if (Position().Y > height * 0.75f)
                target.Y -= 30;

            return ClampVector(target, width, height);
        }

        private void SupportAndMaybePass(
            int width,
            int height,
            List<FootballPlayer> players,
            List<EnemyFootball> enemies)
        {
            MoveSmartTo(CurrentTask.Target, width, height);

            if (!IsControlBall)
                return;

            if (_actionCooldown > 0)
                return;

            EnemyFootball best = FindBestPassTarget(width, height, players, enemies, out float score);

            if (best != null && score > 0.55f)
            {
                PassToRunner(best, width, height);
                return;
            }

            PlayWithBall(width, height, players, enemies);
        }

        private void RunIntoSpace(int width, int height, List<FootballPlayer> players)
        {
            Vector2 target = CurrentTask.Target;

            foreach (var player in players)
            {
                float d = Vector2.Distance(target, player.GetPosition());

                if (d < 80f)
                {
                    Vector2 away = target - player.GetPosition();

                    if (away.Length() > 1f)
                    {
                        away.Normalize();
                        target += away * 35f;
                    }
                }
            }

            MoveSmartTo(target, width, height);
        }

        private void PressOwner(int width, int height, List<FootballPlayer> players)
        {
            FootballPlayer owner = _ball.GetOwner();

            if (owner == null)
            {
                MoveSmartTo(CurrentTask.Target, width, height);
                return;
            }

            Vector2 pressTarget = owner.GetPosition();

            Vector2 coverGoal = new Vector2(width, height / 2f);
            Vector2 blockLine = pressTarget - coverGoal;

            if (blockLine.Length() > 1f)
            {
                blockLine.Normalize();
                pressTarget += blockLine * 18f;
            }

            MoveSmartTo(pressTarget, width, height);
            TryTackle(players);
        }

        private void ChaseBall(int width, int height)
        {
            Vector2 ball = _ball.GetPositionBall();

            MoveSmartTo(ball, width, height);

            if (!_ball.IsPassing() && !_ball.IsShooting())
            {
                if (Vector2.Distance(Position(), ball) < 32)
                    _ball.SetEnemyOwner(this);
            }
        }

        private void PlayGoalkeeper(int width, int height)
        {
            Vector2 ball = _ball.GetPositionBall();

            Vector2 target = new Vector2(
                width - 55,
                MathHelper.Clamp(ball.Y - 16, height / 2f - 120, height / 2f + 120)
            );

            if (ball.X > width * 0.82f)
                target.X = width - 70;

            MoveSmartTo(target, width, height);

            if (!_ball.IsPassing() && !_ball.IsShooting())
            {
                if (Vector2.Distance(Position(), ball) < 38)
                    _ball.SetEnemyOwner(this);
            }
        }

        private void TryTackle(List<FootballPlayer> players)
        {
            if (_ball.IsPassing() || _ball.IsShooting())
                return;

            FootballPlayer owner = _ball.GetOwner();

            if (owner == null)
                return;

            float distance = Vector2.Distance(Position(), owner.GetPosition());

            if (distance > 36)
                return;

            int enemyPower = _dribling + _speed + _headingGame;
            int playerPower = owner.GetValueDribling() + owner.GetValueSpeed() + owner.GetValueHeadingGame();

            int chance = 28 + (enemyPower - playerPower) / 4;

            if (GetPressure(players, 60f) > 1.5f)
                chance += 8;

            chance = MathHelper.Clamp(chance, 10, 58);

            if (_random.Next(0, 100) < chance)
                _ball.SetEnemyOwner(this);
        }

        private float GetPressure(List<FootballPlayer> players, float radius)
        {
            float pressure = 0;

            foreach (var player in players)
            {
                float d = Vector2.Distance(Position(), player.GetPosition());

                if (d < radius)
                    pressure += 1f - d / radius;
            }

            return pressure;
        }

        private FootballPlayer FindNearestPlayer(List<FootballPlayer> players)
        {
            FootballPlayer nearest = null;
            float min = float.MaxValue;

            foreach (var player in players)
            {
                float d = Vector2.Distance(Position(), player.GetPosition());

                if (d < min)
                {
                    min = d;
                    nearest = player;
                }
            }

            return nearest;
        }

        private Vector2 ClampVector(Vector2 value, int width, int height)
        {
            value.X = MathHelper.Clamp(value.X, 0, width - 32);
            value.Y = MathHelper.Clamp(value.Y, 0, height - 32);

            return value;
        }

        private void MoveSmartTo(Vector2 target, int width, int height)
        {
            target = ClampVector(target, width, height);

            if (_decisionCooldown <= 0)
            {
                if (Vector2.Distance(_lastDecisionTarget, target) > TARGET_CHANGE_DEADZONE)
                {
                    _lastDecisionTarget = target;
                    _decisionCooldown = 12;
                }
            }

            _smoothTarget = Vector2.Lerp(
                _smoothTarget,
                _lastDecisionTarget,
                TARGET_SMOOTHING
            );

            Vector2 dir = _smoothTarget - _currentPositionAI;

            if (dir.Length() < 4f)
                return;

            dir.Normalize();

            _currentPositionAI += dir * _speed;

            _currentPositionAI = ClampVector(_currentPositionAI, width, height);
        }
    }
}