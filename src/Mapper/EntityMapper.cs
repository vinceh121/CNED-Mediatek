using System;
using System.Reflection;
using MySqlConnector;

namespace Mediatek.Mapper
{
	public class EntityMapper
	{
		/// <summary>
		/// Maps a record from an SQL row. It must figured the DbAttribute attribute to the constructor parameters.
		/// Currently, this only uses the first constructor and doesn't care about the others.
		/// </summary>
		/// <param name="reader">An open MySqlDataReader that must be on a row</param>
		/// <typeparam name="T">Type to map to</typeparam>
		public static T MapFromRow<T>(MySqlDataReader reader)
		{
			Type type = typeof(T);

			// use the first (and hopefully only) constructor
			ConstructorInfo info = type.GetConstructors()[0];
			ParameterInfo[] parameters = info.GetParameters();
			object[] parameterValues = new object[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo param = parameters[i];
				object[] attributes = param.GetCustomAttributes(typeof(DbAttribute), true);
				if (attributes.Length == 0)
				{
					continue;
				}
				DbAttribute attr = (DbAttribute)attributes[0];
				try
				{
					parameterValues[i] = GetValue(param.ParameterType, attr.ColumnName, reader);
				}
				catch (IndexOutOfRangeException e)
				{
					if (!attr.Optional)
					{
						throw e;
					}
				}
			}

			// call constructor
			T entity = (T)info.Invoke(parameterValues);

			// set properties
			PropertyInfo[] properties = type.GetProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				PropertyInfo property = properties[i];
				object[] attributes = property.GetCustomAttributes(typeof(DbAttribute), true);
				if (attributes.Length == 0)
				{
					continue;
				}
				DbAttribute attr = (DbAttribute)attributes[0];
				try
				{
					properties[i].SetValue(entity, GetValue(property.PropertyType, attr.ColumnName, reader));
				}
				catch (IndexOutOfRangeException e)
				{
					if (!attr.Optional)
					{
						throw e;
					}
				}
			}

			return entity;
		}

		public static object GetValue(Type type, string columnName, MySqlDataReader reader)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.String:
					return reader.GetString(columnName);
				case TypeCode.Int64:
					return reader.GetInt64(columnName);
				case TypeCode.Int32:
					return reader.GetInt32(columnName);
				case TypeCode.Boolean:
					return reader.GetBoolean(columnName);
				default:
					throw new InvalidOperationException("Cannot get type " + type + " from SQL");
			}
		}
	}
}
