using Microsoft.Xna.Framework;

namespace FifaFootballGame.Models
{
    public class EnemyTeamAI
    {
        private List<EnemyFootball> _enemies;
        private List<FootballPlayer> _players;
        private Ball _ball;

        private Random _random = new Random();

        private AIPlayType _currentPlay = AIPlayType.None;
        private float _playTimer = 0;

        public EnemyTeamAI(
            List<EnemyFootball> enemies,
            List<FootballPlayer> players,
            Ball ball)
        {
            _enemies = enemies;
            _players = players;
            _ball = ball;
        }

        public void Update(int width, int height)
        {
            _playTimer++;

            if (_ball.IsPassing() || _ball.IsShooting())
            {
                AssignShape(width, height);
                return;
            }

            if (_ball.IsEnemyOwner())
            {
                RunAttack(width, height);
                return;
            }

            if (_ball.IsPlayerOwner())
            {
                RunDefence(width, height);
                return;
            }

            RunLooseBall(width, height);
        }

        private void RunAttack(int width, int height)
        {
            EnemyFootball owner = _ball.GetEnemyOwner();

            if (owner == null)
                return;

            if (_currentPlay == AIPlayType.None || _playTimer > 180)
                SelectAttackPlay(owner, width, height);

            if (_currentPlay == AIPlayType.ThirdManCombination)
            {
                AssignThirdManCombination(owner, width, height);
                return;
            }

            if (_currentPlay == AIPlayType.WallPass)
            {
                AssignWallPass(owner, width, height);
                return;
            }

            if (_currentPlay == AIPlayType.WingAttack)
            {
                AssignWingAttack(owner, width, height);
                return;
            }

            AssignSimpleAttack(owner, width, height);
        }

        private void SelectAttackPlay(EnemyFootball owner, int width, int height)
        {
            _playTimer = 0;

            float x = owner.Position().X;

            if (x > width * 0.65f)
            {
                _currentPlay = AIPlayType.BuildUp;
                return;
            }

            int value = _random.Next(0, 100);

            if (value < 35)
                _currentPlay = AIPlayType.ThirdManCombination;
            else if (value < 60)
                _currentPlay = AIPlayType.WallPass;
            else if (value < 80)
                _currentPlay = AIPlayType.WingAttack;
            else
                _currentPlay = AIPlayType.CounterAttack;
        }

        private void AssignThirdManCombination(EnemyFootball owner, int width, int height)
        {
            EnemyFootball support = FindNearestTeammate(owner);
            EnemyFootball runner = FindBestRunner(owner, support);

            if (support == null || runner == null)
            {
                AssignSimpleAttack(owner, width, height);
                return;
            }

            owner.SetTask(new AITask
            {
                Type = AITaskType.CarryBall,
                Target = new Vector2(owner.Position().X - 40, owner.Position().Y)
            });

            support.SetTask(new AITask
            {
                Type = AITaskType.SupportPass,
                Target = owner.Position() + new Vector2(-80, 40),
                PassTarget = runner
            });

            runner.SetTask(new AITask
            {
                Type = AITaskType.ThirdManRun,
                Target = new Vector2(width * 0.18f, height / 2f + _random.Next(-90, 91))
            });

            foreach (var enemy in _enemies)
            {
                if (enemy == owner || enemy == support || enemy == runner)
                    continue;

                enemy.SetTask(new AITask
                {
                    Type = AITaskType.HoldPosition,
                    Target = GetRoleShapePosition(enemy, width, height)
                });
            }
        }

        private void AssignWallPass(EnemyFootball owner, int width, int height)
        {
            EnemyFootball support = FindNearestTeammate(owner);

            if (support == null)
            {
                AssignSimpleAttack(owner, width, height);
                return;
            }

            owner.SetTask(new AITask
            {
                Type = AITaskType.RunIntoSpace,
                Target = owner.Position() + new Vector2(-110, _random.Next(-55, 56))
            });

            support.SetTask(new AITask
            {
                Type = AITaskType.SupportPass,
                Target = owner.Position() + new Vector2(-60, 35),
                PassTarget = owner
            });

            foreach (var enemy in _enemies)
            {
                if (enemy == owner || enemy == support)
                    continue;

                enemy.SetTask(new AITask
                {
                    Type = AITaskType.HoldPosition,
                    Target = GetRoleShapePosition(enemy, width, height)
                });
            }
        }

        private void AssignWingAttack(EnemyFootball owner, int width, int height)
        {
            EnemyFootball winger = FindWinger();

            if (winger == null)
            {
                AssignSimpleAttack(owner, width, height);
                return;
            }

            owner.SetTask(new AITask
            {
                Type = AITaskType.CarryBall,
                Target = owner.Position() + new Vector2(-50, 0),
                PassTarget = winger
            });

            winger.SetTask(new AITask
            {
                Type = AITaskType.RunIntoSpace,
                Target = new Vector2(width * 0.25f, winger.Position().Y < height / 2 ? 70 : height - 100)
            });

            foreach (var enemy in _enemies)
            {
                if (enemy == owner || enemy == winger)
                    continue;

                enemy.SetTask(new AITask
                {
                    Type = AITaskType.HoldPosition,
                    Target = GetRoleShapePosition(enemy, width, height)
                });
            }
        }

