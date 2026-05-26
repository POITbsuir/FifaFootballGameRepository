namespace FifaFootballGame.Service
{
    public enum RestartType
    {
        None,
        Goal,
        Out,
        Corner,
        KickOff
    }

    public class MatchState
    {
        public int PlayerScore { get; set; }
        public int EnemyScore { get; set; }

        public float MatchTimeSeconds { get; set; }
        public float MaxMatchTimeSeconds { get; set; } = 180f;

        public RestartType RestartType { get; set; } = RestartType.None;
        public string RestartText { get; set; } = "";

        public bool IsPausedAfterEvent { get; set; }
        public float PauseTimer { get; set; }

        public bool IsKickOff { get; set; }
        public bool PlayerKickOff { get; set; }

        public void UpdateTime(float deltaTime)
        {
            if (!IsPausedAfterEvent && !IsKickOff)
                MatchTimeSeconds += deltaTime;
        }

        public void StartPause(string text, RestartType type)
        {
            RestartText = text;
            RestartType = type;
            IsPausedAfterEvent = true;
            PauseTimer = 2.0f;
        }

        public void UpdatePause(float deltaTime)
        {
            if (!IsPausedAfterEvent)
                return;

            PauseTimer -= deltaTime;

            if (PauseTimer <= 0)
            {
                IsPausedAfterEvent = false;
            }
        }

        public void StartKickOff(bool playerKickOff)
        {
            IsKickOff = true;
            PlayerKickOff = playerKickOff;
            RestartType = RestartType.KickOff;
            RestartText = playerKickOff ? "Развод мяча вашей команды" : "Развод мяча противника";
        }

        public void EndKickOff()
        {
            IsKickOff = false;
            RestartType = RestartType.None;
            RestartText = "";
        }
    }
}