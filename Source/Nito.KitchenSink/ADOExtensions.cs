namespace Nito.KitchenSink
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    /// <summary>
    /// Provides extension methods for ADO.NET classes.
    /// </summary>
    public static class AdoExtensions
    {
        /// <summary>
        /// Creates a new command associated with this connection, with the specified command text.
        /// </summary>
        /// <param name="this">The database connection.</param>
        /// <param name="commandText">The command text for this command.</param>
        /// <returns>The new command.</returns>
        public static DbCommand CreateCommand(this DbConnection @this, string commandText)
        {
            var ret = @this.CreateCommand();
            ret.CommandText = commandText;
            return ret;
        }

        /// <summary>
        /// Executes a command against the database and returns its scalar result, or <c>null</c> if there was no result.
        /// </summary>
        /// <param name="this">The connection executing the command.</param>
        /// <param name="commandText">The command to execute.</param>
        /// <param name="parameters">Any parameters to pass to the command. <c>null</c> values are replaced with <c>DBNull.Value</c>.</param>
        /// <returns>The scalar result of the command, or <c>null</c> if there was no result.</returns>
        public static object ExecuteCommand(this DbConnection @this, string commandText, params object[] parameters)
        {
            return @this.ExecuteCommand<object>(commandText, parameters);
        }

        /// <summary>
        /// Executes a command against the database and returns its scalar result, or <c>(T)null</c> if there was no result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="this">The connection executing the command.</param>
        /// <param name="commandText">The command to execute.</param>
        /// <param name="commandParameters">Parameters to pass to the command. <c>null</c> values are replaced with <c>DBNull.Value</c>.</param>
        /// <returns>The scalar result of the command, or <c>(T)null</c> if there was no result.</returns>
        public static T ExecuteCommand<T>(this DbConnection @this, string commandText, params object[] commandParameters)
        {
            using (var command = @this.CreateCommand(commandText))
            {
                command.Parameters.AddRange(commandParameters.Select(x => command.CreateParameterValue(x)));
                var ret = command.ExecuteScalar();
                return (T)ret;
            }
        }

        /// <summary>
        /// Creates a new, unnamed command parameter with the specified value.
        /// </summary>
        /// <param name="this">The command receiving the parameter.</param>
        /// <param name="value">The value of the parameter. If <c>null</c>, then the parameter is given a value of <c>DBNull.Value</c>.</param>
        /// <returns>The new command parameter</returns>
        public static DbParameter CreateParameterValue<T>(this DbCommand @this, T value)
        {
            var ret = @this.CreateParameter();
            ret.Value = (object)value ?? DBNull.Value;
            return ret;
        }

        /// <summary>
        /// Adds a sequence of parameters to a parameter collection.
        /// </summary>
        /// <param name="this">The parameter collection to which to add the parameters.</param>
        /// <param name="parameters">The parameters to add.</param>
        public static void AddRange(this DbParameterCollection @this, IEnumerable<DbParameter> parameters)
        {
            @this.AddRange(parameters.ToArray());
        }

        /// <summary>
        /// Adds a sequence of values to the parameter collection of a command.
        /// </summary>
        /// <param name="this">The command containing the parameter collection to which to add the parameters.</param>
        /// <param name="parameterValues">The parameter values to add. Any <c>null</c> entries are given a value of <c>DBNull.Value</c>.</param>
        public static void AddParameterValues(this DbCommand @this, IEnumerable<object> parameterValues)
        {
            @this.Parameters.AddRange(parameterValues.Select(x => @this.CreateParameterValue(x)));
        }

        /// <summary>
        /// Adds a single parameter value to the parameter collection of a command.
        /// </summary>
        /// <param name="this">The command containing the parameter collection to which to add the parameter.</param>
        /// <param name="value">The value of the parameter. If <c>null</c>, then the parameter is given a value of <c>DBNull.Value</c>.</param>
        public static void AddParameterValue<T>(this DbCommand @this, T value)
        {
            @this.Parameters.Add(@this.CreateParameterValue(value));
        }
    }
}
