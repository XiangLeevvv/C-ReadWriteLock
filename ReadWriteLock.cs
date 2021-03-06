﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace ReentrantReadWriteLock
{
    public class ReadWriteLock
    {
        //调度管理队列
        private volatile Queue<Node> Sequence;

        //重入量分为读重入量、写重入量方便区分
        private volatile int writerReentrants;
        private volatile int readerReentrants;

        //持有锁的线程
        private volatile Thread holder;

        //队列尾元素
        private volatile Node rear;

        //输出函数实现原子操作的object
        private static object obj = new object();

        public ReadWriteLock()
        {
            Sequence = new Queue<Node>();
        }

        /*
        ** 获取写锁
        */
        public void WriteLock()
        {
            Node waitingNode = null;
            /*
            ** 队列为空时直接入队列运行
            */
            lock (this)
            {
                if (Sequence.Count == 0)
                {
                    addRunningWriter();
                    return;
                }
            }
            /*
            ** 判断写锁是否重入
            */
            lock (this)
            {
                if (holder == Thread.CurrentThread)
                {
                    writerReentrants += 1;
                    return;
                }
                else
                {
                    //其他线程可能在这段时间执行完成，因此需要再次判断队列节点个数
                    if (Sequence.Count == 0)
                    {
                        addRunningWriter();
                        return;
                    }
                    else
                    {
                        waitingNode = addWaitingWriter();
                    }
                }
            }
            /*
            ** 检测当前节点是否可以执行
            */
            while (true)
            {
                //等待的写者被唤醒的条件:写者位于队列最前端
                if (Sequence.Peek() == waitingNode)
                {
                    writerReentrants = 1;
                    waitingNode.Status = Node.RUNNING;
                    holder = Thread.CurrentThread;
                    break;
                }
            }
        }

        /*
        ** 释放写锁
        */
        public void WriteUnlock()
        {
            lock (this)
            {
                writerReentrants -= 1;
                //当写重入量为0时写线程出队列
                if (writerReentrants == 0)
                {
                    Sequence.Dequeue();
                    Console.WriteLine("写线程退出！");
                    Print();
                }
            }
        }

        /*
        ** 获取读锁
        */
        public void ReadLock()
        {
            Node readNode = null;
            /*
            ** 队列为空时直接入队列运行
            */
            lock (this)
            {
                if (Sequence.Count == 0)
                {
                    addRunningReader();
                    return;
                }
            }
            /*
             ** 判断读是否可并发
             */
            lock (this)
            {
                if (ReaderParallel())
                {
                    addParallelReader();
                    return;
                }
                else
                {
                    if (Sequence.Count == 0)
                    {
                        addRunningReader();
                        return;
                    }
                    else
                    {
                        //队列中有写者加到队尾等待
                        if (IsWriterExist())
                        {
                            readNode = addWaitingReader();
                        }
                        else
                        {
                            addParallelReader();
                            return;
                        }
                    }
                }
            }
            /*
             ** 判断是否可以唤醒读者 
             */
             while (true)
            {
                //读者位于队列队首时的情况
                if (readNode == Sequence.Peek())
                {
                    readNode.Status = Node.RUNNING;
                    readerReentrants = 1;
                    holder = readNode.thread;
                    break;
                }
                else
                {
                    //读者前驱节点为读节点且前驱节点正在执行
                    Node preNode = readNode.pre;
                    if (preNode.Status == Node.RUNNING && !preNode.getType())
                    {
                        readNode.Status = Node.RUNNING;
                        readerReentrants += 1;
                        break;
                    }
                }
            }
        }

        /*
         ** 释放读锁
         */
         public void ReadUnlock()
        {
            lock (this)
            {
                readerReentrants -= 1;
                if (readerReentrants == 0)
                {
                    //删除队列中运行的所有读者
                    while (true)
                    {
                        if (Sequence.Count == 0)
                        {
                            //队列空了
                            break;
                        }
                        else
                        {
                            Node head = Sequence.Peek();
                            if (!head.getType())
                            {
                                Sequence.Dequeue();
                                Console.WriteLine("一个读线程退出！");
                            }
                            else
                            {
                                //遇到第一个写者就说明运行的读者删除完了
                                break;
                            }
                        }
                    }
                    Print();
                }
            }
        }

        /*
         **添加写者
         */
         //添加直接运行的写者
        private void addRunningWriter()
        {
            Node node = new Node(Thread.CurrentThread, 1);
            node.Status = Node.RUNNING;
            holder = Thread.CurrentThread;
            writerReentrants = 1;
            rear = node;
            Sequence.Enqueue(node);
        }

        //添加等待的写者
        private Node addWaitingWriter()
        {
            Node node = new Node(Thread.CurrentThread, 1);
            node.Status = Node.WAITING;
            node.pre = rear;
            rear = node;
            Sequence.Enqueue(node);
            return node;
        }

        /*
         **添加读者
         */
         //添加直接运行的读者
        private void addRunningReader()
        {
            Node node = new Node(Thread.CurrentThread, 0);
            node.Status = Node.RUNNING;
            holder = Thread.CurrentThread;
            readerReentrants = 1;
            rear = node;
            Sequence.Enqueue(node);
        }

        //添加并发运行的读者
        private void addParallelReader()
        {
            Node node = new Node(Thread.CurrentThread, 0);
            node.Status = Node.RUNNING;
            holder = Thread.CurrentThread;
            readerReentrants += 1;
            node.pre = rear;
            rear = node;
            Sequence.Enqueue(node);
        }

        //添加等待的读者
        private Node addWaitingReader()
        {
            
            Node node = new Node(Thread.CurrentThread, 0);
            node.Status = Node.WAITING;
            node.pre = rear;
            rear = node;
            Sequence.Enqueue(node);
            return node;
        }

        /*
         **判断读者是否可以并发执行
         */
        private bool ReaderParallel()
        {
            //队列中没有写者时可以并发,因为只要队列中有写者
            //不论是正在执行还是等待都会阻塞后入的读者
            if (!IsWriterExist())
            {
                return true;
            }
            return false;
        }

        /*
         **判断队列中是否有写者:false—没有;true—有
         */
        private bool IsWriterExist()
        {
            if (Sequence.Count == 0)
            {
                return false;
            }
            foreach (var node in Sequence)
            {
                //有写者
                if (node.getType())
                {
                    return true;
                }
            }
            return false;
        }

        /*
         **打印队列情况
         */
        public void Print()
        {
            lock (obj)
            {
                if (Sequence.Count == 0)
                {
                    Console.WriteLine("队列为空!");
                }
                else
                {
                    Console.WriteLine("此时队列中的线程:");
                    Node[] list = Sequence.ToArray();
                    int waitNode = 0;
                    string sep = new string('-', 76);
                    for (int i = 0; i < list.Length; i++)
                    {
                        Node node = list[i];
                        if (node.Status == Node.RUNNING)
                        {
                            if (node.getType())
                            {
                                Console.WriteLine("Writer【持有线程ID = {0},运行状态STATUS = {1},重入量REENTRANT = {2}】", node.thread.ManagedThreadId, node.getStatus(node.Status), writerReentrants);
                            }
                            else
                            {
                                Console.WriteLine("Reader【持有线程ID = {0},运行状态STATUS = {1},重入量REENTRANT = {2}】", node.thread.ManagedThreadId, node.getStatus(node.Status), readerReentrants);
                            }
                        }
                        else
                        {
                            waitNode += 1;
                        }
                    }
                    Console.WriteLine("等待的线程数: {0}", waitNode);
                    Console.WriteLine(sep);
                }
            }
        }
    }
}
