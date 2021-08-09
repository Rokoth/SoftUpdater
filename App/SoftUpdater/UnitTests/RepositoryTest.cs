using Microsoft.Extensions.DependencyInjection;
using SoftUpdater.Db.Context;
using SoftUpdater.Db.Interface;
using SoftUpdater.Db.Model;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TaskCollector.UnitTests
{
    /// <summary>
    /// Тесты методов репозиториев БД
    /// </summary>
    public class RepositoryTest : IClassFixture<CustomFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public RepositoryTest(CustomFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
        }

        /// <summary>
        /// Тест получения списка сущностей по фильтру
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();
            AddUsers(context, "user_select_{0}", "user_description_{0}", "user_login_{0}", "user_password_{0}", 10);
            AddUsers(context, "user_not_select_{0}", "user_description_{0}", "user_login_{0}", "user_password_{0}", 10);            
            await context.SaveChangesAsync();

            var repo = _serviceProvider.GetRequiredService<IRepository<User>>();
            var data = await repo.GetAsync(new Filter<User>()
            {
                Page = 0,
                Size = 10,
                Selector = s => s.Name.Contains("user_select")
            }, CancellationToken.None);

            Assert.Equal(10, data.Data.Count());
            foreach (var item in data.Data)
            {
                Assert.Contains("user_select", item.Name);
            }
        }

        /// <summary>
        /// Тест получения сущности по id
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetItemTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();
            var user = CreateUser("user_{0}", "user_description_{0}", "user_login_{0}", "user_password_{0}");
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = _serviceProvider.GetRequiredService<IRepository<User>>();
            var data = await repo.GetAsync(user.Id, CancellationToken.None);

            Assert.NotNull(data);
            Assert.Equal(user.Id, data.Id);
        }

        /// <summary>
        /// Тест добавления сущности
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();            
            var repo = _serviceProvider.GetRequiredService<IRepository<User>>();
            var user = CreateUser("user_{0}", "user_description_{0}", "user_login_{0}", "user_password_{0}");

            var result = await repo.AddAsync(user, true, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(user.Name, result.Name);
            
            var actual = context.Users.FirstOrDefault();
            Assert.NotNull(actual);
            Assert.Equal(user.Name, actual.Name);
        }

        /// <summary>
        /// Тест удаления сущности
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteTest()
        {
            var context = _serviceProvider.GetRequiredService<DbPgContext>();
            var repo = _serviceProvider.GetRequiredService<IRepository<User>>();
            var user = CreateUser("user_{0}", "user_description_{0}", "user_login_{0}", "user_password_{0}");

            context.Users.Add(user);
            await context.SaveChangesAsync();
                        
            var data = await repo.DeleteAsync(user, true, CancellationToken.None);

            Assert.NotNull(data);
            Assert.Equal(user.Id, data.Id);
            Assert.True(data.IsDeleted);

            var test = context.Users.FirstOrDefault(s => s.Id == user.Id);
            Assert.NotNull(test);
            Assert.Equal(user.Id, test.Id);
            Assert.True(test.IsDeleted);
        }

        private void AddUsers(DbPgContext context, string nameMask, string descriptionMask, string loginMask, string passwordMask, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var user = CreateUser(nameMask, descriptionMask, loginMask, passwordMask);
                context.Users.Add(user);
            }
        }

        private User CreateUser(string nameMask, string descriptionMask, string loginMask, string passwordMask)
        {
            var id = Guid.NewGuid();
            var user = new User()
            {
                Name = string.Format(nameMask, id),
                Id = id,
                Description = string.Format(descriptionMask, id),
                IsDeleted = false,
                Login = string.Format(loginMask, id),
                Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Format(passwordMask, id))),
                VersionDate = DateTimeOffset.Now
            };
            return user;
        }
    }
}
