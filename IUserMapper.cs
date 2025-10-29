public interface IUserMapper
{
    List<User> GetAll();
    User GetById(int id);
    int InsertUser(User user);
    int DeleteUser(int id);
    int UpdateUser(int id, string userName, string email);
}
