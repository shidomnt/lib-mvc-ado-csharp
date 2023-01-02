using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;

namespace DBLib
{
    public class Database
    {
        private static Database DatabaseInstance
        {
            get;
            set;
        }

        SqlConnection _connection;

        public SqlConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        private Database ()
        {
            var connectionString = ConfigurationManager
                .ConnectionStrings["SqlServer"]
                .ConnectionString;
            var connection = new SqlConnection(connectionString);
            _connection = connection;
        }

        public static Database GetDatabase ()
        {
            if (DatabaseInstance == null)
            {
                DatabaseInstance = new Database();
            }
            return DatabaseInstance;
        }

        public void FromSqlCommand (SqlCommand cmd, Action<Exception, SqlDataReader> callback)
        {
            try
            {
                Connection.Open();

                var reader = cmd.ExecuteReader();

                callback(null, reader);

            }
            catch (Exception ex)
            {
                callback(ex, null);
            }
            finally
            {
                Connection.Close();
            }
        }
    }
}
