using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace GoldRush.Core
{
    public static class DynamicExpressionBuilder
    {
        public static Expression<Func<T, bool>> BuildFilterExpression<T>(string jsonString)
        {
            return WebHelper.JSONToFilterExpressionTree<T>(jsonString);
        }

        public static IQueryable<dynamic> BuildFilterExpression<T>(string jsonString, dynamic query)
        {
            return WebHelper.JSONToFilterExpressionTree<T>(jsonString, query);
        }
    }

    public class FilterGroup
    {
        public FilterGroupOperator Operator { get; set; }
        public List<FilterRule> Rules { get; set; }
        public List<FilterGroup> Groups { get; set; }

        public FilterGroup()
        {
            this.Rules = new List<FilterRule>();
            this.Groups = new List<FilterGroup>();
        }

        public Expression<Func<T, bool>> ToExpressionTree<T>()
        {
            Type t = typeof(T);
            ParameterExpression param = Expression.Parameter(t, "p");
            Expression body = GetExpressionFromSubgroup(this, t, param);

            if (body == null)
                return null;
            return Expression.Lambda<Func<T, bool>>(body, new ParameterExpression[] { param });
        }

        private static Type GetQueryType<T>(IQueryable<T> query) => typeof(T);
        private static IQueryable<T> ApplyPredicate<T>(IQueryable<T> query,
            Expression body, params ParameterExpression[] parameters)
            => query.Where(Expression.Lambda<Func<T, bool>>(body, parameters));

        public IQueryable<dynamic> ToExpressionTree<T>(dynamic query)
        {
            Type t = GetQueryType(query);
            var parameters = Expression.Parameter(t);
            Expression body = GetExpressionFromSubgroup(this, t, parameters);
            return ApplyPredicate(query, body, parameters);
            // if (body == null)
            //     return null;

            // return Expression.Lambda<Func<T, bool>>(body, parameters);
        }

        public Expression GetExpressionFromSubgroup(FilterGroup subgroup, Type parameterType, ParameterExpression param)
        {
            Expression body = null;

            // Get expressions from subgroup
            foreach (FilterGroup g in subgroup.Groups)
            {
                // make a recurrent call to make sure that we get all the expressions
                Expression subgroupExpression = GetExpressionFromSubgroup(g, parameterType, param);

                if (subgroupExpression == null)
                    continue; // Ignore groups that don't have rules

                if (body == null)
                    body = subgroupExpression;
                else
                {
                    if (subgroup.Operator == FilterGroupOperator.And)
                        body = Expression.And(body, subgroupExpression);
                    else
                        body = Expression.Or(body, subgroupExpression);
                }
            }

            // Get expressions from rules
            foreach (FilterRule r in subgroup.Rules)
            {
                Expression ruleExpression = r.ToExpression(parameterType, param);

                if (ruleExpression == null)
                    continue; // Ignore broken rules

                if (body == null)
                    body = ruleExpression;
                else
                {
                    if (subgroup.Operator == FilterGroupOperator.And)
                        body = Expression.And(body, ruleExpression);
                    else
                        body = Expression.Or(body, ruleExpression);
                }
            }

            return body;
        }
    }

    public enum FilterGroupOperator
    {
        Or = 0,
        And = 1
    }

    public class FilterRule
    {
        public string Field { get; set; }
        public FilterRuleOperator Operator { get; set; }
        public string Data { get; set; }

        /// <summary>
        /// Converts a rule into an expression. The rule doesn't have any
        /// notion about the input parameter, so we must set these
        /// </summary>
        /// <param name="parameterType"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public Expression ToExpression(Type parameterType, ParameterExpression param)
        {
            // Get the property that will be evaluated
            PropertyInfo pi = null;
            MemberExpression member = null;

            if (this.Field.Contains(".")) // check for subproperties
            {
                foreach (string f in this.Field.Split(".".ToCharArray()))
                {
                    if (pi == null)
                    {
                        pi = parameterType.GetProperty(f);
                        member = Expression.PropertyOrField(param, f);
                    }
                    else
                    {
                        pi = pi.PropertyType.GetProperty(f);
                        member = Expression.PropertyOrField(member, f);
                        //Expression.Parameter(parameterType).Type.GetProperty("Position").PropertyType.GetProperty("name")
                    }
                }
            }
            else
            {
                pi = parameterType.GetProperty(this.Field);
                member = Expression.PropertyOrField(param, this.Field);
            }

            ConstantExpression constant = this.Operator == FilterRuleOperator.IsNull || this.Operator == FilterRuleOperator.NotNull
                    ? Expression.Constant(null, pi.PropertyType)
                    : Expression.Constant(this.CastDataAs(pi.PropertyType), pi.PropertyType);

            switch (this.Operator)
            {
                case FilterRuleOperator.IsNull: // it's the same for null
                case FilterRuleOperator.Equals:
                    return Expression.Equal(member, constant);
                case FilterRuleOperator.NotNull: // it's the same for not null
                case FilterRuleOperator.NotEqual:
                    return Expression.Not(Expression.Equal(member, constant));
                case FilterRuleOperator.LessThan:
                    return Expression.LessThan(member, constant);
                case FilterRuleOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, constant);
                case FilterRuleOperator.GreaterThan:
                    return Expression.GreaterThan(member, constant);
                case FilterRuleOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, constant);
                case FilterRuleOperator.StartsWith: // available only for string fields
                    if (pi.PropertyType != typeof(string))
                        return Expression.Equal(member, constant);
                    return Expression.Call(member, typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }), new Expression[] { constant });
                case FilterRuleOperator.EndsWith: // available only for string fields
                    if (pi.PropertyType != typeof(string))
                        return Expression.Equal(member, constant);
                    return Expression.Call(member, typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }), new Expression[] { constant });
                case FilterRuleOperator.Contains: // available only for string fields
                    if (pi.PropertyType != typeof(string))
                        return Expression.Equal(member, constant);
                    return Expression.Call(member, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), new Expression[] { constant });

            }
            return null;
        }

        /// <summary>
        /// This method is used to cast the Data field to a specifical item type.
        /// As data will only hold numbers, strings or date, we don't need a big
        /// method to reallize this cast
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected object CastDataAs(Type t)
        {
            // ignore invalid casts
            if (t == typeof(string))
                return this.Data;

            if (t == typeof(int) || t == typeof(int?))
                return int.Parse(this.Data);

            if (t == typeof(float) || t == typeof(float?))
                return float.Parse(this.Data);

            if (t == typeof(decimal) || t == typeof(decimal?))
                return decimal.Parse(this.Data);

            if (t == typeof(DateTime) || t == typeof(DateTime?))
                return DateTime.Parse(this.Data);

            if (t == typeof(bool) || t == typeof(bool?))
            {
                bool.TryParse(this.Data, out bool result);
                return result;
            }

            return this.Data;
        }
    }

    public enum FilterRuleOperator
    {
        IsNull,
        NotNull,
        Equals,
        NotEqual,
        Contains,
        StartsWith,
        EndsWith,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual
    }

    public static class WebHelper
    {
        public static Expression<Func<T, bool>> JSONToFilterExpressionTree<T>(string jsonString)
        {
            try
            {
                var djson = DeserializeGroupFromJSON(jsonString);
                var expr = djson.ToExpressionTree<T>();
                return expr;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IQueryable<dynamic> JSONToFilterExpressionTree<T>(string jsonString, dynamic query)
        {
            if (jsonString == null) return null;

            try
            {
                var djson = DeserializeGroupFromJSON(jsonString);
                var expr = djson.ToExpressionTree<T>(query);
                return expr;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class Sorter
        {
            public string Field { get; set; }
            public string Order { get; set; }

        }

        public static object GetDeepPropertyValue(object instance, string path)
        {
            var pp = path.Split('.');
            Type t = instance.GetType();
            foreach (var prop in pp)
            {
                PropertyInfo propInfo = t.GetProperty(prop);
                if (propInfo != null)
                {
                    instance = propInfo.GetValue(instance, null);
                    t = propInfo.PropertyType;
                }
                else throw new ArgumentException("Properties path is not correct");
            }
            return instance;
        }

        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string sortExpression)
        {
            if (source == null)
                return source;

            if (string.IsNullOrEmpty(sortExpression))
                return source;

            var parts = sortExpression.Split(' ');
            var isDescending = false;
            var propertyName = "";
            var tType = typeof(T);
            if (parts.Length > 0 && parts[0] != "")
            {
                propertyName = parts[0];

                if (parts.Length > 1)
                {
                    isDescending = parts[1].ToLower().Contains("esc");
                }

                PropertyInfo prop = tType.GetProperty(propertyName);

                if(propertyName.Contains("."))
                {
                    propertyName = propertyName.Split('.').First();
                    prop = tType.GetProperty(propertyName);
                    //prop = tType.GetProperty(parts[0].Split('.').Last());
                }
                

                if (prop == null)
                    return source;

                var funcType = typeof(Func<,>)
                    .MakeGenericType(tType, prop.PropertyType);

                var lambdaBuilder = typeof(Expression)
                    .GetMethods()
                    .First(x => x.Name == "Lambda" && x.ContainsGenericParameters && x.GetParameters().Length == 2)
                    .MakeGenericMethod(funcType);

                var parameter = Expression.Parameter(tType);
                var propExpress = Expression.Property(parameter, prop);

                var sortLambda = lambdaBuilder
                    .Invoke(null, new object[] { propExpress, new ParameterExpression[] { parameter } });

                var sorter = typeof(Queryable)
                    .GetMethods()
                    .FirstOrDefault(x => x.Name == (isDescending ? "OrderByDescending" : "OrderBy") && x.GetParameters().Length == 2)
                    .MakeGenericMethod(new[] { tType, prop.PropertyType });

                return (IQueryable<T>)sorter
                    .Invoke(null, new object[] { source, sortLambda });
            }

            return source;
        }

        /// <summary>
        /// Serializes a group entity
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static string SerializeGroupToJSON(FilterGroup g)
        {

            string json = "{ \"groupOp\": \"" + g.Operator.ToString().ToLower() + "\"";

            if (g.Groups != null && g.Groups.Count > 0)
            {
                json += ",\"groups\" : [";

                for (int i = 0; i < g.Groups.Count; i++)
                {
                    if (i > 0)
                        json += ",";

                    json += SerializeGroupToJSON(g.Groups[i]);
                }

                json += "]";
            }

            if (g.Rules != null && g.Rules.Count > 0)
            {
                json += ",\"rules\" : [";

                for (int i = 0; i < g.Rules.Count; i++)
                {
                    if (i > 0)
                        json += ",";

                    FilterRule r = g.Rules[i];

                    string opString = "eq";

                    switch (r.Operator)
                    {
                        case FilterRuleOperator.Contains: opString = "cn"; break;
                        case FilterRuleOperator.EndsWith: opString = "ew"; break;
                        case FilterRuleOperator.Equals: opString = "eq"; break;
                        case FilterRuleOperator.GreaterThanOrEqual: opString = "ge"; break;
                        case FilterRuleOperator.GreaterThan: opString = "gt"; break;
                        case FilterRuleOperator.IsNull: opString = "nu"; break;
                        case FilterRuleOperator.LessThanOrEqual: opString = "le"; break;
                        case FilterRuleOperator.LessThan: opString = "lt"; break;
                        case FilterRuleOperator.NotEqual: opString = "ne"; break;
                        case FilterRuleOperator.NotNull: opString = "nn"; break;
                        case FilterRuleOperator.StartsWith: opString = "bw"; break;
                    }

                    json += "{ \"field\": \"" + r.Field + "\", \"op\": \"" + opString + "\", \"data\": \"" + r.Data + "\" }";
                }

                json += "]";
            }

            return json + "}";
        }

        /// <summary>
        /// Deserializes a JSON into a group
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static FilterGroup DeserializeGroupFromJSON(string jsonString)
        {
            JObject container = JObject.Parse(jsonString);

            FilterGroup g = DeserializeGroupFromJSON(container);

            return g;
        }

        public static FilterGroup DeserializeGroupFromJSON(JToken value)
        {
            FilterGroup g = new FilterGroup
            {
                Operator = ((string)value["groupOp"]).ToLower() == "and"
                ? FilterGroupOperator.And : FilterGroupOperator.Or
            };

            if (value["groups"] != null)
                foreach (JToken token in (JArray)value["groups"])
                    g.Groups.Add(DeserializeGroupFromJSON(token));

            if (value["rules"] != null)
                foreach (JToken token in (JArray)value["rules"])
                    g.Rules.Add(DeserializeRuleFromJSON(token));

            return g;
        }

        public static FilterRule DeserializeRuleFromJSON(JToken value)
        {
            FilterRule r = new FilterRule
            {
                Field = (string)value["field"],
                Data = (string)value["data"]
            };
            switch ((string)value["op"])
            {
                case "cn": r.Operator = FilterRuleOperator.Contains; break;
                case "ew": r.Operator = FilterRuleOperator.EndsWith; break;
                case "eq": r.Operator = FilterRuleOperator.Equals; break;
                case "ge": r.Operator = FilterRuleOperator.GreaterThanOrEqual; break;
                case "gt": r.Operator = FilterRuleOperator.GreaterThan; break;
                case "nu": r.Operator = FilterRuleOperator.IsNull; break;
                case "le": r.Operator = FilterRuleOperator.LessThanOrEqual; break;
                case "lt": r.Operator = FilterRuleOperator.LessThan; break;
                case "ne": r.Operator = FilterRuleOperator.NotEqual; break;
                case "nn": r.Operator = FilterRuleOperator.NotNull; break;
                case "bw": r.Operator = FilterRuleOperator.StartsWith; break;
            }

            return r;
        }
    }
}