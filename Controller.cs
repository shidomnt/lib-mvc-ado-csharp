using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using DBLib.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DBLib
{
    public class Controller<EntityT> where EntityT : Model
    {
        public Database Db
        {
            get;
            set;
        }

        Type typeParameterType;

        public Controller ()
        {
            Db = Database.GetDatabase();
            typeParameterType = typeof(EntityT);
        }

        public void Add (EntityT entity)
        {
            Add(entity, (err, reader) =>
            {
            });
        }

        public void Add (EntityT entity, Action<Exception, SqlDataReader> callback)
        {
            var tuple = GetProperiesName();

            var properties = tuple.Item1;
            var propertiesWithPreFix = tuple.Item2;

            var tableName = entity.GetType().Name;
            var columnList = string.Join(",", properties.ToArray());
            var valuelist = string.Join(",", propertiesWithPreFix.ToArray());

            var sql = string.Format(
                @"
                    INSERT INTO {0}
                    ({1})
                    VALUES({2})",
                tableName,
                columnList,
                valuelist
                );
            SqlCommand command;
            FillParameters(sql, entity, out command);
            Db.FromSqlCommand(command, callback);
        }

        public void Update (EntityT entity)
        {
            Update(entity, (err, reader) =>
            {
            });
        }

        public void Update (EntityT entity, Action<Exception, SqlDataReader> callback)
        {
            var tuple = GetProperiesName();

            var properties = tuple.Item1;
            var propertiesWithPreFix = tuple.Item2;

            var primaryProperties = GetPrimaryKeyProperties();

            var tableName = entity.GetType().Name;
            var columnToUpdate = string.Join(",", properties
                .ToArray()
                .Where(property => !primaryProperties.Contains(property))
                .Select(property => string.Format("{0} = {1}", property, '@' + property)));
            var updateCondition = string.Join(" AND ",
                primaryProperties
                .Select(primaryProperty => string.Format("{0} = {1}", primaryProperty, AddParametterPrefix(primaryProperty))));
            
            var sql = string.Format(
                @"
                    UPDATE {0} 
                    SET 
                    {1}
                    WHERE {2}
                ",
                tableName,
                columnToUpdate,
                updateCondition
                );
            SqlCommand command;
            FillParameters(sql, entity, out command);
            Db.FromSqlCommand(command, callback);
        }

        public void Delete (EntityT entity)
        {
            Delete(entity, (err, reader) =>
            {
            });
        }

        public void Delete (EntityT entity, Action<Exception, SqlDataReader> callback)
        {
            var tuple = GetProperiesName();

            var properties = tuple.Item1;
            var propertiesWithPreFix = tuple.Item2;

            var primaryProperties = GetPrimaryKeyProperties();

            var tableName = entity.GetType().Name;
            var deleteCondition = string.Join(" AND ",
                primaryProperties
                .Select(
                primaryProperty => 
                    string.Format("{0} = {1}", 
                    primaryProperty, 
                    AddParametterPrefix(primaryProperty)
                        )
                    )
                );

            var sql = string.Format(
                @"
                    DELETE FROM {0}
                    WHERE {1}
                ",
                tableName,
                deleteCondition
                );
            SqlCommand command;
            FillParameters(sql, entity, out command);
            Db.FromSqlCommand(command, callback);
        }

        public DataTable GetDataTable ()
        {
            DataTable dataTable = new DataTable();
            var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = string.Format("SELECT * FROM {0}", typeParameterType.Name);
            var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dataTable);
            return dataTable;
        }

        private void FillParameters (string sql, EntityT entity, out SqlCommand command)
        {
            var tuple = GetProperiesName();
            var properties = typeParameterType
                .GetProperties()
                .ToList();
            var propertiesWithPreFix = tuple.Item2;
            command = Db.Connection.CreateCommand();
            command.CommandText = sql;

            for (var i = 0; i < properties.Count; i++)
            {
                var propertyType = SqlDbType.NVarChar;

                var attributeList = properties[i].GetCustomAttributes(typeof(ColumnAttribute), false);

                if (attributeList.Any())
                {
                    var attribute = (ColumnAttribute)attributeList.First();
                    propertyType = attribute.DataType;
                }

                command
                    .Parameters
                    .Add
                    (propertiesWithPreFix[i], propertyType).Value = properties[i].GetValue(entity);
            }
        }

        private Tuple<string[], string[]> GetProperiesName ()
        {
            var properties = typeParameterType
                .GetProperties()
                .Select(property => property.Name);
            var propertiesWithPreFix = properties
                .Select(AddParametterPrefix);
            return Tuple.Create(properties.ToArray(), propertiesWithPreFix.ToArray());
        }

        private string AddParametterPrefix (string propertyName)
        {
            return "@" + propertyName;
        }

        private List<string> GetPrimaryKeyProperties ()
        {
            var primaryProperties = typeParameterType
                .GetProperties()
                .Where(property => property.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .Select(property => property.Name)
                .ToList();

            if (!primaryProperties.Any())
            {
                var properties = GetProperiesName().Item1;
                primaryProperties.Add(properties.FirstOrDefault());
            }
            return primaryProperties;
        }

    }
}
