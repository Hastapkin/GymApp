using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using GymApp.Models;

namespace GymApp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = ConfigurationService.Instance.GetConnectionString();
        }

        public void InitializeDatabase()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    // Test connection successful
                }
            }
            catch (OracleException ex)
            {
                throw new Exception($"❌ Oracle Database connection failed!\n\n" +
                                  $"Error: {ex.Message}\n" +
                                  $"Code: {ex.Number}\n\n" +
                                  $"🔧 Checklist:\n" +
                                  $"✓ Oracle Database is running\n" +
                                  $"✓ User C##GymApp exists\n" +
                                  $"✓ Password is 'a123'\n" +
                                  $"✓ Service name is 'FREE'\n" +
                                  $"✓ Port 1521 is open");
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Database connection failed: {ex.Message}");
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Members operations
        public List<Member> GetAllMembers()
        {
            var members = new List<Member>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand("SELECT * FROM Members WHERE IsActive = 1 ORDER BY FullName", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(new Member
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FullName = reader["FullName"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Gender = reader["Gender"]?.ToString() ?? "",
                                DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? null : Convert.ToDateTime(reader["DateOfBirth"]),
                                Address = reader["Address"]?.ToString() ?? "",
                                JoinDate = Convert.ToDateTime(reader["JoinDate"]),
                                IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                                Notes = reader["Notes"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error loading members: {ex.Message}");
            }
            return members;
        }

        public void AddMember(Member member)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand(@"
                        INSERT INTO Members (FullName, Phone, Email, Gender, DateOfBirth, Address, JoinDate, IsActive, Notes) 
                        VALUES (:fullName, :phone, :email, :gender, :dateOfBirth, :address, :joinDate, :isActive, :notes)", connection);

                    command.Parameters.Add(":fullName", member.FullName);
                    command.Parameters.Add(":phone", (object)member.Phone ?? DBNull.Value);
                    command.Parameters.Add(":email", (object)member.Email ?? DBNull.Value);
                    command.Parameters.Add(":gender", (object)member.Gender ?? DBNull.Value);
                    command.Parameters.Add(":dateOfBirth", (object)member.DateOfBirth ?? DBNull.Value);
                    command.Parameters.Add(":address", (object)member.Address ?? DBNull.Value);
                    command.Parameters.Add(":joinDate", member.JoinDate);
                    command.Parameters.Add(":isActive", member.IsActive ? 1 : 0);
                    command.Parameters.Add(":notes", (object)member.Notes ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error adding member: {ex.Message}");
            }
        }

        public void UpdateMember(Member member)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand(@"
                        UPDATE Members SET 
                            FullName = :fullName, Phone = :phone, Email = :email, Gender = :gender, 
                            DateOfBirth = :dateOfBirth, Address = :address, Notes = :notes,
                            UpdatedDate = SYSDATE 
                        WHERE Id = :id", connection);

                    command.Parameters.Add(":fullName", member.FullName);
                    command.Parameters.Add(":phone", (object)member.Phone ?? DBNull.Value);
                    command.Parameters.Add(":email", (object)member.Email ?? DBNull.Value);
                    command.Parameters.Add(":gender", (object)member.Gender ?? DBNull.Value);
                    command.Parameters.Add(":dateOfBirth", (object)member.DateOfBirth ?? DBNull.Value);
                    command.Parameters.Add(":address", (object)member.Address ?? DBNull.Value);
                    command.Parameters.Add(":notes", (object)member.Notes ?? DBNull.Value);
                    command.Parameters.Add(":id", member.Id);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error updating member: {ex.Message}");
            }
        }

        public void DeleteMember(int memberId)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand("UPDATE Members SET IsActive = 0 WHERE Id = :id", connection);
                    command.Parameters.Add(":id", memberId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error deleting member: {ex.Message}");
            }
        }

        // MembershipCards operations
        public List<MembershipCard> GetAllMembershipCards()
        {
            var cards = new List<MembershipCard>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand(@"
                        SELECT mc.*, m.FullName as MemberName, p.PackageName 
                        FROM MembershipCards mc 
                        JOIN Members m ON mc.MemberId = m.Id 
                        JOIN Packages p ON mc.PackageId = p.Id
                        ORDER BY mc.EndDate DESC", connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cards.Add(new MembershipCard
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                MemberId = Convert.ToInt32(reader["MemberId"]),
                                MemberName = reader["MemberName"]?.ToString() ?? "",
                                PackageId = Convert.ToInt32(reader["PackageId"]),
                                PackageName = reader["PackageName"]?.ToString() ?? "",
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                EndDate = Convert.ToDateTime(reader["EndDate"]),
                                Price = Convert.ToDecimal(reader["Price"]),
                                PaymentMethod = reader["PaymentMethod"]?.ToString() ?? "Tiền mặt",
                                Status = reader["Status"]?.ToString() ?? "Hoạt động",
                                Notes = reader["Notes"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error loading membership cards: {ex.Message}");
            }
            return cards;
        }

        public void AddMembershipCard(MembershipCard card)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand(@"
                        INSERT INTO MembershipCards (MemberId, PackageId, StartDate, EndDate, Price, PaymentMethod, Status, Notes) 
                        VALUES (:memberId, :packageId, :startDate, :endDate, :price, :paymentMethod, :status, :notes)", connection);

                    command.Parameters.Add(":memberId", card.MemberId);
                    command.Parameters.Add(":packageId", card.PackageId);
                    command.Parameters.Add(":startDate", card.StartDate);
                    command.Parameters.Add(":endDate", card.EndDate);
                    command.Parameters.Add(":price", card.Price);
                    command.Parameters.Add(":paymentMethod", card.PaymentMethod);
                    command.Parameters.Add(":status", card.Status);
                    command.Parameters.Add(":notes", (object)card.Notes ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error adding membership card: {ex.Message}");
            }
        }

        // Staff operations
        public List<Staff> GetAllStaff()
        {
            var staff = new List<Staff>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand("SELECT * FROM Staff WHERE IsActive = 1 ORDER BY FullName", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            staff.Add(new Staff
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FullName = reader["FullName"]?.ToString() ?? "",
                                Phone = reader["Phone"]?.ToString() ?? "",
                                Email = reader["Email"]?.ToString() ?? "",
                                Role = reader["Role"]?.ToString() ?? "",
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                Salary = reader["Salary"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Salary"]),
                                Address = reader["Address"]?.ToString() ?? "",
                                IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                                Notes = reader["Notes"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error loading staff: {ex.Message}");
            }
            return staff;
        }

        public void AddStaff(Staff staff)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand(@"
                        INSERT INTO Staff (FullName, Phone, Email, Role, StartDate, Salary, Address, IsActive, Notes) 
                        VALUES (:fullName, :phone, :email, :role, :startDate, :salary, :address, :isActive, :notes)", connection);

                    command.Parameters.Add(":fullName", staff.FullName);
                    command.Parameters.Add(":phone", (object)staff.Phone ?? DBNull.Value);
                    command.Parameters.Add(":email", (object)staff.Email ?? DBNull.Value);
                    command.Parameters.Add(":role", staff.Role);
                    command.Parameters.Add(":startDate", staff.StartDate);
                    command.Parameters.Add(":salary", staff.Salary);
                    command.Parameters.Add(":address", (object)staff.Address ?? DBNull.Value);
                    command.Parameters.Add(":isActive", staff.IsActive ? 1 : 0);
                    command.Parameters.Add(":notes", (object)staff.Notes ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error adding staff: {ex.Message}");
            }
        }

        public void UpdateStaff(Staff staff)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand(@"
                        UPDATE Staff SET 
                            FullName = :fullName, Phone = :phone, Email = :email, Role = :role, 
                            Salary = :salary, Address = :address, Notes = :notes 
                        WHERE Id = :id", connection);

                    command.Parameters.Add(":fullName", staff.FullName);
                    command.Parameters.Add(":phone", (object)staff.Phone ?? DBNull.Value);
                    command.Parameters.Add(":email", (object)staff.Email ?? DBNull.Value);
                    command.Parameters.Add(":role", staff.Role);
                    command.Parameters.Add(":salary", staff.Salary);
                    command.Parameters.Add(":address", (object)staff.Address ?? DBNull.Value);
                    command.Parameters.Add(":notes", (object)staff.Notes ?? DBNull.Value);
                    command.Parameters.Add(":id", staff.Id);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error updating staff: {ex.Message}");
            }
        }

        public void DeleteStaff(int staffId)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand("UPDATE Staff SET IsActive = 0 WHERE Id = :id", connection);
                    command.Parameters.Add(":id", staffId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error deleting staff: {ex.Message}");
            }
        }

        // Packages operations  
        public List<Package> GetAllPackages()
        {
            var packages = new List<Package>();
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var command = new OracleCommand("SELECT * FROM Packages WHERE IsActive = 1 ORDER BY DurationDays", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            packages.Add(new Package
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                PackageName = reader["PackageName"]?.ToString() ?? "",
                                Description = reader["Description"]?.ToString() ?? "",
                                DurationDays = Convert.ToInt32(reader["DurationDays"]),
                                Price = Convert.ToDecimal(reader["Price"]),
                                IsActive = Convert.ToInt32(reader["IsActive"]) == 1
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error loading packages: {ex.Message}");
            }
            return packages;
        }
    }
}