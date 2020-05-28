using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ReentrantReadWriteLock
{
    public class Node
    {
        //任务类型:0—reader;1—writer
        public readonly int type;
        //持有线程
        public Thread thread;
        //任务状态
        public int Status;
        //前驱节点在唤醒读节点的时候用到
        public Node pre;

        //节点状态
        public static readonly int RUNNING = 1;
        public static readonly int WAITING = -1;

        //构造函数
        public Node(Thread thread, int type)
        {
            this.type = type;
            this.thread = thread;
        }

        //获取线程状态
        public string getStatus(int status)
        {
            switch (status)
            {
                case 1:
                    return "RUNNING";
                case -1:
                    return "WAITING";
            }
            return "DEFAULT";
        }

        //获取线程类型返回一个bool类型:writer—>true,reader—>false
        public bool getType()
        {
            if (type == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
