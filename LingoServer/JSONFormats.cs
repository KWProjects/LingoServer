using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingoServer
{
    class JSONFormats
    {

    }

    class JoinRequest
    {
        public string seat { get; set; }
        public string name { get; set; }
    }
    
    class JSONPlayer
    {
        public string clientid { get; set; }
        public string seat { get; set; }
        public string name { get; set; }
    }
    class NextTurn
    {
        public string playerturn { get; set; }
        public string turncount { get; set; }
        public string correctletters { get; set; }
        public string guessedword { get; set; }
        public int[] check { get; set; }
        public int teamonescore { get; set; }
        public int teamtwoscore { get; set; }
    }
    
}
