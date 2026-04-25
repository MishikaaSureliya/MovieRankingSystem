using Microsoft.ML.Data;

namespace MovieRankingSystem.Models
{
    public class MovieData
    {
        [LoadColumn(6)]
        public float Label { get; set; }

        [LoadColumn(0)]
        public string Query { get; set; } = string.Empty;
        [LoadColumn(1)]
        public string Movie { get; set; } = string.Empty;
        [LoadColumn(2)]
        public string Genre { get; set; } = string.Empty;
        [LoadColumn(3)]
        public float Rating { get; set; }
        [LoadColumn(4)]
        public float Popularity { get; set; }
        [LoadColumn(5)]
        public float GenreMatch { get; set; }
        [LoadColumn(7)]
        public string GroupId { get; set; } = string.Empty;
    }
}
