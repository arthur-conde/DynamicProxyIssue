using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using LinqKit;

namespace DynamicProxyIssue
{
    class Program
    {
        public static ProxyGenerator ProxyGenerator = new ProxyGenerator();

        static void Main(string[] args)
        {
            var source = new Entity1();
            source.Details.Add(new Entity2 {Value = "A"});
            source.Details.Add(new Entity2 {Value = "B"});
            source.Details.Add(new Entity2 {Value = "C"});

            var proxy = ProxyGenerator.CreateInterfaceProxyWithTarget<IEntity1<Entity2>>(source, new Interceptor());

            var details = proxy.Details;

            using (var enumerator = details.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var detail = enumerator.Current;
                    Console.WriteLine($"Detail: Value = {detail.Value} - Type: {detail.GetType()}");
                }
            }

            foreach (var detail in details)
                Console.WriteLine($"Detail: Value = {detail.Value} - Type: {detail.GetType()}");

            Console.WriteLine(string.Empty);

            proxy.Details.ToList().ForEach(detail => Console.WriteLine($"Detail: Value = {detail.Value} - Type: {detail.GetType()}"));

            Console.ReadLine();
        }
    }
}
