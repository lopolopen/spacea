using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SpaceA.Model.Dto
{
    public interface IMask
    {
        string Mask { get; set; }
        bool Allow(string key);
    }

    public abstract class WithMask<TDto> : IMask
    {
        public string Mask { get; set; }
        private string[] _keys;
        private string[] Keys
        {
            get
            {
                if (_keys == null && Mask != null)
                {
                    _keys = Mask.ToLower().Split(',').Select(k => k.Trim()).ToArray();
                }
                return _keys;
            }
        }

        public bool Allow(string key)
        {
            if (string.IsNullOrEmpty(Mask)) return false;
            if ("*" == Mask) return true;
            return Keys.Any(k => k.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool Allow<TProperty>(Expression<Func<TDto, TProperty>> propertyExpression)
        {
            if (string.IsNullOrEmpty(Mask)) return true;
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null) return false;
            string key = memberExpression.Member.Name;
            return Allow(key);
        }
    }
}