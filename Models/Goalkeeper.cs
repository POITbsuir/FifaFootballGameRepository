using FifaFootballGame.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using SharpDX.Direct3D9;
using System.Windows.Input;
namespace FifaFootballGame.Models
{
    class Goalkeeper : FootballPlayer
    {
        public Goalkeeper(Texture2D texture, Vector2 startPosition, Ball ball) : base(texture, startPosition, ball)
        {
            _position = "Goalkeeper";
            _workingLeg = "left";
            _speed = 4;
            _dribling = 85;
        }

        public override void SmartMove( int width,int height,List<FootballPlayer> team,List<EnemyFootball> enemies,TeamTacticContext context)
        {
            Vector2 ball = context.BallPosition;

            Vector2 target = new Vector2(
                10,
                MathHelper.Clamp(ball.Y - 16, height / 2f - 120, height / 2f + 120)
            );

            if (context.BallIsFree && Vector2.Distance(GetPosition(), ball) < 90)
                target = ball;

            target = ClampToField(target, width, height);

            _targetPosition = target;
            MoveToTarget(_targetPosition);
        }
    }
}
