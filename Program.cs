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
        public static void testReentrantReader(ReadWriteLock readWriteLock)
        {
            readWriteLock.ReadLock();
            readWriteLock.ReadLock();
            readWriteLock.ReadLock();
            readWriteLock.ReadLock();
            Console.WriteLine("Test Reentrant Reader");
            readWriteLock.ReadUnlock();
            readWriteLock.ReadUnlock();
            readWriteLock.ReadUnlock();
            readWriteLock.ReadUnlock();
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
            Mutex mutex = new Mutex();
            
            
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
            
            
            /*
            for (int i = 0; i < 10; i++)
            {
                Thread thread = new Thread(() => testWriter(readWriteLock));
                thread.Start();
            }
            for (int i = 0; i < 20; i++)
            {
                Thread thread = new Thread(() => testReader(readWriteLock));
                thread.Start();
            }
            */
            
            /*
            for (int i = 0; i < 30; i++)
            {
                readWriteLock.Print();
            }
            */

        }
    }
}
