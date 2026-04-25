using Microsoft.ML.Data;

namespace MovieRankingSystem.Models
{
    public class MoviePrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}
