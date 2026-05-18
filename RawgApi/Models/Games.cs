using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RawgApi.Models
{
    public class Games
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string ImagemUrl { get; set; } = string.Empty;

        public string Avaliacao { get; set; } = "0";

        public string Classificacao { get; set; } = "0";

        public DateTime Upload { get; set; } = DateTime.Now;

        [NotMapped]
        [JsonIgnore]
        public bool IsSelected { get; set; }

        [NotMapped]
        [JsonIgnore]
        public int DisplayId { get; set; }
    }
}