        private void AssignSimpleAttack(EnemyFootball owner, int width, int height)
        {
            Vector2 goal = new Vector2(10, height / 2f);

            owner.SetTask(new AITask
            {
                Type = AITaskType.CarryBall,
                Target = goal
            });

            foreach (var enemy in _enemies)
            {
                if (enemy == owner)
                    continue;

                enemy.SetTask(new AITask
                {
                    Type = AITaskType.RunIntoSpace,
                    Target = GetRoleAttackPosition(enemy, width, height)
                });
            }
        }

        private void RunDefence(int width, int height)
        {
            FootballPlayer owner = _ball.GetOwner();

            foreach (var enemy in _enemies)
            {
                if (enemy.GetRole() == "goalkeeper")
                {
                    enemy.SetTask(new AITask
                    {
                        Type = AITaskType.Goalkeeper,
                        Target = new Vector2(width - 55, MathHelper.Clamp(_ball.GetPositionBall().Y, height / 2f - 120, height / 2f + 120))
                    });

                    continue;
                }

                float distance = Vector2.Distance(enemy.Position(), owner.GetPosition());

                if (distance < 140)
                {
                    enemy.SetTask(new AITask
                    {
                        Type = AITaskType.Press,
                        Target = owner.GetPosition()
                    });
                }
                else
                {
                    enemy.SetTask(new AITask
                    {
                        Type = AITaskType.HoldPosition,
                        Target = GetRoleShapePosition(enemy, width, height)
                    });
                }
            }
        }

        private void RunLooseBall(int width, int height)
        {
            EnemyFootball nearest = FindNearestToBall();

            foreach (var enemy in _enemies)
            {
                if (enemy == nearest)
                {
                    enemy.SetTask(new AITask
                    {
                        Type = AITaskType.ChaseBall,
                        Target = _ball.GetPositionBall()
                    });
                }
                else
                {
                    enemy.SetTask(new AITask
                    {
                        Type = AITaskType.HoldPosition,
                        Target = GetRoleShapePosition(enemy, width, height)
                    });
                }
            }
        }

        private void AssignShape(int width, int height)
        {
            foreach (var enemy in _enemies)
            {
                enemy.SetTask(new AITask
                {
                    Type = AITaskType.HoldPosition,
                    Target = GetRoleShapePosition(enemy, width, height)
                });
            }
        }

        private Vector2 GetRoleShapePosition(EnemyFootball enemy, int width, int height)
        {
            Vector2 ball = _ball.GetPositionBall();

            if (enemy.GetRole() == "forward")
                return new Vector2(width * 0.58f, MathHelper.Lerp(height / 2f, ball.Y, 0.35f));

            if (enemy.GetRole() == "leftMidfielder")
                return new Vector2(width * 0.70f, MathHelper.Lerp(height * 0.75f, ball.Y, 0.25f));

            if (enemy.GetRole() == "rightMidfielder")
                return new Vector2(width * 0.70f, MathHelper.Lerp(height * 0.25f, ball.Y, 0.25f));

            if (enemy.GetRole() == "defender")
                return new Vector2(width * 0.84f, MathHelper.Lerp(height / 2f, ball.Y, 0.45f));

            return new Vector2(width - 55, height / 2f);
        }

        private Vector2 GetRoleAttackPosition(EnemyFootball enemy, int width, int height)
        {
            if (enemy.GetRole() == "forward")
                return new Vector2(width * 0.18f, height / 2f);

            if (enemy.GetRole() == "leftMidfielder")
                return new Vector2(width * 0.35f, height * 0.78f);

            if (enemy.GetRole() == "rightMidfielder")
                return new Vector2(width * 0.35f, height * 0.22f);

            if (enemy.GetRole() == "defender")
                return new Vector2(width * 0.58f, height / 2f);

            return new Vector2(width - 55, height / 2f);
        }

        private EnemyFootball FindNearestToBall()
        {
            EnemyFootball nearest = null;
            float min = float.MaxValue;

            foreach (var enemy in _enemies)
            {
                float distance = Vector2.Distance(enemy.Position(), _ball.GetPositionBall());

                if (distance < min)
                {
                    min = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private EnemyFootball FindNearestTeammate(EnemyFootball owner)
        {
            EnemyFootball nearest = null;
            float min = float.MaxValue;

            foreach (var enemy in _enemies)
            {
                if (enemy == owner || enemy.GetRole() == "goalkeeper")
                    continue;

                float distance = Vector2.Distance(owner.Position(), enemy.Position());

                if (distance < min)
                {
                    min = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private EnemyFootball FindBestRunner(EnemyFootball owner, EnemyFootball support)
        {
            EnemyFootball best = null;
            float bestScore = float.MinValue;

            foreach (var enemy in _enemies)
            {
                if (enemy == owner || enemy == support)
                    continue;

                if (enemy.GetRole() == "goalkeeper")
                    continue;

                float forward = owner.Position().X - enemy.Position().X;
                float score = forward + _random.Next(-20, 21);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = enemy;
                }
            }

            return best;
        }

        private EnemyFootball FindWinger()
        {
            foreach (var enemy in _enemies)
            {
                if (enemy.GetRole() == "leftMidfielder" || enemy.GetRole() == "rightMidfielder")
                    return enemy;
            }

            return null;
        }
    }
}