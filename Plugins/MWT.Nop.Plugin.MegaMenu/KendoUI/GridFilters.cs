using System;
using System.Collections.Generic;
using System.Text;

namespace MWT.Nop.Plugin.MegaMenu.KendoUI
{
    public class GridFilters
    {
        protected static readonly IDictionary<string, string> operators = (IDictionary<string, string>)new Dictionary<string, string>()
    {
      {
        "eq",
        "="
      },
      {
        "neq",
        "!="
      },
      {
        "lt",
        "<"
      },
      {
        "lte",
        "<="
      },
      {
        "gt",
        ">"
      },
      {
        "gte",
        ">="
      },
      {
        "startswith",
        "StartsWith"
      },
      {
        "endswith",
        "EndsWith"
      },
      {
        "contains",
        "Contains"
      },
      {
        "doesnotcontain",
        "DoesNotContain"
      }
    };

        public IList<GridFilter> Filters { get; set; }

        public string Logic { get; set; }

        public string ToExpression(IList<GridFilter> filters, out IList<object> values)
        {
            values = (IList<object>)new List<object>();
            return filters != null && filters.Count > 0 ? this.BuildExpression(filters, values) : string.Empty;
        }

        protected virtual string BuildExpression(
          IList<GridFilter> filters,
          IList<object> values,
          int index = 0)
        {
            StringBuilder stringBuilder1 = new StringBuilder();
            stringBuilder1.Append("(");
            int num1 = index;
            for (int index1 = index; index1 < filters.Count + index; ++index1)
            {
                GridFilter filter = filters[index1 - index];
                string str = GridFilters.operators[filter.Operator];
                if (filter.Field.StartsWith("Id"))
                {
                    int result;
                    if (int.TryParse(filter.Value, out result))
                    {
                        stringBuilder1.AppendFormat("{0} {1} @{2}", (object)filter.Field, (object)str, (object)num1++);
                        values.Add((object)result);
                    }
                }
                else if (str == "=")
                {
                    stringBuilder1.AppendFormat("{0}.ToLower().Equals(@{1}.ToLower())", (object)filter.Field, (object)num1++);
                    values.Add((object)filter.Value);
                }
                else if (str == "!=")
                {
                    stringBuilder1.AppendFormat("!({0}.Equals(@{1}, @{2}))", filter.Field, (int)num1++, (int)num1++);
                    values.Add(filter.Value);
                    values.Add((StringComparison)StringComparison.InvariantCultureIgnoreCase);

                }
                else if (str == "Contains")
                {
                    stringBuilder1.AppendFormat("{0}.Contains(@{1})", (object)filter.Field, (object)num1++);
                    values.Add((object)filter.Value);
                }
                else if (str == "DoesNotContain")
                {
                    stringBuilder1.AppendFormat("!({0}.Contains(@{1}))", (object)filter.Field, (object)num1++);
                    values.Add((object)filter.Value);
                }
                else if (str == "StartsWith" || str == "EndsWith")
                {
                    stringBuilder1.AppendFormat("{0}.{1}(@{2})", (object)filter.Field, (object)str, (object)num1++);
                    values.Add((object)filter.Value);
                }
                else
                {
                    int result;
                    if (int.TryParse(filter.Value, out result))
                    {
                        stringBuilder1.AppendFormat("{0} {1} @{2}", (object)filter.Field, (object)str, (object)num1++);
                        values.Add((object)result);
                    }
                    else
                    {
                        stringBuilder1.AppendFormat("{0} {1} @{2}", (object)filter.Field, (object)str, (object)num1++);
                        values.Add((object)filter.Value);
                    }
                }
                if (index1 < filters.Count - 1)
                    stringBuilder1.AppendFormat(" {0} ", (object)this.Logic);
            }
            stringBuilder1.Append(")");
            return stringBuilder1.ToString();
        }
    }
}
