using LoanShark.Data;
using LoanShark.Domain;
using LoanShark.Repository;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using System.Data;

namespace LoanShark.Tests.Repository
{
    public class UserRepositoryTests
    {
        private readonly Mock<IDataLink> dataLinkMock;
        private readonly UserRepository userRepository;

        public UserRepositoryTests()
        {
            dataLinkMock = new Mock<IDataLink>();
            userRepository = new UserRepository(dataLinkMock.Object);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnUserWithId_WhenSuccessful()
        {
            // Arrange
            
            var user = new User(
                0,
                new Cnp("1234567890123"),
                "John",
                "Doe",
                new Email("john.doe@example.com"),
                new PhoneNumber("0712345678"),
                new HashedPassword("hashedpassT!123")
            );

            var sqlParamsCapture = new SqlParameter[8];
            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("CreateUser", It.IsAny<SqlParameter[]>()))
                .Callback<string, SqlParameter[]>((_, p) =>
                {
                    sqlParamsCapture = p;
                    var idUserParam = p.First(x => x.ParameterName == "@id_user");
                    idUserParam.Value = 42;
                })
                .ReturnsAsync(1);

            // Act
            var result = await userRepository.CreateUser(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(42, result.UserID);
        }
        [Fact]
        public async Task CreateUser_ShouldThrowException_WhenExecuteNonQueryFails()
        {
            // Arrange
            var user = new User(
                0,
                new Cnp("1234567890123"),
                "John",
                "Doe",
                new Email("john.doe@example.com"),
                new PhoneNumber("0712345678"),
                new HashedPassword("hashedpassT!123")
            );


            // Simulate exception thrown by ExecuteNonQuery
            dataLinkMock.Setup(dl => dl.ExecuteNonQuery("CreateUser", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Simulated DB failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                userRepository.CreateUser(user));

            Assert.Equal("Simulated DB failure", exception.Message);
        }



        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("cnp", typeof(string));
            table.Columns.Add("first_name", typeof(string));
            table.Columns.Add("last_name", typeof(string));
            table.Columns.Add("email", typeof(string));
            table.Columns.Add("phone_number", typeof(string));
            table.Columns.Add("hashed_password", typeof(string));
            table.Columns.Add("password_salt", typeof(string));

            table.Rows.Add("1234567890123", "John", "Doe", "john@example.com", "0712345678", "hashed", "salt");

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(table);

            // Act
            var result = await userRepository.GetUserById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNull_WhenNoUserIsFound()
        {
            // Arrange
            var emptyTable = new DataTable();
            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(emptyTable);

            // Act
            var result = await userRepository.GetUserById(99); 

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNull_WhenExceptionIsThrown()
        {
            // Arrange
            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserById", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Simulated DB error"));

            // Act
            var result = await userRepository.GetUserById(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByCnp_ShouldReturnUser_WhenFound()
        {
            // Arrange
            var cnp = "9876543210987"; 

            var dataTable = new DataTable();
            dataTable.Columns.Add("id_user", typeof(int));
            dataTable.Columns.Add("cnp", typeof(string));
            dataTable.Columns.Add("first_name", typeof(string));
            dataTable.Columns.Add("last_name", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("phone_number", typeof(string));
            dataTable.Columns.Add("hashed_password", typeof(string));
            dataTable.Columns.Add("password_salt", typeof(string));

            dataTable.Rows.Add(1, "9876543210987", "Bob", "Jones", "bob@example.com", "0711222333", "hashedPassword", "salt");

            
            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByCnp", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            // Act
            var user = await userRepository.GetUserByCnp(cnp);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user?.UserID);
            Assert.Equal("Bob", user?.FirstName);
            Assert.Equal("Jones", user?.LastName);
        }

        [Fact]
        public async Task GetUserByCnp_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var cnp = "9876543210987"; 
            var dataTable = new DataTable();

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByCnp", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);


            // Act
            var user = await userRepository.GetUserByCnp(cnp);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnUser_WhenFound()
        {
            // Arrange
            var email = "bob@example.com";  

            var dataTable = new DataTable();
            dataTable.Columns.Add("id_user", typeof(int));
            dataTable.Columns.Add("cnp", typeof(string));
            dataTable.Columns.Add("first_name", typeof(string));
            dataTable.Columns.Add("last_name", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("phone_number", typeof(string));
            dataTable.Columns.Add("hashed_password", typeof(string));
            dataTable.Columns.Add("password_salt", typeof(string));

            dataTable.Rows.Add(1, "9876543210987", "Bob", "Jones", email, "0711222333", "hashedPassword", "salt");

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByEmail", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            // Act
            var user = await userRepository.GetUserByEmail(email);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user?.UserID);
            Assert.Equal("Bob", user?.FirstName);
            Assert.Equal("Jones", user?.LastName);
            Assert.Equal(email, user?.Email.ToString());
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com"; 

            var dataTable = new DataTable();

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByEmail", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            // Act
            var user = await userRepository.GetUserByEmail(email);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            var email = "bob@example.com";  

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByEmail", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var user = await userRepository.GetUserByEmail(email);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByPhoneNumber_ShouldReturnUser_WhenFound()
        {
            // Arrange
            var phoneNumber = "0711222333"; 

            var dataTable = new DataTable();
            dataTable.Columns.Add("id_user", typeof(int));
            dataTable.Columns.Add("cnp", typeof(string));
            dataTable.Columns.Add("first_name", typeof(string));
            dataTable.Columns.Add("last_name", typeof(string));
            dataTable.Columns.Add("email", typeof(string));
            dataTable.Columns.Add("phone_number", typeof(string));
            dataTable.Columns.Add("hashed_password", typeof(string));
            dataTable.Columns.Add("password_salt", typeof(string));

            dataTable.Rows.Add(1, "9876543210987", "Bob", "Jones", "bob@example.com", phoneNumber, "hashedPassword", "salt");

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByPhoneNumber", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            // Act
            var user = await userRepository.GetUserByPhoneNumber(phoneNumber);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(1, user?.UserID);
            Assert.Equal("Bob", user?.FirstName);
            Assert.Equal("Jones", user?.LastName);
            Assert.Equal(phoneNumber, user?.PhoneNumber.ToString());
        }

        [Fact]
        public async Task GetUserByPhoneNumber_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var phoneNumber = "1234567890"; 

            var dataTable = new DataTable();


            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByPhoneNumber", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            // Act
            var user = await userRepository.GetUserByPhoneNumber(phoneNumber);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByPhoneNumber_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            var phoneNumber = "0711222333";

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetUserByPhoneNumber", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var user = await userRepository.GetUserByPhoneNumber(phoneNumber);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange

            var user = new User(
                1,
                new Cnp("1234567890123"),
                "John",
                "Doe",
                new Email("john.doe@example.com"),
                new PhoneNumber("0712345678"),
                new HashedPassword("hashedpass!2R#")
            );

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("UpdateUser", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(1);

            // Act
            var result = await userRepository.UpdateUser(user);

            // Assert
            Assert.True(result);
        }
        [Fact]
        public async Task UpdateUser_ShouldReturnFalse_WhenExceptionIsThrown()
        {
            // Arrange
            var user = new User(
                2,
                new Cnp("9876543210987"),
                "Bob",
                "Jones",
                new Email("bob@example.com"),
                new PhoneNumber("0711222333"),
                new HashedPassword("pw", "salt", false)
            );

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("UpdateUser", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database update failed"));

            // Act
            var result = await userRepository.UpdateUser(user);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnTrue_WhenSuccessful()
        {
            

            // Set up mock to return success
            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("DeleteUser", It.Is<SqlParameter[]>(p =>
                    (int)p.First(param => param.ParameterName == "@id_user").Value == 5)))
                .ReturnsAsync(1);


            var result = await userRepository.DeleteUser();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnFalse_WhenExceptionIsThrown()
        {
            

            // Throw exception when executing DeleteUser
            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("DeleteUser", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Something went wrong"));

            // Act
            var result = await userRepository.DeleteUser();

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task GetUserPasswordHashSalt_ShouldReturnHashAndSalt_WhenDataIsReturned()
        {
            // Arrange
            int userId = 10;
            string expectedHash = "mockedHash123";
            string expectedSalt = "mockedSalt456";

            // Simulate UserSession (assuming you can't inject it, we just override it manually for the test)
            UserSession.Instance.SetUserData("id_user", userId.ToString());

            var outputHashedPassword = new SqlParameter("@hashed_password", SqlDbType.VarChar, 255)
            {
                Direction = ParameterDirection.Output,
                Value = expectedHash
            };
            var outputSalt = new SqlParameter("@password_salt", SqlDbType.VarChar, 32)
            {
                Direction = ParameterDirection.Output,
                Value = expectedSalt
            };
            var inputUserId = new SqlParameter("@id_user", userId);

            var parameters = new[] { inputUserId, outputHashedPassword, outputSalt };

            // Setup ExecuteNonQuery to simulate filling in output parameters
            dataLinkMock.Setup(dl => dl.ExecuteNonQuery("GetHashedPassword", It.IsAny<SqlParameter[]>()))
                .Callback<string, SqlParameter[]>((_, actualParams) =>
                {
                    var hashedParam = actualParams.First(p => p.ParameterName == "@hashed_password");
                    var saltParam = actualParams.First(p => p.ParameterName == "@password_salt");

                    hashedParam.Value = expectedHash;
                    saltParam.Value = expectedSalt;
                })
                .ReturnsAsync(1);

            // Act
            var result = await userRepository.GetUserPasswordHashSalt();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal(expectedHash, result[0]);
            Assert.Equal(expectedSalt, result[1]);
        }
        [Fact]
        public async Task GetUserByCnp_ShouldReturnNull_WhenExceptionIsThrown()
        {
            // Arrange
            var mockDataLink = new Mock<IDataLink>();
            var repository = new UserRepository(mockDataLink.Object); // Replace with your actual repo name

            string testCnp = "1234567890123";
            mockDataLink
                .Setup(dl => dl.ExecuteReader("GetUserByCnp", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await repository.GetUserByCnp(testCnp);

            // Assert
            Assert.Null(result);
        }


    }

}
