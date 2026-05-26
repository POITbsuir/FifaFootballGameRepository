namespace FifaFootballGame.Models
{
    public enum AITaskType
    {
        HoldPosition,
        Press,
        ChaseBall,
        CarryBall,
        RunIntoSpace,
        SupportPass,
        ThirdManRun,
        Shoot,
        Goalkeeper
    }

    public enum AIPlayType
    {
        None,
        BuildUp,
        WallPass,
        ThirdManCombination,
        WingAttack,
        CounterAttack,
        HighPress
    }
}