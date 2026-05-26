using Microsoft.Xna.Framework;

namespace FifaFootballGame.Models
{
    public class AITask
    {
        public AITaskType Type { get; set; }
        public Vector2 Target { get; set; }
        public EnemyFootball PassTarget { get; set; }
        public float Timer { get; set; }
    }
}   