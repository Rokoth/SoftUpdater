using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using SoftUpdater.Contract.Model;
using SoftUpdater.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SoftUpdater.UnitTests
{
    /// <summary>
    /// api unit tests
    /// </summary>
    public class APITest : IClassFixture<CustomFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public APITest(CustomFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
        }

        /// <summary>
        /// AuthController. Test for Auth method (positive scenario)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ApiAuthTest()
        {            
            var user = await AddUser();
            var client = await AddClient(user.Id);
            await AuthAndAssert(client);
        }
                
        private async Task<ClientIdentityResponse> AuthAndAssert(Db.Model.Client client)
        {
            var clientController = new AuthController(_serviceProvider);
            var result = await clientController.Auth(new Contract.Model.ClientIdentity()
            {
                Login = client.Login,
                Password = $"client_password_{client.Id}"
            });
            var response = result as OkObjectResult;
            Assert.NotNull(response);
            var value = JObject.FromObject(response.Value).ToObject<ClientIdentityResponse>();
            Assert.Equal(client.Id.ToString(), value.UserName);
            return value;
        }
                
        private async Task<Db.Model.User> AddUser()
        {
            var context = _serviceProvider.GetRequiredService<Db.Context.DbPgContext>();
            var user = CreateUser();
            context.Set<Db.Model.User>().Add(user);
            await context.SaveChangesAsync();
            return user;
        }
            

        private Db.Model.User CreateUser()
        {
            var user_id = Guid.NewGuid();
            return new Db.Model.User()
            {
                Name = $"user_{user_id}",
                Id = user_id,
                Description = $"user_description_{user_id}",
                IsDeleted = false,
                Login = $"user_login_{user_id}",
                Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes($"user_password_{user_id}")),
                VersionDate = DateTimeOffset.Now
            };
        }

        private async Task<Db.Model.Client> AddClient(Guid userId)
        {
            var context = _serviceProvider.GetRequiredService<Db.Context.DbPgContext>();
            var client = CreateClient(userId);
            context.Set<Db.Model.Client>().Add(client);
            await context.SaveChangesAsync();
            return client;
        }


        private Db.Model.Client CreateClient(Guid userId)
        {
            var client_id = Guid.NewGuid();
            return new Db.Model.Client()
            {
                Name = $"client_{client_id}",
                Id = client_id,
                Description = $"client_description_{client_id}",
                IsDeleted = false,
                Login = $"client_login_{client_id}",
                Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes($"client_password_{client_id}")),
                VersionDate = DateTimeOffset.Now,
                BasePath = "test",
                UserId = userId
            };
        }
    }
}
