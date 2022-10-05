using System;
using System.IO;

using NeuralTaflGame;

using tensorflow;
using numpy;
using tensorflow.keras.layers;
using keras = tensorflow.keras;
using Model = tensorflow.keras.Model;
using tensorflow.keras.optimizers;
using LostTech.Gradient.BuiltIns;

namespace NeuralTaflAi
{
    public class NNet
    {
        Model model {set; get;}

        public NNet(Board board)
        {
            this.model = createKerasNet(board);
        }

        /// <summary>
        /// Train the neural network with a series of data in the form (x = boards, y = [policies, values])
        /// </summary>
        /// <param name="trainData">The data to train the network with</param>
        /// <returns>void</returns>
        public void train(List<NNData> trainData)
        {

            List<int[]> iBoardList = new List<int[]>();
            List<double[]> oPolicyList = new List<double[]>();
            List<double> oValueList = new List<double>();

            foreach (NNData trainItem in trainData)
            {
                iBoardList.Add(trainItem.boardArray);
                oPolicyList.Add(trainItem.policy);
                oValueList.Add(trainItem.v);
            }

            ndarray iBoardTrainND = numpy.np.array(iBoardList);
            ndarray oPolicyTrainND = numpy.np.array(oPolicyList);
            ndarray oValueTrainND = numpy.np.array(oValueList);

            this.model.fit(x: iBoardTrainND, y: new [] {oPolicyTrainND, oValueTrainND}, batch_size: Constants.BATCH_SIZE, epochs: Constants.EPOCS);
        }

        /// <summary>
        /// Predicts the policies and estimated value given a board state.
        /// </summary>
        /// <param name="board">The board state to be evaluated</param>
        /// <returns>NDarray created by the neural network model</returns>
        public NNData predict(Board board)
        {
            List<int[]> input = new List<int[]>() {board.getBoard1DArray()};
            ndarray inputND = numpy.np.array(input);

            PythonList<object> predictPyList = this.model.predict(x: inputND);

            // Unwrap from python to C#
            ndarray policyND = numpy.np.array(predictPyList[0]);
            ndarray valueND = numpy.np.array(predictPyList[1]);
            
            double[] policy = policyND[0].tolist_dyn();
            double[] value = valueND[0].tolist_dyn();

            return new NNData(board.getBoard1DArray(), policy, value[0]);
        }

        /// <summary>
        /// Create a keras neural network to train and test data.
        /// </summary>
        /// <param name="board">The board which will be used to generate network parameters such as size, data</param>
        /// <returns>The model that was generated</returns>
        public Model createKerasNet(Board board)
        {
            int boardSize = board.NCols * board.NRows;

            var iLayer = tf.keras.Input(shape: new List<int>() {boardSize});
            var x_image = new keras.layers.Reshape(target_shape: new List<int>() {board.NRows, board.NCols, 1}).__call__(iLayer);

            // 2D Convolutional Layers - A toolbox for the neural network to utilize in order to learn
            var conv1 = new keras.layers.Conv2D(filters: Constants.NN_CHANNELS, kernel_size: 3, strides: (1, 1), activation: "relu", padding: "same", use_bias: false).__call__(x_image);
            var conv2 = new keras.layers.Conv2D(filters: Constants.NN_CHANNELS, kernel_size: 3, strides: (1, 1), activation: "relu", padding: "same", use_bias: false).__call__(conv1);
            var conv3 = new keras.layers.Conv2D(filters: Constants.NN_CHANNELS, kernel_size: 3, strides: (1, 1), activation: "relu", padding: "valid", use_bias: false).__call__(conv2);
            var conv4 = new keras.layers.Conv2D(filters: Constants.NN_CHANNELS, kernel_size: 3, strides: (1, 1), activation: "relu", padding: "valid", use_bias: false).__call__(conv3);
            
            // Flatten the above convolutional layers to a 1xN layer which will assist outputs
            var conv4_flat = new keras.layers.Flatten().__call__(conv4);

            // Prevent overfitting by allowing the network to apply a mask over the outputs
            var droupout1 = new keras.layers.Dropout(Constants.NN_DROPOUT).__call__(conv4_flat); // Dropouts randomly sets inputs to 0 at specific rates to prevent overfitting
            var droupout2 = new keras.layers.Dropout(Constants.NN_DROPOUT).__call__(droupout1); // Dropouts randomly sets inputs to 0 at specific rates to prevent overfitting

            // Dense layers are required to have connections to every node in the prior layer - good output layer
            var oLayerPolicy = new keras.layers.Dense(board.getActionSize(), activation: "softmax").__call__(droupout2);
            var oLayerValue = new keras.layers.Dense(1, activation: "tanh").__call__(droupout2);

            // This is a more complex NN due to multiple out-layers. Care must be taken in initializing, compiling, training and predicting to make sure both output layers are returned
            var dictModel = new Dictionary<string, object>();

            dictModel.Add("inputs", new List<object>() {iLayer});
            dictModel.Add("outputs", new List<object>() {oLayerPolicy, oLayerValue});

            var model = new Model(kwargs: dictModel);

            model.compile(optimizer: new Adam(learning_rate: Constants.NN_LEARNING_RATE), loss: new [] {tf.keras.losses.categorical_crossentropy_fn, tf.keras.losses.MSE_fn});

            return model;
        }

        /// <summary>
        /// Saves the weights of the current Keras model into a checkpoint file.
        /// </summary>
        /// <param name="folder">The folder that contains checkpoint files</param>
        /// <param name="file">The file that will contain the neural network weights</param>
        /// <returns>void</returns>
        public void save(String folder = "checkpoints", String file = "checkpoint.h5")
        {
            String path = Constants.ROOT_FOLDER + folder;
            String fullFilePath = path + "/" + file;

            // Initialize the directory
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            this.model.save_weights(fullFilePath);
        }

        /// <summary>
        /// Loads the weights of the latest checkpoint file into the existing neural network.
        /// </summary>
        /// <param name="folder">The folder that contains checkpoint files</param>
        /// <param name="file">The file that contains the neural network weights</param>
        /// <returns>void</returns>
        public void load(String folder = "checkpoints", String file = "checkpoint.h5")
        {
            String path = Constants.ROOT_FOLDER + folder;
            String fullFilePath = path + "/" + file;

            // If the file or folder don't exist, 
            if (!File.Exists(fullFilePath))
            {
                Console.WriteLine("Error: There is no folder/file at {0}!", folder);
                return;
            }

            this.model.load_weights(fullFilePath);
        }
    }
}