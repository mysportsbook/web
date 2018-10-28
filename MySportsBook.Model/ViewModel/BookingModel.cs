using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySportsBook.Model.ViewModel
{
    public class BookingModel
    {
        public int BookingID { get; set; }
        public string BookingNo { get; set; }
        public string Name { get; set; }
        public string Court { get; set; }
        public string Slots { get; set; }
        public string Amount { get; set; }

    }
}
