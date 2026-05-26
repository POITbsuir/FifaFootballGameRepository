using FifaFootballGame.Models;
using FifaFootballGame.Service;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop.Input;
using System.Windows.Forms;

namespace FifaFootballGame.Data
{
    //реализация класса для противника-игрока
    public class SecondFootballPlayer : EnemyFootball
    {
        public bool isActive;

        public SecondFootballPlayer(Texture2D texture, Vector2 currentPositionAI, Ball ball, string position) : base(texture, currentPositionAI, ball, position)
        {
            _textureAI = texture;
            _ball = ball;
            _currentPositionAI = currentPositionAI;
            _isControlBall = false;
            _position = position;
        }
        //реализация движение ai-агентов 
        public new void MoveAI(WpfKeyboard keyboard)
        {
            //логика движения по кнопкам
            var state = keyboard.GetState();

            if (!isActive)
                return;

            //словарь, в котором хранятся направления и делегат, выполняющий действия
            var moves = new Dictionary<Keys, Action>
            {
                { Keys.W, () => _currentPositionAI.Y -= _speed },
                { Keys.S, () => _currentPositionAI.Y += _speed },
                { Keys.A, () => _currentPositionAI.X -= _speed },
                { Keys.D, () => _currentPositionAI.X += _speed },
                

            };

            foreach (var move in moves)
                if (state.IsKeyDown((Microsoft.Xna.Framework.Input.Keys)move.Key))
                    move.Value();
        }

        public void MoveFromNetwork(PlayerInputPacket input)
        {
            if (!isActive)
                return;

            if (input.W)
                _currentPositionAI.Y -= _speed;

            if (input.S)
                _currentPositionAI.Y += _speed;

            if (input.A)
                _currentPositionAI.X -= _speed;

            if (input.D)
                _currentPositionAI.X += _speed;
        }

    }
}
