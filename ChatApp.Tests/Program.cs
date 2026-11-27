using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChatApp.Tests
{
    /// <summary>
    /// Test sınıflarındaki her public static metodu bir test olarak çalıştırır.
    /// Kullanım:  dotnet run --project ChatApp.Tests            (hepsi)
    ///            dotnet run --project ChatApp.Tests -- Server   (adında "Server" geçenler)
    /// Başarısız test varsa çıkış kodu 1 olur (CI için).
    /// </summary>
    public static class Program
    {
        private static readonly Type[] TestClasses = { typeof(ProtocolTests), typeof(ConnectionTests), typeof(ServerTests) };

        public static int Main(string[] args)
        {
            string filter = args.Length > 0 ? args[0] : null;

            var tests = new List<KeyValuePair<string, Func<Task>>>();
            foreach (Type type in TestClasses)
            {
                foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                                             .Where(m => m.GetParameters().Length == 0)
                                             .Where(m => m.ReturnType == typeof(void) || m.ReturnType == typeof(Task)))
                {
                    string name = type.Name + "." + m.Name;
                    if (filter != null && name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    MethodInfo method = m;
                    tests.Add(new KeyValuePair<string, Func<Task>>(name, () =>
                    {
                        try
                        {
                            object result = method.Invoke(null, null);
                            return result as Task ?? Task.CompletedTask;
                        }
                        catch (TargetInvocationException ex)
                        {
                            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                            throw;
                        }
                    }));
                }
            }

            Console.WriteLine(tests.Count + " test çalıştırılıyor...");
            return TestRunner.Run(tests) == 0 ? 0 : 1;
        }
    }
}
