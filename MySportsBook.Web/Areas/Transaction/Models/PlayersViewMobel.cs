using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySportsBook.Web.Areas.Transaction.Models
{
    public class PlayersViewMobel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Batches { get; set; }
        public int PK_PlayerId { get; set; }
    }
}