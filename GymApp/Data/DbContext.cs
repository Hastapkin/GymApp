using Oracle.ManagedDataAccess.Client;
using GymApp.Models;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GymApp.Data
{
    public class DbContext
    {
        private readonly string _connectionString;

        public DbContext()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();
            _connectionString = configuration.GetConnectionString("OracleConnection") ?? throw new InvalidOperationException("Connection string not found");
        }

        private OracleConnection GetConnection()
        {
            return new OracleConnection(_connectionString);
        }

        // Member CRUD Operations
        public async Task<List<Models.Member>> GetMembersAsync()
        {
            var members = new List<Models.Member>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("SELECT * FROM Members WHERE IsActive = 1 ORDER BY FullName", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                members.Add(new Models.Member
                {
                    Id = reader.GetInt32("Id"),
                    FullName = reader.GetString("FullName"),
                    Phone = reader.IsDBNull("Phone") ? string.Empty : reader.GetString("Phone"),
                    Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString("Email"),
                    Gender = reader.IsDBNull("Gender") ? string.Empty : reader.GetString("Gender"),
                    DateOfBirth = reader.IsDBNull("DateOfBirth") ? null : reader.GetDateTime("DateOfBirth"),
                    Address = reader.IsDBNull("Address") ? string.Empty : reader.GetString("Address"),
                    JoinDate = reader.GetDateTime("JoinDate"),
                    IsActive = reader.GetInt32("IsActive") == 1,
                    Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes")
                });
            }

            return members;
        }

        public async Task<int> CreateMemberAsync(Models.Member member)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                INSERT INTO Members (FullName, Phone, Email, Gender, DateOfBirth, Address, Notes, JoinDate, IsActive)
                VALUES (:FullName, :Phone, :Email, :Gender, :DateOfBirth, :Address, :Notes, :JoinDate, :IsActive)
                RETURNING Id INTO :Id", connection);

            command.Parameters.Add(":FullName", OracleDbType.NVarchar2).Value = member.FullName;
            command.Parameters.Add(":Phone", OracleDbType.NVarchar2).Value = (object?)member.Phone ?? DBNull.Value;
            command.Parameters.Add(":Email", OracleDbType.NVarchar2).Value = (object?)member.Email ?? DBNull.Value;
            command.Parameters.Add(":Gender", OracleDbType.NVarchar2).Value = (object?)member.Gender ?? DBNull.Value;
            command.Parameters.Add(":DateOfBirth", OracleDbType.Date).Value = (object?)member.DateOfBirth ?? DBNull.Value;
            command.Parameters.Add(":Address", OracleDbType.NVarchar2).Value = (object?)member.Address ?? DBNull.Value;
            command.Parameters.Add(":Notes", OracleDbType.NVarchar2).Value = (object?)member.Notes ?? DBNull.Value;
            command.Parameters.Add(":JoinDate", OracleDbType.Date).Value = DateTime.Now;
            command.Parameters.Add(":IsActive", OracleDbType.Int32).Value = 1;

            var idParam = new OracleParameter(":Id", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(idParam.Value);
        }

        public async Task UpdateMemberAsync(Models.Member member)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                UPDATE Members SET 
                    FullName = :FullName, 
                    Phone = :Phone, 
                    Email = :Email, 
                    Gender = :Gender, 
                    DateOfBirth = :DateOfBirth, 
                    Address = :Address, 
                    Notes = :Notes,
                    UpdatedDate = SYSDATE
                WHERE Id = :Id", connection);

            command.Parameters.Add(":FullName", OracleDbType.NVarchar2).Value = member.FullName;
            command.Parameters.Add(":Phone", OracleDbType.NVarchar2).Value = (object?)member.Phone ?? DBNull.Value;
            command.Parameters.Add(":Email", OracleDbType.NVarchar2).Value = (object?)member.Email ?? DBNull.Value;
            command.Parameters.Add(":Gender", OracleDbType.NVarchar2).Value = (object?)member.Gender ?? DBNull.Value;
            command.Parameters.Add(":DateOfBirth", OracleDbType.Date).Value = (object?)member.DateOfBirth ?? DBNull.Value;
            command.Parameters.Add(":Address", OracleDbType.NVarchar2).Value = (object?)member.Address ?? DBNull.Value;
            command.Parameters.Add(":Notes", OracleDbType.NVarchar2).Value = (object?)member.Notes ?? DBNull.Value;
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = member.Id;

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteMemberAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("UPDATE Members SET IsActive = 0 WHERE Id = :Id", connection);
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = id;

            await command.ExecuteNonQueryAsync();
        }

        // Membership CRUD Operations
        public async Task<List<Models.Membership>> GetMembershipsAsync()
        {
            var memberships = new List<Models.Membership>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("SELECT * FROM Packages WHERE IsActive = 1 ORDER BY PackageName", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                memberships.Add(new Models.Membership
                {
                    Id = reader.GetInt32("Id"),
                    PackageName = reader.GetString("PackageName"),
                    Description = reader.IsDBNull("Description") ? string.Empty : reader.GetString("Description"),
                    DurationDays = reader.GetInt32("DurationDays"),
                    Price = reader.GetDecimal("Price"),
                    IsActive = reader.GetInt32("IsActive") == 1
                });
            }

            return memberships;
        }

        public async Task<int> CreateMembershipAsync(Models.Membership membership)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                INSERT INTO Packages (PackageName, Description, DurationDays, Price, IsActive)
                VALUES (:PackageName, :Description, :DurationDays, :Price, :IsActive)
                RETURNING Id INTO :Id", connection);

            command.Parameters.Add(":PackageName", OracleDbType.NVarchar2).Value = membership.PackageName;
            command.Parameters.Add(":Description", OracleDbType.NVarchar2).Value = (object?)membership.Description ?? DBNull.Value;
            command.Parameters.Add(":DurationDays", OracleDbType.Int32).Value = membership.DurationDays;
            command.Parameters.Add(":Price", OracleDbType.Decimal).Value = membership.Price;
            command.Parameters.Add(":IsActive", OracleDbType.Int32).Value = 1;

            var idParam = new OracleParameter(":Id", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(idParam.Value);
        }

        public async Task UpdateMembershipAsync(Models.Membership membership)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                UPDATE Packages SET 
                    PackageName = :PackageName, 
                    Description = :Description, 
                    DurationDays = :DurationDays, 
                    Price = :Price
                WHERE Id = :Id", connection);

            command.Parameters.Add(":PackageName", OracleDbType.NVarchar2).Value = membership.PackageName;
            command.Parameters.Add(":Description", OracleDbType.NVarchar2).Value = (object?)membership.Description ?? DBNull.Value;
            command.Parameters.Add(":DurationDays", OracleDbType.Int32).Value = membership.DurationDays;
            command.Parameters.Add(":Price", OracleDbType.Decimal).Value = membership.Price;
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = membership.Id;

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteMembershipAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("UPDATE Packages SET IsActive = 0 WHERE Id = :Id", connection);
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = id;

            await command.ExecuteNonQueryAsync();
        }

        // Staff CRUD Operations
        public async Task<List<Models.Staff>> GetStaffAsync()
        {
            var staffList = new List<Models.Staff>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("SELECT * FROM Staff WHERE IsActive = 1 ORDER BY FullName", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                staffList.Add(new Models.Staff
                {
                    Id = reader.GetInt32("Id"),
                    FullName = reader.GetString("FullName"),
                    Phone = reader.IsDBNull("Phone") ? string.Empty : reader.GetString("Phone"),
                    Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString("Email"),
                    Role = reader.GetString("Role"),
                    StartDate = reader.GetDateTime("StartDate"),
                    Salary = reader.GetDecimal("Salary"),
                    Address = reader.IsDBNull("Address") ? string.Empty : reader.GetString("Address"),
                    IsActive = reader.GetInt32("IsActive") == 1,
                    Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes")
                });
            }

            return staffList;
        }

        public async Task<int> CreateStaffAsync(Models.Staff staff)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                INSERT INTO Staff (FullName, Phone, Email, Role, StartDate, Salary, Address, Notes, IsActive)
                VALUES (:FullName, :Phone, :Email, :Role, :StartDate, :Salary, :Address, :Notes, :IsActive)
                RETURNING Id INTO :Id", connection);

            command.Parameters.Add(":FullName", OracleDbType.NVarchar2).Value = staff.FullName;
            command.Parameters.Add(":Phone", OracleDbType.NVarchar2).Value = (object?)staff.Phone ?? DBNull.Value;
            command.Parameters.Add(":Email", OracleDbType.NVarchar2).Value = (object?)staff.Email ?? DBNull.Value;
            command.Parameters.Add(":Role", OracleDbType.NVarchar2).Value = staff.Role;
            command.Parameters.Add(":StartDate", OracleDbType.Date).Value = staff.StartDate;
            command.Parameters.Add(":Salary", OracleDbType.Decimal).Value = staff.Salary;
            command.Parameters.Add(":Address", OracleDbType.NVarchar2).Value = (object?)staff.Address ?? DBNull.Value;
            command.Parameters.Add(":Notes", OracleDbType.NVarchar2).Value = (object?)staff.Notes ?? DBNull.Value;
            command.Parameters.Add(":IsActive", OracleDbType.Int32).Value = 1;

            var idParam = new OracleParameter(":Id", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(idParam.Value);
        }

        public async Task UpdateStaffAsync(Models.Staff staff)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                UPDATE Staff SET 
                    FullName = :FullName, 
                    Phone = :Phone, 
                    Email = :Email, 
                    Role = :Role, 
                    StartDate = :StartDate, 
                    Salary = :Salary, 
                    Address = :Address, 
                    Notes = :Notes
                WHERE Id = :Id", connection);

            command.Parameters.Add(":FullName", OracleDbType.NVarchar2).Value = staff.FullName;
            command.Parameters.Add(":Phone", OracleDbType.NVarchar2).Value = (object?)staff.Phone ?? DBNull.Value;
            command.Parameters.Add(":Email", OracleDbType.NVarchar2).Value = (object?)staff.Email ?? DBNull.Value;
            command.Parameters.Add(":Role", OracleDbType.NVarchar2).Value = staff.Role;
            command.Parameters.Add(":StartDate", OracleDbType.Date).Value = staff.StartDate;
            command.Parameters.Add(":Salary", OracleDbType.Decimal).Value = staff.Salary;
            command.Parameters.Add(":Address", OracleDbType.NVarchar2).Value = (object?)staff.Address ?? DBNull.Value;
            command.Parameters.Add(":Notes", OracleDbType.NVarchar2).Value = (object?)staff.Notes ?? DBNull.Value;
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = staff.Id;

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteStaffAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("UPDATE Staff SET IsActive = 0 WHERE Id = :Id", connection);
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = id;

            await command.ExecuteNonQueryAsync();
        }
    }
}