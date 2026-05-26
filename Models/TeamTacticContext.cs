using Microsoft.Xna.Framework;

namespace FifaFootballGame.Models
{
    public class TeamTacticContext
    {
        public bool HasBall { get; set; }
        public bool EnemyHasBall { get; set; }
        public bool BallIsFree { get; set; }

        public Vector2 BallPosition { get; set; }
        public Vector2 AttackDirection { get; set; } = new Vector2(1, 0);

        public FootballPlayer Owner { get; set; }
        public FootballPlayer NearestToBall { get; set; }

        public float AttackProgress { get; set; }
    }
}