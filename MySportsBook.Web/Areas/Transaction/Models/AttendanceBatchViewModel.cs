using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySportsBook.Web.Areas.Transaction.Models
{
    public class AttendanceBatchViewModel
    {
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string FK_BatchId { get; set; }
        public string Date { get; set; }
        public string Fees { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string PK_PlayerId { get; set; }
        public string AttendanceBatchCode { get; set; }
        public string AttendanceBatchName { get; set; }
    }

    public class AttendanceBatchPlayerViewModel
    {
        public bool Present { get; set; }
        public string AttendanceId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string PlayerId { get; set; }
        public string BatchId { get; set; }
        public string LastPaidMonth { get; set; }
        public string Image { get; set; }
             
    }

}