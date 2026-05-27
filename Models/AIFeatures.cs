namespace FifaFootballGame.Models
{
    public class AIFeatures
    {
        public float DistanceToGoal;
        public float DistanceToNearestPlayer;
        public float PassLaneSafety;
        public float ForwardProgress;
        public float Pressure;
        public float TeammateOpenness;
        public float BallX;
        public float BallY;

        public float[] ToArray()
        {
            return new float[]
            {
                DistanceToGoal,
                DistanceToNearestPlayer,
                PassLaneSafety,
                ForwardProgress,
                Pressure,
                TeammateOpenness,
                BallX,
                BallY,
                1f // bias
            };
        }
    }
}