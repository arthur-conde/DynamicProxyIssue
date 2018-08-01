using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace DynamicProxyIssue
{
    public class Interceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            string methodName = invocation.Method.Name;
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("Interceptor");
            Debug.WriteLine($"Invocation.Method: {invocation.Method.Name}");
            Debug.WriteLine($"Invocation.InvocationTarget: {invocation.InvocationTarget.GetType().Name}");
            Debug.WriteLine($"Invocation.TargetType: {invocation.TargetType.Name}");
            Debug.WriteLine($"Invocation.GenericArguments: {string.Join(", ", invocation.GenericArguments?.Select(a => a.Name) ?? new[] { "" })}");
            Debug.WriteLine($"Invocation.Proxy: {invocation.Proxy.GetType().Name}");
            Debug.WriteLine($"Invocation.MethodInvocationTarget: {invocation.MethodInvocationTarget.Name}");
            Debug.WriteLine(string.Empty);

            if (invocation.Method.Name.StartsWith("set_") || invocation.Method.Name.StartsWith("get_"))
            {
                var propertyName = invocation.Method.Name.Substring(4);
                var targetType = invocation.InvocationTarget.GetType();
                var property = targetType.GetProperty(propertyName);

                if (property?.PropertyType == invocation.Method.ReturnType)
                {
                    // Correct Property, probably
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var rawValue = property.GetValue(invocation.InvocationTarget);

                    Debug.WriteLine($"Initial Value: {rawValue} ");

                    if (rawValue == null)
                    {
                        invocation.ReturnValue = null;
                        return;
                    }

                    if (propertyType.IsGenericType &&
                        propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        invocation.ReturnValue = ProxyList(rawValue);
                        return;
                    }
                }
            }

            
            invocation.Proceed();
        }

        public static object ProxyList(object source)
        {
            var generator = Program.ProxyGenerator;

            var sourceType = source.GetType();

            if (!sourceType.IsGenericType)
                throw new ArgumentException();
            if (sourceType.GetGenericTypeDefinition() != typeof(ICollection<>) && sourceType.GetInterface("ICollection`1") == null)
                throw new ArgumentException();

            var proxyIface = sourceType.GetInterface("ICollection`1");

            var proxy = generator.CreateInterfaceProxyWithTarget(proxyIface, sourceType.GetInterfaces(), source, new CollectionInterceptor());
            return proxy;
        }
    }

    public class CollectionInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("CollectionInterceptor");
            Debug.WriteLine($"Invocation.Method: {invocation.Method.Name}");
            Debug.WriteLine($"Invocation.InvocationTarget: {invocation.InvocationTarget.GetType().Name}");
            Debug.WriteLine($"Invocation.TargetType: {invocation.TargetType.Name}");
            Debug.WriteLine($"Invocation.GenericArguments: {string.Join(", ", invocation.GenericArguments?.Select(a => a.Name) ?? new[] { "" })}");
            Debug.WriteLine($"Invocation.Proxy: {invocation.Proxy.GetType().Name}");
            Debug.WriteLine($"Invocation.MethodInvocationTarget: {invocation.MethodInvocationTarget.Name}");
            Debug.WriteLine(string.Empty);

            var generator = Program.ProxyGenerator;

            var methodName = invocation.Method.Name;
            if (invocation.Method.ReturnType.IsGenericType &&
                invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerator<>))
            {
                var source = invocation.GetConcreteMethodInvocationTarget()
                    .Invoke(invocation.InvocationTarget, invocation.Arguments);
                var proxy = generator.CreateInterfaceProxyWithTarget(typeof(IEnumerator<>).MakeGenericType(invocation.Method.ReturnType.GetGenericArguments().First()), Activator.CreateInstance(typeof(ProxyingEnumerator<>).MakeGenericType(invocation.Method.ReturnType.GetGenericArguments().First()), source), new IEnumeratorInterceptor());
                invocation.ReturnValue = proxy;
                return;
            }

            invocation.Proceed();
        }
    }

    public class IEnumeratorInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Debug.WriteLine(string.Empty);
            Debug.WriteLine("IEnumeratorInterceptor");
            Debug.WriteLine($"Invocation.Method: {invocation.Method.Name}");
            Debug.WriteLine($"Invocation.InvocationTarget: {invocation.InvocationTarget.GetType().Name}");
            Debug.WriteLine($"Invocation.TargetType: {invocation.TargetType.Name}");
            Debug.WriteLine($"Invocation.GenericArguments: {string.Join(", ", invocation.GenericArguments?.Select(a => a.Name) ?? new[] { "" })}");
            Debug.WriteLine($"Invocation.Proxy: {invocation.Proxy.GetType().Name}");
            Debug.WriteLine($"Invocation.MethodInvocationTarget: {invocation.MethodInvocationTarget.Name}");
            Debug.WriteLine(string.Empty);

            var generator = Program.ProxyGenerator;

            invocation.Proceed();
        }
    }

    public class ProxyingEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator<T> Source { get; }

        public T Current
        {
            get
            {
                T current = Source.Current;
                if (current != null)
                {
                    current = (T) Program.ProxyGenerator.CreateClassProxyWithTarget(typeof(T), typeof(T).GetInterfaces(), current, new Interceptor());
                }

                return current;
            }
        }


        object IEnumerator.Current => Current;

        public ProxyingEnumerator(IEnumerator<T> source)
        {
            Source = source;
        }

        public void Dispose()
        {
            Source.Dispose();
        }

        public bool MoveNext()
        {
            return Source.MoveNext();
        }

        public void Reset()
        {
            Source.Reset();
        }
    }
}
