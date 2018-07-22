using System;
using System.Collections.Generic;
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

            foreach(var detail in proxy.Details)
                Console.WriteLine($"Detail: Value = {detail.Value} - Type: {detail.GetType()}");

            Console.WriteLine();

            proxy.Details.ToList().ForEach(detail => Console.WriteLine($"Detail: Value = {detail.Value} - Type: {detail.GetType()}"));

            Console.ReadLine();
        }
    }
}
