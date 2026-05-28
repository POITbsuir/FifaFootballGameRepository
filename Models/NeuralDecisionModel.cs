using System;

namespace FifaFootballGame.Models
{
    //модель для обучения моих агентов с помощью Перцептрона
    public class NeuralDecisionModel
    {
        private const int InputCount = 9;
        private const int ActionCount = 7;

        private float[,] _weights = new float[ActionCount, InputCount];
        private Random _random = new Random();

        public NeuralDecisionModel()
        {
            for (int a = 0; a < ActionCount; a++)
                for (int i = 0; i < InputCount; i++)
                    _weights[a, i] = (float)(_random.NextDouble() * 0.2 - 0.1);
        }

        public AIActionType Predict(AIFeatures features)
        {
            float[] x = features.ToArray();

            int bestAction = 0;
            float bestScore = float.MinValue;

            for (int a = 0; a < ActionCount; a++)
            {
                float score = 0;

                for (int i = 0; i < InputCount; i++)
                    score += _weights[a, i] * x[i];

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = a;
                }
            }

            return (AIActionType)bestAction;
        }

        public void Train(AIFeatures features, AIActionType correctAction, float learningRate = 0.03f)
        {
            AIActionType predicted = Predict(features);

            if (predicted == correctAction)
                return;

            float[] x = features.ToArray();

            int correct = (int)correctAction;
            int wrong = (int)predicted;

            for (int i = 0; i < InputCount; i++)
            {
                _weights[correct, i] += learningRate * x[i];
                _weights[wrong, i] -= learningRate * x[i];
            }
        }
    }
}