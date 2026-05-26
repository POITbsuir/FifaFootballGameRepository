using FifaFootballGame.Data;
using FifaFootballGame.Service;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System.Linq;
namespace FifaFootballGame.Models.GamePresentWindowFolder.GameFolder
{
    public class Game : WpfGame
    {
        //переменные для использования, как константы
        private const int SPEED_REFERI = 1;

        private IGraphicsDeviceService _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        private Ball _gameBall;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;
        private FootballPlayer _footballPlayer;
       
        private Dictionary<(FootballPlayer, FootballPlayer), float> _dictionaryDistance;


        private Texture2D _textureReferi;
        private Vector2 _referiPosition;
        private Texture2D _textureReferiSecond;
        private Vector2 _referiSecondPosition;

        //футболисты
        Forward _forward;
        Midfielder _leftMidfielder;
        Midfielder _rightMidfielder;
        Defender _defender;
        Goalkeeper _goalkeeper;


        //противники
        EnemyFootball _aiForward;
        EnemyFootball _aiLMidfielder;
        EnemyFootball _aiRMidfielder;
        EnemyFootball _aiDefender;
        EnemyFootball _aiGoalkeeper;

        //комагда противников в качестве отдельной структуры
        private EnemyTeamAI _enemyTeamAI;

        //для коианды пользователя
        private PlayerTeamAI _playerTeamAI;

        //статистика игры
        private MatchState _matchState;
        private SpriteFont _font;

        //переменные для рефери
        private bool _directionReferi;

        //переменная, чтобы отдать пас
        private bool _wasPassPressed = false;
        private bool _wasShootPressed = false;

        private List<FootballPlayer> _footballPlayers;
        private List<EnemyFootball> _aiFootballAgents;

        private int _windowWidth = 900;
        private int _radiusCentreCircle = 128;
        private int _windowHeight = 600;

        private bool _wasMousePressed = false;

        private GameLogic _gameLogic;

        //серверная логика, основная для курсача
        private GameServer _gameServer;
        private GameClient _gameClient;

        private bool _isServerGame;
        private bool _isClientGame;

        public Game()
        {
            _dictionaryDistance = new Dictionary<(FootballPlayer, FootballPlayer), float>();
        }

        public Game(GameServer server)
        {
            _dictionaryDistance = new Dictionary<(FootballPlayer, FootballPlayer), float>();
            _gameServer = server;
            _isServerGame = true;
        }

