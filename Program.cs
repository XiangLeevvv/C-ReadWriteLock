using System;
using System.Threading;

namespace ReentrantReadWriteLock
{
    class Program
    {
        static int num = 0;
        public static void testLock(ReadWriteLock readWriteLock)
        {
            for (int i = 0; i < 100000; i++)
            {
                readWriteLock.WriteLock();
                num++;
                readWriteLock.WriteUnlock();
            }
        }
        public static void standard(Mutex mutex)
        {
            for (int i = 0; i < 100000; i++)
            {
                mutex.WaitOne();
                num++;
                mutex.ReleaseMutex();
            }
        }
        public static void testReentrantWriter(ReadWriteLock readWriteLock)
        {
            readWriteLock.WriteLock();
            readWriteLock.WriteLock();
            readWriteLock.WriteLock();
            readWriteLock.WriteLock();
            Console.WriteLine("Test Reentrant Writer");
            readWriteLock.WriteUnlock();
            readWriteLock.WriteUnlock();
            readWriteLock.WriteUnlock();
            readWriteLock.WriteUnlock();
        }
        public static void testWriter(ReadWriteLock readWriteLock)
        {
            readWriteLock.WriteLock();
            Thread.Sleep(500);
            readWriteLock.WriteUnlock();
        }
        public static void testReader(ReadWriteLock readWriteLock)
        {
            readWriteLock.ReadLock();
            Thread.Sleep(250);
            readWriteLock.ReadUnlock();
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ReadWriteLock readWriteLock = new ReadWriteLock();

            testReentrantWriter(readWriteLock);
            Console.WriteLine();
            Console.WriteLine("测试多读者、写者并发:  (每次在有线程退出时打印当前队列中的运行、等待的线程)");
            
            for (int i = 0; i < 30; i++)
            {
                Thread thread;
                if (i % 3 == 0)
                {
                    thread = new Thread(() => testWriter(readWriteLock));
                    //Console.WriteLine("创建了一个读线程");
                }
                else
                {
                    thread = new Thread(() => testReader(readWriteLock));
                    //Console.WriteLine("创建了一个写线程");
                }
                thread.Start();
            }
        }
    }
}
