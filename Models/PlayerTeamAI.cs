using Microsoft.Xna.Framework;

namespace FifaFootballGame.Models
{
    public class PlayerTeamAI
    {
        private List<FootballPlayer> _team;
        private List<EnemyFootball> _enemies;
        private Ball _ball;

        public PlayerTeamAI(List<FootballPlayer> team, List<EnemyFootball> enemies, Ball ball)
        {
            _team = team;
            _enemies = enemies;
            _ball = ball;
        }

        public TeamTacticContext BuildContext(int width, int height)
        {
            TeamTacticContext context = new TeamTacticContext();

            context.BallPosition = _ball.GetPositionBall();
            context.HasBall = _ball.IsPlayerOwner();
            context.EnemyHasBall = _ball.IsEnemyOwner();
            context.BallIsFree = !_ball.IsControlled() && !_ball.IsPassing() && !_ball.IsShooting();
            context.Owner = _ball.GetOwner();
            context.NearestToBall = FindNearestToBall();

            context.AttackProgress = MathHelper.Clamp(
                context.BallPosition.X / width,
                0f,
                1f
            );

            return context;
        }

        private FootballPlayer FindNearestToBall()
        {
            FootballPlayer nearest = null;
            float minDistance = float.MaxValue;

            foreach (var player in _team)
            {
                float distance = Vector2.Distance(
                    player.GetPosition() + new Vector2(16, 16),
                    _ball.GetPositionBall()
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = player;
                }
            }

            return nearest;
        }
    }
}