        public Game(GameClient client)
        {
            _dictionaryDistance = new Dictionary<(FootballPlayer, FootballPlayer), float>();
            _gameClient = client;
            _isClientGame = true;
        }
        //метод нажатия на игрока
        private void MouseClick(WpfMouse mouse)
        {
            var mouseState = mouse.GetState();
            bool isPressed = (int)mouseState.LeftButton == 1;

            if (isPressed && !_wasMousePressed)
            {
                foreach (var player in _footballPlayers)
                {
                    Vector2 playerPos = player.GetPosition();

                    if (mouseState.X >= playerPos.X &&
                        mouseState.X <= playerPos.X + 32 &&
                        mouseState.Y >= playerPos.Y &&
                        mouseState.Y <= playerPos.Y + 32)
                    {
                        foreach (var p in _footballPlayers)
                            p.isActive = false;

                        player.isActive = true;
                        break;
                    }
                }
            }

            _wasMousePressed = isPressed;
        }  
        protected override void Initialize()
        {
            _graphicsDeviceManager = new WpfGraphicsDeviceService(this);
            _keyboard = new WpfKeyboard(this);

            _mouse = new WpfMouse(this);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            LoadBall();
            LoadFootballPlayers();
            LoadTextureAI();
            LoadReferi();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_spriteBatch != null && _gameBall != null)
            {
                _spriteBatch.Begin();

                _gameBall.Draw(_spriteBatch);
                _forward.DrawFootballPlayer(_spriteBatch);
                _leftMidfielder.DrawFootballPlayer(_spriteBatch);
                _rightMidfielder.DrawFootballPlayer(_spriteBatch);
                _defender.DrawFootballPlayer(_spriteBatch);
                _goalkeeper.DrawFootballPlayer(_spriteBatch);

                _aiForward.DrawAIPlayers(_spriteBatch);
                _aiLMidfielder.DrawAIPlayers(_spriteBatch);
                _aiRMidfielder.DrawAIPlayers(_spriteBatch);
                _aiDefender.DrawAIPlayers(_spriteBatch);
                _aiGoalkeeper.DrawAIPlayers(_spriteBatch);


                _spriteBatch.Draw(_textureReferi, _referiPosition, Color.Yellow);
                _spriteBatch.Draw(_textureReferiSecond, _referiSecondPosition, Color.Yellow);

                //отрисовка времени, изменю:
                
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            //если кто-то из команды владеет мячом, то команде присваивается значение владения мячом
            if (_footballPlayers.Any(p => p.IsControllBall == true))
            {
                foreach (var player in _footballPlayers)
                    player.IsControllBallTeam = true;
            }
                
            if (_keyboard == null || _gameBall == null)
            {
                base.Update(gameTime);
                return;
            }

            // Если это клиент, он только отправляет нажатия на сервер.
            // Сам игру не считает.
            if (_isClientGame && _gameClient != null)
            {
                SendClientInput();
                base.Update(gameTime);
                return;
            }

            CalculateDistanceAllPlayers();

            MouseClick(_mouse);

            var keyboardState = _keyboard.GetState();

            bool isPassPressed = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P);

            if (isPassPressed && !_wasPassPressed)
            {
                PassToNearestPlayer();
            }
            _wasPassPressed = isPassPressed;
            bool isShootPressed = keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);

            if (isShootPressed && !_wasShootPressed)
            {
                ShootToGoal();
            }

            _wasShootPressed = isShootPressed;
            // Движение твоей команды
            TeamTacticContext playerContext = _playerTeamAI.BuildContext(_windowWidth, _windowHeight);

            foreach (var player in _footballPlayers)
            {
                if (!player.isActive)
                    player.SmartMove(_windowWidth, _windowHeight, _footballPlayers, _aiFootballAgents, playerContext);

                player.MoveCurrentPlayer(_keyboard);
            }


            // Если это серверная игра, двигаем второго игрока по данным клиента
            if (_isServerGame && _gameServer != null)
            {
                var input = _gameServer.LastInput;

                if (_aiForward is SecondFootballPlayer secondPlayer)
                {
                    secondPlayer.isActive = true;
                    secondPlayer.MoveFromNetwork(input);
                }
            }
            else
            {
                _enemyTeamAI.Update(_windowWidth, _windowHeight);

                foreach (var ai in _aiFootballAgents)
                {
                    ai.MoveAI(_windowWidth, _windowHeight, _footballPlayers, _aiFootballAgents);
                }
            }
            
            PickBall();
            ResolveBallPlayerCollisions();

            _gameBall.Update();

            MoveReferi(_keyboard);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _gameLogic.Update(deltaTime);

            if (_gameLogic.NeedResetPositions)
            {
                ResetAllPlayersToStart();
            }

            //возможно удалю потом
            if (_matchState != null && _matchState.IsPausedAfterEvent)
            {
                deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _gameLogic.Update(deltaTime);

                if (_gameLogic.NeedResetPositions)
                    ResetAllPlayersToStart();

                base.Update(gameTime);
                return;
            }
            //развожим мяч
            if (_matchState != null && _matchState.IsKickOff)
            {
                HandleKickOff();
                base.Update(gameTime);
                return;
            }

