namespace FifaFootballGame.Models
{
    //класс отвечающий за видение поля игрока, как мозг
    public class FootballBrain
    {
        private NeuralDecisionModel _model = new NeuralDecisionModel();

        public AIActionType Decide(AIFeatures features)
        {
            return _model.Predict(features);
        }

        public void Learn(AIFeatures features, AIActionType goodAction)
        {
            _model.Train(features, goodAction);
        }
    }
}