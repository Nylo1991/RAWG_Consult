using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawgApi.Models
{
    public class Games
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descrição { get; set; }
        public string ImagemUrl { get; set; }

        public string Avaliacao { get; set; } 

        public string Classificação { get; set; }

        public DateTime Upload { get; set; } 

    }
}
