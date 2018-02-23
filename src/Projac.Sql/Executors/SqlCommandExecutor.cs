﻿using System;
using System.Collections.Generic;
#if !NETSTANDARD2_0
using System.Configuration;
#endif
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Projac.Sql.Executors
{
    /// <summary>
    ///     Represents the execution of commands against a data source.
    /// </summary>
    public class SqlCommandExecutor : 
        ISqlQueryCommandExecutor,
        ISqlNonQueryCommandExecutor,
        IAsyncSqlQueryCommandExecutor,
        IAsyncSqlNonQueryCommandExecutor
    {
        private readonly int _commandTimeout;
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _connectionString;

#if !NETSTANDARD2_0
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlCommandExecutor" /> class.
        /// </summary>
        /// <param name="settings">The connection string settings.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="settings" /> is <c>null</c>.</exception>
        public SqlCommandExecutor(ConnectionStringSettings settings, int commandTimeout = 30)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            _dbProviderFactory = DbProviderFactories.GetFactory(settings.ProviderName);
            _connectionString = settings.ConnectionString;
            _commandTimeout = commandTimeout;
        }
#endif

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlCommandExecutor" /> class.
        /// </summary>
        /// <param name="dbProviderFactory">The database provider factory.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="dbProviderFactory" /> or <paramref name="connectionString" /> is <c>null</c>.</exception>
        public SqlCommandExecutor(DbProviderFactory dbProviderFactory, string connectionString, int commandTimeout = 30)
        {
            if (dbProviderFactory == null) throw new ArgumentNullException(nameof(dbProviderFactory));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            _dbProviderFactory = dbProviderFactory;
            _connectionString = connectionString;
            _commandTimeout = commandTimeout;
        }

        /// <summary>
        ///     Executes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The number of <see cref="SqlNonQueryCommand">commands</see> executed.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public void ExecuteNonQuery(SqlNonQueryCommand command)
        {
            if (command == null) 
                throw new ArgumentNullException(nameof(command));

            using (var dbConnection = _dbProviderFactory.CreateConnection())
            {
                dbConnection.ConnectionString = _connectionString;
                dbConnection.Open();
                try
                {
                    using (var dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.Connection = dbConnection;
                        dbCommand.CommandTimeout = _commandTimeout;

                        dbCommand.CommandType = command.Type;
                        dbCommand.CommandText = command.Text;
                        dbCommand.Parameters.AddRange(command.Parameters);
                        dbCommand.ExecuteNonQuery();
                    }
                }
                finally
                {
                    dbConnection.Close();
                }
            }
        }

        /// <summary>
        ///     Executes the specified commands.
        /// </summary>
        /// <param name="commands">The commands to execute.</param>
        /// <returns>The number of <see cref="SqlNonQueryCommand">commands</see> executed.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="commands" /> are <c>null</c>.</exception>
        public int ExecuteNonQuery(IEnumerable<SqlNonQueryCommand> commands)
        {
            if (commands == null) 
                throw new ArgumentNullException(nameof(commands));

            using (var dbConnection = _dbProviderFactory.CreateConnection())
            {
                dbConnection.ConnectionString = _connectionString;
                dbConnection.Open();
                try
                {
                    using (var dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.Connection = dbConnection;
                        dbCommand.CommandTimeout = _commandTimeout;

                        var count = 0;
                        foreach (var command in commands)
                        {
                            dbCommand.CommandType = command.Type;
                            dbCommand.CommandText = command.Text;
                            dbCommand.Parameters.Clear();
                            dbCommand.Parameters.AddRange(command.Parameters);
                            dbCommand.ExecuteNonQuery();
                            count++;
                        }
                        return count;
                    }
                }
                finally
                {
                    dbConnection.Close();
                }
            }
        }

        /// <summary>
        ///     Executes the specified commands asynchronously.
        /// </summary>
        /// <param name="commands">The commands to execute.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return the number of <see cref="SqlNonQueryCommand">commands</see>
        ///     executed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="commands" /> are <c>null</c>.</exception>
        public Task<int> ExecuteNonQueryAsync(IEnumerable<SqlNonQueryCommand> commands)
        {
            return ExecuteNonQueryAsync(commands, CancellationToken.None);
        }

        /// <summary>
        ///     Executes the specified commands asynchronously.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return the number of <see cref="SqlNonQueryCommand">commands</see>
        ///     executed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="commands" /> are <c>null</c>.</exception>
        public async Task<int> ExecuteNonQueryAsync(IEnumerable<SqlNonQueryCommand> commands, CancellationToken cancellationToken)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            using (var dbConnection = _dbProviderFactory.CreateConnection())
            {
                dbConnection.ConnectionString = _connectionString;
                await dbConnection.OpenAsync(cancellationToken);
                try
                {
                    using (var dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.Connection = dbConnection;
                        dbCommand.CommandTimeout = _commandTimeout;
                        var count = 0;
                        foreach (var command in commands)
                        {
                            dbCommand.CommandType = command.Type;
                            dbCommand.CommandText = command.Text;
                            dbCommand.Parameters.Clear();
                            dbCommand.Parameters.AddRange(command.Parameters);
                            await dbCommand.ExecuteNonQueryAsync(cancellationToken);
                            count++;
                        }
                        return count;
                    }
                }
                finally
                {
                    dbConnection.Close();
                }
            }
        }

        /// <summary>
        ///     Executes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <see cref="DbDataReader"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public DbDataReader ExecuteReader(SqlQueryCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var dbConnection = _dbProviderFactory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;
            dbConnection.Open();
            try
            {
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandTimeout = _commandTimeout;

                    dbCommand.CommandType = command.Type;
                    dbCommand.CommandText = command.Text;
                    dbCommand.Parameters.AddRange(command.Parameters);
                    return dbCommand.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                }
            }
            catch
            {
                dbConnection.Close();
                throw;
            }
        }

        /// <summary>
        ///     Executes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <see cref="object">scalar value</see>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public object ExecuteScalar(SqlQueryCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var dbConnection = _dbProviderFactory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;
            dbConnection.Open();
            try
            {
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandTimeout = _commandTimeout;

                    dbCommand.CommandType = command.Type;
                    dbCommand.CommandText = command.Text;
                    dbCommand.Parameters.AddRange(command.Parameters);
                    return dbCommand.ExecuteScalar();
                }
            }
            catch
            {
                dbConnection.Close();
                throw;
            }
        }

        /// <summary>
        ///     Executes the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return a <see cref="object">scalar value</see>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public Task<object> ExecuteScalarAsync(SqlQueryCommand command)
        {
            return ExecuteScalarAsync(command, CancellationToken.None);
        }

        /// <summary>
        ///     Executes the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return a <see cref="object">scalar value</see>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public async Task<object> ExecuteScalarAsync(SqlQueryCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            using (var dbConnection = _dbProviderFactory.CreateConnection())
            {
                dbConnection.ConnectionString = _connectionString;
                await dbConnection.OpenAsync(cancellationToken);
                try
                {
                    using (var dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.Connection = dbConnection;
                        dbCommand.CommandTimeout = _commandTimeout;
                        dbCommand.CommandType = command.Type;
                        dbCommand.CommandText = command.Text;
                        dbCommand.Parameters.Clear();
                        dbCommand.Parameters.AddRange(command.Parameters);
                        return dbCommand.ExecuteScalarAsync(cancellationToken);
                    }
                }
                finally
                {
                    dbConnection.Close();
                }
            }
        }

        /// <summary>
        ///     Executes the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return a <see cref="DbDataReader">data reader</see>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public Task<DbDataReader> ExecuteReaderAsync(SqlQueryCommand command)
        {
            return ExecuteReaderAsync(command, CancellationToken.None);
        }

        /// <summary>
        ///     Executes the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return a <see cref="DbDataReader">data reader</see>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public async Task<DbDataReader> ExecuteReaderAsync(SqlQueryCommand command, CancellationToken cancellationToken)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var dbConnection = _dbProviderFactory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;
            await dbConnection.OpenAsync(cancellationToken);
            try
            {
                using (var dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandTimeout = _commandTimeout;

                    dbCommand.CommandType = command.Type;
                    dbCommand.CommandText = command.Text;
                    dbCommand.Parameters.AddRange(command.Parameters);
                    return await dbCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess, cancellationToken);
                }
            }
            catch
            {
                dbConnection.Close();
                throw;
            }
        }

        /// <summary>
        ///     Executes the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return when the <see cref="SqlNonQueryCommand">command</see>
        ///     was executed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public Task ExecuteNonQueryAsync(SqlNonQueryCommand command)
        {
            return ExecuteNonQueryAsync(command, CancellationToken.None);
        }

        /// <summary>
        ///     Executes the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task" /> that will return when the <see cref="SqlNonQueryCommand">command</see>
        ///     was executed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="command" /> is <c>null</c>.</exception>
        public async Task ExecuteNonQueryAsync(SqlNonQueryCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            using (var dbConnection = _dbProviderFactory.CreateConnection())
            {
                dbConnection.ConnectionString = _connectionString;
                await dbConnection.OpenAsync(cancellationToken);
                try
                {
                    using (var dbCommand = dbConnection.CreateCommand())
                    {
                        dbCommand.Connection = dbConnection;
                        dbCommand.CommandTimeout = _commandTimeout;
                        dbCommand.CommandType = command.Type;
                        dbCommand.CommandText = command.Text;
                        dbCommand.Parameters.AddRange(command.Parameters);
                        await dbCommand.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                finally
                {
                    dbConnection.Close();
                }
            }
        }
    }
}