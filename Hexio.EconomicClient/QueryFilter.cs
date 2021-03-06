using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Hexio.EconomicClient.ReadModels;

namespace Hexio.EconomicClient
{
    public enum QueryOperator
    {
        Eq,
        Like,
        In,
    }
    
    public class QueryFilter<T> where T : IReadModel, new()
    {
        public IList<QueryFilterValue> Filters { get; set; } = new List<QueryFilterValue>();

        public QueryFilter<T> Where(Expression<Func<T, object>> expression, QueryOperator queryOperator, string value)
        {
            var type = new T();
            
            MemberExpression body = expression.Body as MemberExpression;

            if (body == null) {
                UnaryExpression ubody = (UnaryExpression)expression.Body;
                body = ubody.Operand as MemberExpression;
            }

            var parameterName = body.Member.Name;

            if (!type.FieldsToFilter.Contains(parameterName, StringComparer.InvariantCultureIgnoreCase))
            {
                throw new Exception($"Filtering for parameter {parameterName} is not allowed, see the EConomic Api documentation for further explanation");
            }
            
            var filterValue = new QueryFilterValue
            {
                Parameter = parameterName,
                CompareOperator = queryOperator.ToString().ToLower(),
                Value = value
            };
            
            Filters.Add(filterValue);

            return this;
        }

        public override string ToString()
        {
            var filter = string.Join("$or:", Filters.Select(x => x.ToString()));
            
            if (Filters.Count == 0)
            {
                return "";
            }

            filter = WebUtility.UrlEncode(filter);
            
            return $"filter={filter}";
        }
    }

    public class QueryFilterValue
    {
        public string Parameter { get; set; }
        public string CompareOperator { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return $"{Parameter}${CompareOperator}:{Value}";
        }
    }
}