namespace UserManagementService.Model
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Email { get; set; }
        public string Role { get; set; }
    }
}
