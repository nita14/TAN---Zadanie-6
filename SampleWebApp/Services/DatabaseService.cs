using Microsoft.Extensions.Configuration;
using SampleWebApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SampleWebApp.Services
{
    public interface IDatabaseService
    {
        IEnumerable<Animal> GetAnimals();
        int CreateAnimal(Animal animal);
        Animal GetAnimal(int idAnimal);
        bool UpdateAnimal(int idAnimal, Animal animal);
        bool DeleteAnimal(int idAnimal);
    }

    public interface IDatabaseService2
    {
        Task<IEnumerable<Animal>> GetAnimalsByStoredProcedureAsync();
        Task<IEnumerable<Animal>> GetAnimalsAsync();
        Task<int> ChangeAnimalAsync();
    }

    public class DatabaseService : IDatabaseService
    {
        private IConfiguration _configuration;

        public DatabaseService(IConfiguration configuration) {
            _configuration = configuration;
        } 
     

        public IEnumerable<Animal> GetAnimals()
        {
            using var con = new SqlConnection(_configuration.GetConnectionString("Prod"));
            using var com = new SqlCommand("select * from animal", con);
            con.Open();
            var dr = com.ExecuteReader();
            var result = new List<Animal>();
            while (dr.Read())
            {
                Thread.Sleep(300);
                result.Add(new Animal
                {
                    IdAnimal = (int)dr["IdAnimal"],
                    Name = dr["Name"].ToString(),
                    Description = dr["Description"].ToString(),
                    Category = dr["Category"].ToString(),
                    Area = dr["Area"].ToString()

                });
            }
            return result;
            
        }

 

        public int CreateAnimal(Animal animal)
        {
            int newAnimalID = 0;

            using var con = new SqlConnection(_configuration.GetConnectionString("Prod"));
            using var com = new SqlCommand("INSERT INTO TAN.dbo.Animal (Name, Description, Category, Area) VALUES (@name ,@des, @cat, @area);SELECT CAST(scope_identity() AS int)", con);

            com.Parameters.AddWithValue("@name", animal.Name);
            com.Parameters.AddWithValue("@des", animal.Description);
            com.Parameters.AddWithValue("@cat", animal.Category);
            com.Parameters.AddWithValue("@area", animal.Area);

            try
            {
                con.Open();

                //Object o = com.ExecuteScalar();
                newAnimalID = (int) com.ExecuteScalar();
                return newAnimalID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return newAnimalID;
            }
            finally {

                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }

        public Animal GetAnimal(int idAnimal)
        {
            Animal animalFound = null;


            using var con = new SqlConnection(_configuration.GetConnectionString("Prod"));
            using var com = new SqlCommand("SELECT * FROM TAN.dbo.Animal WHERE idAnimal = @idAnimal", con);

            com.Parameters.AddWithValue("@idAnimal", idAnimal);


            try
            {
                con.Open();
                SqlDataReader dr = com.ExecuteReader();

                while (dr.Read()) {
                    animalFound = new Animal
                    {
                        IdAnimal = (int)dr["idAnimal"],
                        Name = dr["Name"].ToString(),
                        Description = dr["Description"].ToString(),
                        Area = dr["Area"].ToString()
                    };
                
              
                }
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return animalFound;
            }
            finally
            {

                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
                
            }


            return animalFound;
        }

        public bool UpdateAnimal(int idAnimal, Animal animal)
        {
            bool result = false;

            using var con = new SqlConnection(_configuration.GetConnectionString("Prod"));
            using var com = new SqlCommand("UPDATE TAN.dbo.Animal SET Name = @name, Description = @des , Category = @cat, Area = @area WHERE idAnimal = @idAnimal; SELECT @@ROWCOUNT;", con);

            com.Parameters.AddWithValue("@name", animal.Name);
            com.Parameters.AddWithValue("@des", animal.Description);
            com.Parameters.AddWithValue("@cat", animal.Category);
            com.Parameters.AddWithValue("@area", animal.Area);
            com.Parameters.AddWithValue("@idAnimal", idAnimal);

            try
            {
                con.Open();

                int updatedRows = (int) com.ExecuteNonQuery();

                   if (updatedRows == 1) {
                    result = true;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return result;
            }
            finally
            {

                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }

        public bool DeleteAnimal(int idAnimal)
        {
            bool isDeleted = false;
            using var con = new SqlConnection(_configuration.GetConnectionString("Prod"));
            using var com = new SqlCommand("DELETE FROM TAN.dbo.Animal WHERE idAnimal = @idAnimal; SELECT @@ROWCOUNT;", con);

            com.Parameters.AddWithValue("@idAnimal", idAnimal);

            try
            {
                con.Open();

                int deletedRows = (int)com.ExecuteNonQuery();
                if (deletedRows == 1)
                {
                    isDeleted = true;
                }

                return isDeleted;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return isDeleted;
            }
            finally
            {

                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }
    }



    public class DatabaseService2 : IDatabaseService2
    {
        public async Task<int> ChangeAnimalAsync()
        {
            using var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=pgago;Integrated Security=True");
            using var com = new SqlCommand("select * from animal", con);

            await con.OpenAsync();
            DbTransaction tran = await con.BeginTransactionAsync();
            com.Transaction = (SqlTransaction)tran;

            try
            {
                var list = new List<Animal>();
                using (var dr = await com.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        list.Add(new Animal
                        {
                            Name = dr["Name"].ToString(),
                            Description = dr["Description"].ToString()
                        });
                    }
                }

                com.Parameters.Clear();
                com.CommandText = "UPDATE Animal SET Name=Name+'a' WHERE Name=@Name";
                com.Parameters.AddWithValue("@Name", list[0].Name);
                await com.ExecuteNonQueryAsync();

                throw new Exception("Error");

                com.Parameters.Clear();
                com.Parameters.AddWithValue("@Name", list[1].Name);
                await com.ExecuteNonQueryAsync();

                await tran.CommitAsync();
            }
            catch (SqlException exc)
            {
                //...
                await tran.RollbackAsync();
            }
            catch (Exception exc)
            {
                //...
                await tran.RollbackAsync();
            }

            return 1;
        }

        public async Task<IEnumerable<Animal>> GetAnimalsAsync()
        {
            using var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=pgago;Integrated Security=True");
            using var com = new SqlCommand("select * from animal", con);
            await con.OpenAsync();
            var dr = await com.ExecuteReaderAsync();
            var result = new List<Animal>();
            while (await dr.ReadAsync())
            {
                await Task.Delay(300);
                result.Add(new Animal
                {
                    Name = dr["Name"].ToString(),
                    Description = dr["Description"].ToString()
                });
            }

            return result;
        }

        public async Task<IEnumerable<Animal>> GetAnimalsByStoredProcedureAsync()
        {
            using var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=pgago;Integrated Security=True");
            using var com = new SqlCommand("GetAnimals", con);
            com.CommandType = CommandType.StoredProcedure;

            await con.OpenAsync();
            var result = new List<Animal>();
            using (var dr = await com.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                {
                    result.Add(new Animal
                    {
                        Name = dr["Name"].ToString(),
                        Description = dr["Description"].ToString()
                    });
                }
            }

            return result;
        }
    }
}
