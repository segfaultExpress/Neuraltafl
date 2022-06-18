using System;


namespace NeuralTaflAi
{
    public class Constants
    {
        // AI Train Constants
        public const int NUM_TOTAL_ITERS = 1000;
        public const int PLAY_OUT_ITERATIONS = 800;
        public const int NODE_TRAVERSALS = 10;
        public const int HISTORY_QUEUE_SIZE = 20;
        public const bool SKIP_FIRST_ITERATION = false;
        public const int NUM_EPISODES = 30;
        public const int DUAL_FIGHT_NUM = 100;
        public const int TEMP_THRESHOLD = 20;

        // MCTS Constants
        public const decimal EPS = 1E-08M;
        public const double CPUCT = 1;
        public const int NUM_MCTS_SIM = 25;

        // NN Constants
        public const int NN_CHANNELS = 512;
        public const double NN_DROPOUT = 0.3;
        public const double NN_LEARNING_RATE = 0.001;
        public const int BATCH_SIZE = 64;
        public const int EPOCS = 10;

        public const String ROOT_FOLDER = "../../";
    }

}