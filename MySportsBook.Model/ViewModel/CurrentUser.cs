using System.Collections.Generic;

namespace MySportsBook.Model.ViewModel
{
    public class CurrentUser
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public int CurrentVenueId { get; set; }
        public string CurrentVenueName { get; set; }
        public int RoleId { get; set; }
        public List<string> MenuList { get; set; }
    }
}
