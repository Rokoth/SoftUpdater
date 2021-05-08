using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Contract.Model
{
    public class Entity : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Display(Name = "Идентификатор")]
        public Guid Id { get; set; }
    }

    public interface IEntity
    {
        Guid Id { get; set; }
    }

    public abstract class Filter<T> : IFilter<T> where T : Entity
    {
        public Filter(int size, int page, string sort)
        {
            Size = size;
            Page = page;
            Sort = sort;
        }
        public int Size { get; }
        public int Page { get; }
        public string Sort { get; }
    }

    public interface IFilter<T> where T : Entity
    {
        int Page { get; }
        int Size { get; }
        string Sort { get; }
    }

    public class PagedResult<T>
    {
        public PagedResult(IEnumerable<T> data, int allCount)
        {
            Data = data;
            AllCount = allCount;
        }
        public IEnumerable<T> Data { get; }
        public int AllCount { get; }
    }

    public class ClientIdentity : IIdentity
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class UserIdentity : IIdentity
    {
        [Display(Name = "Логин")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }

    public interface IIdentity
    {
        string Login { get; set; }
        string Password { get; set; }
    }
}
