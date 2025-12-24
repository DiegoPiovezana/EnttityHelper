using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EH.Connection
{
    internal class Memory
    {
        public class InMemoryQueryProvider<T> : IQueryProvider
        {
            private List<T> _data;

            public InMemoryQueryProvider(List<T> data)
            {
                _data = data;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new InMemoryQueryable<T>(this, expression);
            }

            public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
            {
                return new InMemoryQueryable<TResult>(this, expression);
            }

            public object Execute(Expression expression)
            {
                return _data.AsQueryable().Provider.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _data.AsQueryable().Provider.Execute<TResult>(expression);
            }
        }

        public class InMemoryQueryable<T> : IQueryable<T>
        {
            public Type ElementType => typeof(T);
            public Expression Expression { get; private set; }
            public IQueryProvider Provider { get; private set; }

            public InMemoryQueryable(IQueryProvider provider, Expression expression)
            {
                Provider = provider;
                Expression = expression;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }



    }
}
