using System.Data.SqlClient;
using System.Data;
using FlowServer.Models;
using FlowServer.DBServices;
using User = FlowServer.Models.User;
using System.Reflection.PortableExecutable;

namespace FlowServer.DBServices
{
    public class UserDBServices
    {
        public SqlConnection connect(String conString)
        {

            // read the connection string from the configuration file
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build();
            string cStr = configuration.GetConnectionString("DefaultConnection");
            SqlConnection con = new SqlConnection(cStr);
            con.Open();
            return con;
        }


        private SqlCommand CreateCommandWithSP(string sqlSP, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = sqlSP,
                CommandTimeout = 10, // optional
                CommandType = CommandType.StoredProcedure
            };

            if (paramDic != null)
            {
                foreach (KeyValuePair<string, object> param in paramDic)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return cmd;
        }

        public List<User> ReadUsers()
        {
            List<User> users = new List<User>();
            string storedProcedureName = "GetUsers";

            try
            {
                using (SqlConnection con = connect("igroup16_test1")) // your connection function
                using (SqlCommand cmd = CreateCommandWithSP(storedProcedureName, con, null))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User();

                                user.Id = Convert.ToInt32(reader["userId"]);

                                user.Name = reader["userName"].ToString();

                                user.Email = reader["email"].ToString();

                                user.Password = reader["password"].ToString();

                                user.IsManager = Convert.ToBoolean(reader["isManager"]);

                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log the error.
                throw; // Use 'throw' to preserve the original stack trace.
            }

            return users;
        }

        public void InsertUser(User user)
        {
            Dictionary<string, object> paramDic = new Dictionary<string, object>
        {
            { "@username", user.Name },
            { "@isManager", user.IsManager },
            { "@password", user.Password },
            { "@email", user.Email }
        };

            using (SqlConnection con = connect("igroup16_test1"))
            using (SqlCommand cmd = CreateCommandWithSP("InsertUser", con, paramDic))
            {
                cmd.ExecuteNonQuery();
            }
        }

    }
}

