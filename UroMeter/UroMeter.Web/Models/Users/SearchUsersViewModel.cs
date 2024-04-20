using UroMeter.DataAccess.Models;

namespace UroMeter.Web.Models.Users;

public class SearchUsersViewModel
{
    public List<User> Users { get; set; } = new();
}