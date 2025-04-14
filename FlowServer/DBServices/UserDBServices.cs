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


        private SqlCommand CreateCommandWithTextQuery(string sqlSP, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = sqlSP,
                CommandTimeout = 10, // optional
                CommandType = CommandType.Text // <-- this is the key difference
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
                using (SqlCommand cmd = CreateCommandWithTextQuery(storedProcedureName, con, null))
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

        public int CreateUser(User user)
        {
            int newUserId = 0;

            try
            {
                using (SqlConnection con = connect("igroup16_test1"))
                using (SqlCommand cmd = new SqlCommand("dbo.CreateUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", user.name);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@IsManager", user.IsManager);

                    // ExecuteScalar will return the single value SELECTed by the SP:
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        newUserId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log the error.
                throw; // Use 'throw' to preserve the original stack trace.
            }
            return newUserId;
        }

    }
}

