using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ChatApp.Tests
{
    /// <summary>Bir test başarısız olduğunda fırlatılır.</summary>
    public class AssertException : Exception
    {
        public AssertException(string message) : base(message) { }
    }

    /// <summary>xUnit'in Assert sınıfının çok küçük bir benzeri.</summary>
    public static class Assert
    {
        public static void True(bool condition, string message)
        {
            if (!condition) throw new AssertException(message);
        }

        public static void Equal<T>(T expected, T actual, string what)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new AssertException(what + ": beklenen <" + expected + ">, gelen <" + actual + ">");
        }

        public static TEx Throws<TEx>(Action action, string what) where TEx : Exception
        {
            try { action(); }
            catch (TEx ex) { return ex; }
            catch (Exception ex) { throw new AssertException(what + ": " + typeof(TEx).Name + " bekleniyordu, " + ex.GetType().Name + " geldi"); }
            throw new AssertException(what + ": " + typeof(TEx).Name + " bekleniyordu, hata fırlatılmadı");
        }

        public static async Task<TEx> ThrowsAsync<TEx>(Func<Task> action, string what) where TEx : Exception
        {
            try { await action(); }
            catch (TEx ex) { return ex; }
            catch (Exception ex) { throw new AssertException(what + ": " + typeof(TEx).Name + " bekleniyordu, " + ex.GetType().Name + " geldi"); }
            throw new AssertException(what + ": " + typeof(TEx).Name + " bekleniyordu, hata fırlatılmadı");
        }
    }

    /// <summary>
    /// Windows Forms'un UI iş parçacığını taklit eder: tüm await devamları tek bir
    /// iş parçacığında, sırayla çalışır. ChatServerCore kilitsiz tasarımını bu varsayıma
    /// dayandırdığı için testler de aynı ortamda çalıştırılır.
    /// </summary>
    public sealed class SingleThreadContext : SynchronizationContext
    {
        private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue =
            new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        public override void Post(SendOrPostCallback d, object state)
        {
            try { _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state)); }
            catch (InvalidOperationException) { }   // test bitti; geç gelen devamları yok say
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException();
        }

        public static void Run(Func<Task> func)
        {
            SynchronizationContext previous = Current;
            var ctx = new SingleThreadContext();
            SetSynchronizationContext(ctx);
            try
            {
                Task task = func();
                task.ContinueWith(_ => ctx._queue.CompleteAdding(), TaskScheduler.Default);
                foreach (KeyValuePair<SendOrPostCallback, object> work in ctx._queue.GetConsumingEnumerable())
                    work.Key(work.Value);
                task.GetAwaiter().GetResult();
            }
            finally
            {
                SetSynchronizationContext(previous);
            }
        }
    }

    public static class TestRunner
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        /// <summary>Testleri çalıştırır; başarısız test sayısını döner.</summary>
        public static int Run(IEnumerable<KeyValuePair<string, Func<Task>>> tests)
        {
            int passed = 0, failed = 0;
            var total = Stopwatch.StartNew();

            foreach (KeyValuePair<string, Func<Task>> test in tests)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    SingleThreadContext.Run(async () =>
                    {
                        Task t = test.Value();
                        if (await Task.WhenAny(t, Task.Delay(Timeout)) != t)
                            throw new AssertException("zaman aşımı (" + Timeout.TotalSeconds + " sn)");
                        await t;
                    });
                    passed++;
                    Console.WriteLine("  OK    " + test.Key + "  (" + sw.ElapsedMilliseconds + " ms)");
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine("  FAIL  " + test.Key);
                    Console.WriteLine("        " + (ex is AssertException ? ex.Message : ex.ToString()));
                }
            }

            Console.WriteLine();
            Console.WriteLine(passed + " passed, " + failed + " failed (" + total.ElapsedMilliseconds + " ms)");
            return failed;
        }
    }
}
