using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySportsBook.Model.ViewModel
{
    public class PlayerModel
    {
        public Master_Player Player { get; set; }
        public List<Transaction_PlayerSport> PlayerSports { get; set; }
    }
    public class PlayerSportModel
    {
        public int SportId { get; set; }
        public string Sport { get; set; }
        public int BatchId { get; set; }
        public string Batch { get; set; }
        public int InvId { get; set; }
        public string Inv { get; set; }
        public double Fee { get; set; }
    }
}
