using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeManagement.ViewModel
{
    public class NotesModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public string CreatedOn { get; set; }
        public int WeeklyReportId { get; set; }
    }
}