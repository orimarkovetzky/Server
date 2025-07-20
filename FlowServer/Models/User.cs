using FlowServer.DBServices;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace FlowServer.Models
{
    public class User
    {
        private int id;


        public string name;

        [JsonPropertyName("email")]
        public string email;

        [JsonPropertyName("password")]
        public string password;



        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Email { get => email; set => email = value; }

        public bool IsManager { get; set; } = false;
        public string Password { get => password; set => password = value; }

        public User()
        {

        }
        public User(string name, string email,bool isManager ,string password)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "" : name;
            Email = email;
            IsManager = isManager;
            Password = password;

        }

        public static void CreateUser(User user)
        {
            UserDBServices dbs = new UserDBServices();
            dbs.InsertUser(user);
        }

        public User(string name, bool isManager, string password)
        {
            
            Name =  name;
            IsManager = isManager;
            Password =  password;

        }


        public bool Login(string email, string password)
        {
            UserDBServices dbs = new UserDBServices();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            foreach (User u in dbs.ReadUsers())
            {
                if (u.Email == email && u.Password == password)
                { return true; }
            }
            return false;
        }

        public bool CheckManager()
        {
            UserDBServices db = new UserDBServices();
            return db.IsUserManager(this.Id);
        }

        public int GetUserID(string email)
        {
            UserDBServices dbs = new UserDBServices();
            foreach (User u in dbs.ReadUsers())
            {
                if (u.Email == email)
                { return u.Id; }
            }
            return 0;
        }

        public string GetUserName(string email)
        {
            UserDBServices dbs = new UserDBServices();
            foreach (User u in dbs.ReadUsers())
            {
                if (u.Email == email)
                { return u.Name; }
            }
            return "Error";
        }

        static public List<User> Read()
        {
            UserDBServices dbs = new UserDBServices();
            return dbs.ReadUsers();
        }
    }
}
