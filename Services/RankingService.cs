using Microsoft.ML;
using Microsoft.ML.Data;
using MovieRankingSystem.Models;

namespace MovieRankingSystem.Services
{
    public class RankingService
    {
        private readonly MLContext? _mlContext;
        private readonly string _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel", "rankingModel.zip");
        private ITransformer? _model;

        public RankingService()
        {
            _mlContext = new MLContext();
        }

        public List<MovieData> GetMoviesFromCsv(string path)
        {
            var dataView = _mlContext!.Data.LoadFromTextFile<MovieData>(
                path,
                hasHeader: true,
                separatorChar: ','
            );

            return _mlContext.Data.CreateEnumerable<MovieData>(dataView, reuseRowObject: false).ToList();
        }

        public void LoadModel()
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext!.Model.Load(_modelPath, out _);
                Console.WriteLine("✅ Model loaded!");
            }
            else
            {
                Console.WriteLine("❌ Model not found, training...");
                Train(Path.Combine(Directory.GetCurrentDirectory(), "Dataset", "movies.csv"));
            }
        }
        public void Train(string dataPath)
        {
            var data = _mlContext!.Data.LoadFromTextFile<MovieData>(
                dataPath,
                hasHeader: true,
                separatorChar: ','
            );

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                                    nameof(MovieData.Rating),
                                    nameof(MovieData.Popularity),
                                    nameof(MovieData.GenreMatch))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("GroupId"))
                .Append(_mlContext.Ranking.Trainers.LightGbm(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    rowGroupColumnName: "GroupId"));

            _model = pipeline.Fit(data);

            // ✅ SAVE MODEL
            Directory.CreateDirectory("MLModel");
            _mlContext.Model.Save(_model, data.Schema, _modelPath);

            Console.WriteLine("✅ Model trained and saved!");
        }

        public float Predict(MovieData input)
        {
            var engine = _mlContext!.Model.CreatePredictionEngine<MovieData, MoviePrediction>(_model);
            return engine.Predict(input).Score;
        }
    }
}
