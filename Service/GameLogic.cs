using FifaFootballGame.Models;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace FifaFootballGame.Service
{
    public class GameLogic
    {
        private Ball _gameBall;
        private MatchState _matchState;

        private int _width = 900;
        private int _height = 600;

        private bool _waitingForKickOffAfterGoal;

        public bool NeedResetPositions { get; private set; } //переменная для хранения информации о сбросе позиций
        public bool LastGoalForPlayer { get; private set; } 

        public GameLogic(Ball ball, MatchState matchState)
        {
            _gameBall = ball;
            _matchState = matchState;
        }

        public void Update(float deltaTime)
        {
            NeedResetPositions = false; 

            _matchState.UpdateTime(deltaTime);
            _matchState.UpdatePause(deltaTime);

            if (_waitingForKickOffAfterGoal && !_matchState.IsPausedAfterEvent)
            {
                _waitingForKickOffAfterGoal = false;

                // После гола разводит команда, которая пропустила
                _matchState.StartKickOff(!LastGoalForPlayer);
                NeedResetPositions = true;
                return;
            }

            if (_matchState.IsPausedAfterEvent || _matchState.IsKickOff)
                return;

            CheckGoal();
            CheckOut();
        }

        private void CheckGoal()
        {
            Vector2 ball = _gameBall.GetPositionBall();

            bool inGoalY = ball.Y >= 240 && ball.Y <= 360;

            if (ball.X <= 0 && inGoalY)
            {
                _matchState.EnemyScore++;
                LastGoalForPlayer = false;
                ResetAfterGoal("Гол противника!");
            }
            else if (ball.X >= _width && inGoalY)
            {
                _matchState.PlayerScore++;
                LastGoalForPlayer = true;
                ResetAfterGoal("Гол!");
            }
        }

        private void CheckOut()
        {
            Vector2 ball = _gameBall.GetPositionBall();

            if (ball.Y <= 0 || ball.Y >= _height)
            {
                ResetAfterEvent("Аут", RestartType.Out);
                return;
            }

            bool outsideLeft = ball.X <= 0;
            bool outsideRight = ball.X >= _width;
            bool notGoalY = ball.Y < 240 || ball.Y > 360;

            if ((outsideLeft || outsideRight) && notGoalY)
            {
                ResetAfterEvent("Угловой / удар от ворот", RestartType.Corner);
            }
        }

        private void ResetAfterGoal(string text)
        {
            MessageBox.Show(text);
            _matchState.StartPause(text, RestartType.Goal);

            _gameBall.DropOwner();
            _gameBall.StartPositionBall();

            _waitingForKickOffAfterGoal = true;
            NeedResetPositions = true;
        }

        private void ResetAfterEvent(string text, RestartType type)
        {
            _matchState.StartPause(text, type);

            _gameBall.DropOwner();
            _gameBall.StartPositionBall();

            NeedResetPositions = true;
        }
    }
}