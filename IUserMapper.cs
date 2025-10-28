public interface IUserMapper
{
    List<User> GetAll();
    User GetById(int id);
    int InsertUser(User user);
}
