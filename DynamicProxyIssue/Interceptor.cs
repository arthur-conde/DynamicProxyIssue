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
                    }
                }
            }

            Console.WriteLine($"Intercept: {invocation.Method.Name}");
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

            var proxy = generator.CreateInterfaceProxyWithTarget(proxyIface, new[] { typeof(IList) }, source, new Interceptor());
            return proxy;
        }
    }
}