            base.Update(gameTime);
        }

        //сервеная логика основная ПОДКЛЮЧЕНИЕ КЛИЕНТА 
        private async void SendClientInput()
        {
            var state = _keyboard.GetState();

            var input = new PlayerInputPacket
            {
                W = state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W),
                A = state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A),
                S = state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S),
                D = state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D),
                Space = state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space),
                P = state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P)
            };

            await _gameClient.SendInputAsync(input);
        }
        //метод развода мяча
        private void HandleKickOff()
        {
            var keyboardState = _keyboard.GetState();

            _gameBall.StartPositionBall();

            if (_matchState.PlayerKickOff)
            {
                _gameBall.SetOwner(_forward);
                SetActivePlayer(_forward);

                bool startPressed =
                    keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W) ||
                    keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) ||
                    keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S) ||
                    keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) ||
                    keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P) ||
                    keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);

                if (startPressed)
                    _matchState.EndKickOff();
            }
            else
            {
                _gameBall.SetEnemyOwner(_aiForward);

                if (_aiForward is AIFootballAgent)
                {
                    _matchState.EndKickOff();
                }
            }
        }

        //чтобы мяч не проходи сквозь модели
        private void ResolveBallPlayerCollisions()
        {
            if (_gameBall.IsControlled())
                return;

            if (_gameBall.IsPassing())
                return;

            if (_gameBall.IsShooting())
                return;

            Vector2 ballPos = _gameBall.GetPositionBall();

            foreach (var player in _footballPlayers)
            {
                Vector2 playerCenter = player.GetPosition() + new Vector2(16, 16);
                float distance = Vector2.Distance(ballPos, playerCenter);

                if (distance < 30)
                {
                    SetActivePlayer(player);
                    _gameBall.SetOwner(player);
                    return;
                }
            }

            foreach (var enemy in _aiFootballAgents)
            {
                Vector2 enemyCenter = enemy.Position() + new Vector2(16, 16);
                float distance = Vector2.Distance(ballPos, enemyCenter);

                if (distance < 30)
                {
                    _gameBall.SetEnemyOwner(enemy);
                    return;
                }
            }
        }

        //метод сброса позиций, если был гол
        private void ResetAllPlayersToStart()
        {
            foreach (var player in _footballPlayers)
                player.ResetToHome();

            foreach (var enemy in _aiFootballAgents)
                enemy.ResetToHome();

            _forward.isActive = true;

            _gameBall.DropOwner();
            _gameBall.StartPositionBall();
        }



        //метод, который делает игрока, владеющего мячом активным, то есть я им управляю
        private void SetActivePlayer(FootballPlayer player)
        {
            foreach (var p in _footballPlayers)
                p.isActive = false;

            player.isActive = true;
        }

        //методы связанные с генерацией игровых объектов
        //--------------------------------------------------------------------------------------------------
        //метод получения тексутры объекта
        private Texture2D GenerateTextureEntity(Color colors)
        {
            Texture2D textureEntity = new Texture2D(GraphicsDevice, 32, 32);
            Color[] color = new Color[32 * 32];
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = colors;
            }
            textureEntity.SetData(color);
            return textureEntity;
        }
        //метод загрузки мяча
        private void LoadBall()
        {
            var ballTexture = GenerateTextureEntity(Color.Red);
            Vector2 startPosition = new Vector2(_windowWidth / 2 - 16, _windowHeight / 2 - 16);
            _gameBall = new Ball(ballTexture, startPosition, 16f);

            _gameBall.OwnerChanged += SetActivePlayer; //подписываюсь на событие 

            _matchState = new MatchState();
            _gameLogic = new GameLogic(_gameBall, _matchState);
        }
        //метод загрузки игроков + рефери
        private void LoadFootballPlayers()
        {
            //определние стартовой позиции каждому игроку нашему
            Vector2 startPositionForward = new Vector2((int)(_windowWidth / 2 - _radiusCentreCircle), (int)(_windowHeight / 2 - 32));
            int positionXMidfielder = (_windowWidth / 2 - _radiusCentreCircle) - (_windowWidth / 2 * 10 / 42);
            Vector2 startPositionLeftMidfielder = new Vector2(positionXMidfielder, _windowHeight / 2 - 144);
            Vector2 startPositionRightMidfielder = new Vector2(positionXMidfielder, _windowHeight / 2 + 144 - 64);
            Vector2 startPositionDefender = new Vector2((int)(_windowWidth / 2 * 10 / 42), (int)(_windowHeight / 2 - 32));
            Vector2 startPositionGoalkeeper = new Vector2(10, (int)(_windowHeight / 2 )); //сторатовая позиция вратаря
            var textureForward = GenerateTextureEntity(Color.BlueViolet);

            //создание игроков моей команды
            _forward = new Forward(textureForward, startPositionForward, _gameBall);
            _leftMidfielder = new Midfielder(textureForward, startPositionLeftMidfielder, _gameBall, -1);
            _rightMidfielder = new Midfielder(textureForward, startPositionRightMidfielder, _gameBall, 1);
            _defender = new Defender(textureForward, startPositionDefender, _gameBall);
            _goalkeeper = new Goalkeeper(textureForward, startPositionGoalkeeper, _gameBall);

            //добавление в список игроков
            _footballPlayers = new List<FootballPlayer>()
            {
                _forward,
                _leftMidfielder,
                _rightMidfielder,
                _defender
            };
            //длеаю изначально столба активным игроком
            _forward.isActive = true;
            _playerTeamAI = new PlayerTeamAI(_footballPlayers, _aiFootballAgents, _gameBall);
        }

        //загрузка текстур противника AI
        private void LoadTextureAI()
        {
            int positionXMidfielder = ((_windowWidth / 2 - _radiusCentreCircle) + (_windowWidth / 2 * 10 / 42) + 2 * _radiusCentreCircle);

            Vector2 startPositionForwardAI = new Vector2((int)(_windowWidth / 2 + _radiusCentreCircle), (int)(_windowHeight / 2 - 32));
            Vector2 startPositionLeftMidfielderAI = new Vector2(positionXMidfielder, _windowHeight / 2 + 144);
            Vector2 startPositionRightMidfielderAI = new Vector2(positionXMidfielder, _windowHeight / 2 - 144 - 64);
            Vector2 startPositionDefenderAI = new Vector2((int)(((_windowWidth / 10) - 10) * 10), (int)((_windowHeight / 2) - 32));
            Vector2 startPositionGoalkeeperAI = new Vector2(880 - 32, 300 - 32);

            var textureAI = GenerateTextureEntity(Color.Green);

            if (_isServerGame)
            {
                _aiForward = new SecondFootballPlayer(textureAI, startPositionForwardAI, _gameBall, "forward");
            }
            else
            {
                _aiForward = new AIFootballAgent(textureAI, startPositionForwardAI, _gameBall, "forward");
            }

            _aiLMidfielder = new AIFootballAgent(textureAI, startPositionLeftMidfielderAI, _gameBall, "leftMidfielder");
            _aiRMidfielder = new AIFootballAgent(textureAI, startPositionRightMidfielderAI, _gameBall, "rightMidfielder");
            _aiDefender = new AIFootballAgent(textureAI, startPositionDefenderAI, _gameBall, "defender");
            _aiGoalkeeper = new AIFootballAgent(textureAI, startPositionGoalkeeperAI, _gameBall, "goalkeeper");

            _aiFootballAgents = new List<EnemyFootball>()
            {
                _aiForward,
                _aiLMidfielder,
                _aiRMidfielder,
                _aiDefender,
                _aiGoalkeeper
            };

            _enemyTeamAI = new EnemyTeamAI(_aiFootballAgents, _footballPlayers, _gameBall);
        }
        //загрузка текстур рефери
        private void LoadReferi()
        {
            Vector2 startPositionFirrstReferi = new Vector2(0, 0);
            Vector2 startPositionSecondReferi = new Vector2(0, (int)(_windowHeight) - 70);
            var textureReferi = GenerateTextureEntity(Color.Yellow);
            _textureReferi = textureReferi;
            _referiPosition = startPositionFirrstReferi;

            _textureReferiSecond = textureReferi;
            _referiSecondPosition = startPositionSecondReferi;
        }
        //-----------------------------------------------------------------------------------------------------
        //алгоритмические методы
        //-----------------------------------------------------------------------------------------------------
        //метод для вычисление дистанции между игроками
        private void CalculateDistanceAllPlayers()
        {
            _dictionaryDistance.Clear();

            var players = _footballPlayers;

            for (int i = 0; i < players.Count; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    float distance = Vector2.Distance(players[i].GetPosition(), players[j].GetPosition());
                    _dictionaryDistance[(players[i], players[j])] = distance;
                    _dictionaryDistance[(players[j], players[i])] = distance;
                }
            }
        }

        //метод для поиска ближайшего игрока, чтобы отдать ему пас
        private FootballPlayer FindNearestPlayer(FootballPlayer owner)
        {
            FootballPlayer nearestPlayer = null;
            float minDistance = float.MaxValue;

            foreach (var player in _footballPlayers)
            {
                if (player == owner)
                    continue;

                float distance = Vector2.Distance(owner.GetPosition(), player.GetPosition());

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlayer = player;
                }
            }

            return nearestPlayer;
        }
        //определение нового игрока, владеющего мячом
        private void PassToNearestPlayer()
        {
            if (!_gameBall.IsControlled())
                return;

            FootballPlayer owner = _gameBall.GetOwner();

            if (owner == null)
                return;

            FootballPlayer target = FindNearestPlayer(owner);

            if (target == null)
                return;

            _gameBall.StartPass(target);
        }
        //-----------------------------------------------------------------------------------------------------
        //методы передвижения объектов
        //-----------------------------------------------------------------------------------------------------

        //логика движения мяча и игрока
        private void PickBall()
        {
            if (_gameBall.IsControlled())
                return;

            if (_gameBall.IsPassing())
                return;

            if (_gameBall.IsShooting())
                return;

            foreach (var player in _footballPlayers)
            {
                float distance = Vector2.Distance(
                    player.GetPosition() + new Vector2(16, 16),
                    _gameBall.GetPositionBall()
                );

                if (distance < 35)
                {
                    SetActivePlayer(player);
                    _gameBall.SetOwner(player);
                    return;
                }
            }

            foreach (var ai in _aiFootballAgents)
            {
                float distance = Vector2.Distance(
                    ai.Position() + new Vector2(16, 16),
                    _gameBall.GetPositionBall()
                );

                if (distance < 30)
                {
                    _gameBall.SetEnemyOwner(ai);
                    return;
                }
            }
        }

        private void ShootToGoal()
        {
            if (!_gameBall.IsControlled())
                return;

            FootballPlayer owner = _gameBall.GetOwner();

            if (owner == null)
                return;

            Vector2 enemyGoal = new Vector2(_windowWidth, _windowHeight / 2);
            Vector2 direction = enemyGoal - owner.GetPosition();

            _gameBall.Shoot(direction, 10f);
        }

        private void MoveReferi(WpfKeyboard keyboard)
        {
            
            if (_directionReferi)
            {
                _referiPosition.X += SPEED_REFERI;
                if (_referiPosition.X >= _windowWidth - 32)
                    _directionReferi = false;
            }
            else
            {
                _referiPosition.X -= SPEED_REFERI;
                if (_referiPosition.X <= 0)
                    _directionReferi = true;
            }
            if (_directionReferi)
            {
                _referiSecondPosition.X += SPEED_REFERI;
                if (_referiSecondPosition.X >= _windowWidth - 32)
                    _directionReferi = false;
            }
            else
            {
                _referiSecondPosition.X -= SPEED_REFERI;
                if (_referiSecondPosition.X <= 0)
                    _directionReferi = true;
            }
        }
    }
}