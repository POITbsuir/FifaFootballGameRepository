namespace FifaFootballGame.Service
{
    public class GameStatePacket
    {
        public float BallX { get; set; }
        public float BallY { get; set; }

        public float ForwardX { get; set; }
        public float ForwardY { get; set; }

        public float LeftMidX { get; set; }
        public float LeftMidY { get; set; }

        public float RightMidX { get; set; }
        public float RightMidY { get; set; }

        public float DefenderX { get; set; }
        public float DefenderY { get; set; }

        public float AiForwardX { get; set; }
        public float AiForwardY { get; set; }

        public float AiLeftMidX { get; set; }
        public float AiLeftMidY { get; set; }

        public float AiRightMidX { get; set; }
        public float AiRightMidY { get; set; }

        public float AiDefenderX { get; set; }
        public float AiDefenderY { get; set; }

        public float AiGoalkeeperX { get; set; }
        public float AiGoalkeeperY { get; set; }
    }
}