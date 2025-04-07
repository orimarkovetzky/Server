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


        private SqlCommand CreateCommandWithTextQuery(string sqlQuery, SqlConnection con, Dictionary<string, object> paramDic)
        {
            SqlCommand cmd = new SqlCommand
            {
                Connection = con,
                CommandText = sqlQuery,
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
            string sqlQuery = "select userName from dbo.Users";

            try
            {
                using (SqlConnection con = connect("igroup16_test1"))
                using (SqlCommand cmd = CreateCommandWithTextQuery(sqlQuery, con, null))
                {
                   
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User();
                            user.Name = reader["userName"].ToString();
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

    }


}
