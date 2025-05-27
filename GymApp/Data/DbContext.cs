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
            _connectionString = configuration.GetConnectionString("OracleConnection") ??
                throw new InvalidOperationException("Connection string not found");
        }

        private OracleConnection GetConnection() => new OracleConnection(_connectionString);

        // =================== MEMBERS CRUD ===================
        public async Task<List<Member>> GetMembersAsync()
        {
            var members = new List<Member>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("SELECT * FROM Members WHERE IsActive = 1 ORDER BY FullName", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                members.Add(new Member
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
                    Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes"),
                    CreatedDate = reader.GetDateTime("CreatedDate"),
                    UpdatedDate = reader.GetDateTime("UpdatedDate")
                });
            }
            return members;
        }

        public async Task<int> CreateMemberAsync(Member member)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                INSERT INTO Members (FullName, Phone, Email, Gender, DateOfBirth, Address, Notes, JoinDate, IsActive, CreatedDate, UpdatedDate)
                VALUES (:FullName, :Phone, :Email, :Gender, :DateOfBirth, :Address, :Notes, :JoinDate, :IsActive, SYSDATE, SYSDATE)
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

            var idParam = new OracleParameter(":Id", OracleDbType.Int32) { Direction = ParameterDirection.Output };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(idParam.Value);
        }

        public async Task UpdateMemberAsync(Member member)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                UPDATE Members SET FullName = :FullName, Phone = :Phone, Email = :Email, Gender = :Gender, 
                DateOfBirth = :DateOfBirth, Address = :Address, Notes = :Notes, UpdatedDate = SYSDATE
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

        // =================== PACKAGES CRUD ===================
        public async Task<List<Packages>> GetPackagesAsync()
        {
            var packages = new List<Packages>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("SELECT * FROM Packages WHERE IsActive = 1 ORDER BY PackageName", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                packages.Add(new Packages
                {
                    Id = reader.GetInt32("Id"),
                    PackageName = reader.GetString("PackageName"),
                    Description = reader.IsDBNull("Description") ? string.Empty : reader.GetString("Description"),
                    DurationDays = reader.GetInt32("DurationDays"),
                    Price = reader.GetDecimal("Price"),
                    IsActive = reader.GetInt32("IsActive") == 1,
                    CreatedDate = reader.GetDateTime("CreatedDate")
                });
            }
            return packages;
        }

        public async Task<int> CreatePackageAsync(Packages package)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                INSERT INTO Packages (PackageName, Description, DurationDays, Price, IsActive, CreatedDate)
                VALUES (:PackageName, :Description, :DurationDays, :Price, :IsActive, SYSDATE)
                RETURNING Id INTO :Id", connection);

            command.Parameters.Add(":PackageName", OracleDbType.NVarchar2).Value = package.PackageName;
            command.Parameters.Add(":Description", OracleDbType.NVarchar2).Value = (object?)package.Description ?? DBNull.Value;
            command.Parameters.Add(":DurationDays", OracleDbType.Int32).Value = package.DurationDays;
            command.Parameters.Add(":Price", OracleDbType.Decimal).Value = package.Price;
            command.Parameters.Add(":IsActive", OracleDbType.Int32).Value = 1;

            var idParam = new OracleParameter(":Id", OracleDbType.Int32) { Direction = ParameterDirection.Output };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(idParam.Value);
        }

        public async Task UpdatePackageAsync(Packages package)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                UPDATE Packages SET PackageName = :PackageName, Description = :Description, 
                DurationDays = :DurationDays, Price = :Price WHERE Id = :Id", connection);

            command.Parameters.Add(":PackageName", OracleDbType.NVarchar2).Value = package.PackageName;
            command.Parameters.Add(":Description", OracleDbType.NVarchar2).Value = (object?)package.Description ?? DBNull.Value;
            command.Parameters.Add(":DurationDays", OracleDbType.Int32).Value = package.DurationDays;
            command.Parameters.Add(":Price", OracleDbType.Decimal).Value = package.Price;
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = package.Id;

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeletePackageAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = new OracleCommand("UPDATE Packages SET IsActive = 0 WHERE Id = :Id", connection);
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = id;
            await command.ExecuteNonQueryAsync();
        }

        // =================== MEMBERSHIP CARDS CRUD ===================
        public async Task<List<MembershipCards>> GetMembershipCardsAsync()
        {
            var cards = new List<MembershipCards>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                SELECT mc.*, m.FullName as MemberName, p.PackageName 
                FROM MembershipCards mc
                JOIN Members m ON mc.MemberId = m.Id
                JOIN Packages p ON mc.PackageId = p.Id
                ORDER BY mc.CreatedDate DESC", connection);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                cards.Add(new MembershipCards
                {
                    Id = reader.GetInt32("Id"),
                    MemberId = reader.GetInt32("MemberId"),
                    PackageId = reader.GetInt32("PackageId"),
                    StartDate = reader.GetDateTime("StartDate"),
                    EndDate = reader.GetDateTime("EndDate"),
                    Price = reader.GetDecimal("Price"),
                    PaymentMethod = reader.IsDBNull("PaymentMethod") ? "Tiền mặt" : reader.GetString("PaymentMethod"),
                    Status = reader.IsDBNull("Status") ? "Hoạt động" : reader.GetString("Status"),
                    Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes"),
                    CreatedDate = reader.GetDateTime("CreatedDate"),
                    CreatedBy = reader.IsDBNull("CreatedBy") ? "Admin" : reader.GetString("CreatedBy"),
                    MemberName = reader.GetString("MemberName"),
                    PackageName = reader.GetString("PackageName")
                });
            }
            return cards;
        }

        public async Task<int> CreateMembershipCardAsync(MembershipCards card)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                INSERT INTO MembershipCards (MemberId, PackageId, StartDate, EndDate, Price, PaymentMethod, Status, Notes, CreatedDate, CreatedBy)
                VALUES (:MemberId, :PackageId, :StartDate, :EndDate, :Price, :PaymentMethod, :Status, :Notes, SYSDATE, :CreatedBy)
                RETURNING Id INTO :Id", connection);

            command.Parameters.Add(":MemberId", OracleDbType.Int32).Value = card.MemberId;
            command.Parameters.Add(":PackageId", OracleDbType.Int32).Value = card.PackageId;
            command.Parameters.Add(":StartDate", OracleDbType.Date).Value = card.StartDate;
            command.Parameters.Add(":EndDate", OracleDbType.Date).Value = card.EndDate;
            command.Parameters.Add(":Price", OracleDbType.Decimal).Value = card.Price;
            command.Parameters.Add(":PaymentMethod", OracleDbType.NVarchar2).Value = card.PaymentMethod;
            command.Parameters.Add(":Status", OracleDbType.NVarchar2).Value = card.Status;
            command.Parameters.Add(":Notes", OracleDbType.NVarchar2).Value = (object?)card.Notes ?? DBNull.Value;
            command.Parameters.Add(":CreatedBy", OracleDbType.NVarchar2).Value = card.CreatedBy;

            var idParam = new OracleParameter(":Id", OracleDbType.Int32) { Direction = ParameterDirection.Output };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(idParam.Value);
        }

        public async Task UpdateMembershipCardAsync(MembershipCards card)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                UPDATE MembershipCards SET MemberId = :MemberId, PackageId = :PackageId, StartDate = :StartDate, 
                EndDate = :EndDate, Price = :Price, PaymentMethod = :PaymentMethod, Status = :Status, Notes = :Notes
                WHERE Id = :Id", connection);

            command.Parameters.Add(":MemberId", OracleDbType.Int32).Value = card.MemberId;
            command.Parameters.Add(":PackageId", OracleDbType.Int32).Value = card.PackageId;
            command.Parameters.Add(":StartDate", OracleDbType.Date).Value = card.StartDate;
            command.Parameters.Add(":EndDate", OracleDbType.Date).Value = card.EndDate;
            command.Parameters.Add(":Price", OracleDbType.Decimal).Value = card.Price;
            command.Parameters.Add(":PaymentMethod", OracleDbType.NVarchar2).Value = card.PaymentMethod;
            command.Parameters.Add(":Status", OracleDbType.NVarchar2).Value = card.Status;
            command.Parameters.Add(":Notes", OracleDbType.NVarchar2).Value = (object?)card.Notes ?? DBNull.Value;
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = card.Id;

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteMembershipCardAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = new OracleCommand("DELETE FROM MembershipCards WHERE Id = :Id", connection);
            command.Parameters.Add(":Id", OracleDbType.Int32).Value = id;
            await command.ExecuteNonQueryAsync();
        }

        // =================== STAFF CRUD ===================
        public async Task<List<Staff>> GetStaffAsync()
        {
            var staff = new List<Staff>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand("SELECT * FROM Staff WHERE IsActive = 1 ORDER BY FullName", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                staff.Add(new Staff
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
                    Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes"),
                    CreatedDate = reader.GetDateTime("CreatedDate")
                });
            }
            return staff;
        }

        public async Task<int> CreateStaffAsync(Staff staff)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                INSERT INTO Staff (FullName, Phone, Email, Role, StartDate, Salary, Address, Notes, IsActive, CreatedDate)
                VALUES (:FullName, :Phone, :Email, :Role, :StartDate, :Salary, :Address, :Notes, :IsActive, SYSDATE)
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

            var idParam = new OracleParameter(":Id", OracleDbType.Int32) { Direction = ParameterDirection.Output };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();
            return Convert.ToInt32(idParam.Value);
        }

        public async Task UpdateStaffAsync(Staff staff)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = new OracleCommand(@"
                UPDATE Staff SET FullName = :FullName, Phone = :Phone, Email = :Email, Role = :Role, 
                StartDate = :StartDate, Salary = :Salary, Address = :Address, Notes = :Notes
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