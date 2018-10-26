using System;
using System.Collections.Generic;

namespace MySportsBook.Model.ViewModel
{
    public class BatchModel
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public double Fee { get; set; }
        public int SportId { get; set; }
        public int CourtId { get; set; }
        public int MaxPlayers { get; set; }
        public bool AttendanceRequired { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int BatchTypeId { get; set; }
        public int? CoachId { get; set; }
        public List<BatchTimingModel> BatchTimings { get; set; }
    }
    public class BatchTimingModel
    {
        public string WeekDay { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
