internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var tasks = new List<Task>();

        tasks.Add(Task.Run(Thread1));
        tasks.Add(Task.Run(Thread2));
        tasks.Add(Task.Run(Thread3));

        foreach (var task in tasks)
        {
            task.GetAwaiter().GetResult();
        }
    }

    private static async Task Thread1()
    {
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine("Thread 1 :: {0}", i);
            await Task.Delay(i * 1000);
        }
    }

    private static async Task Thread2()
    {
        for (int i = 0; i < 20; i++)
        {
            Console.WriteLine("Thread 2 :: {0}", i);
            await Task.Delay(i * 2000);
        }
    }

    private static async Task Thread3()
    {
        for (int i = 0; i < 30; i++)
        {
            Console.WriteLine("Thread 3 :: {0}", i);
            await Task.Delay(i * 3000);
        }
    }
}