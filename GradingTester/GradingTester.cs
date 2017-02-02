using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using CustomNetworking;
using System.Threading.Tasks;

namespace GradingTester
{
    [TestClass]
    public class StringSocketTest
    {
        /// <summary>
        /// Opens and returns (with out parameters) a pair of communicating sockets.
        /// </summary>
        private static void OpenSockets(int port, out TcpListener server, out Socket s1, out Socket s2)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            TcpClient client = new TcpClient("localhost", port);
            s1 = server.AcceptSocket();
            s2 = client.Client;
        }

        /// <summary>
        /// Closes stuff down
        /// </summary>
        private static void CloseSockets(TcpListener server, StringSocket s1, SS s2)
        {
            try
            {
                s1.Shutdown();
            }
            finally
            {
            }
            try
            {
                s2.Shutdown();
            }
            finally
            {
            }
            try
            {
                server.Stop();
            }
            finally
            {
            }
        }

        /// <summary>
        /// Tests whether StringSocket can receive a line of text
        /// </summary>
        [TestMethod()]
        public void Test1()
        {
            new Test1Class().run(4001);
        }

        public class Test1Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String line = "";
                ManualResetEvent mre = new ManualResetEvent(false);
                SS sender = null;
                StringSocket receiver = null;
                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    sender.BeginSend("Hello\n", (e, p) => { }, null);
                    receiver.BeginReceive((s, e, p) => { line = s; mre.Set(); }, null);
                    mre.WaitOne();
                    Assert.AreEqual("Hello", line);
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Tests whether StringSocket can send a line of text
        /// </summary>
        [TestMethod()]
        public void Test2()
        {
            new Test2Class().run(4002);
        }

        public class Test2Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String line = "";
                ManualResetEvent mre = new ManualResetEvent(false);
                StringSocket sender = null;
                SS receiver = null;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    sender.BeginSend("Hello\n", (e, p) => { }, null);
                    receiver.BeginReceive((s, e, p) => { line = s; mre.Set(); }, null);
                    mre.WaitOne();
                    Assert.AreEqual("Hello", line);
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }

        /// <summary>
        /// Like Test1, make sure payload comes through.
        /// </summary>
        [TestMethod()]
        public void Test3()
        {
            new Test3Class().run(4003);
        }

        public class Test3Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                object payload = null;
                ManualResetEvent mre = new ManualResetEvent(false);
                StringSocket receiver = null;
                SS sender = null;

                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    sender.BeginSend("Hello\n", (e, p) => { }, null);
                    receiver.BeginReceive((s, e, p) => { payload = p; mre.Set(); }, "Payload");
                    mre.WaitOne();
                    Assert.AreEqual("Payload", payload);
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Like Test2, make sure the payload comes through
        /// </summary>
        [TestMethod()]
        public void Test4()
        {
            new Test4Class().run(4004);
        }

        public class Test4Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                object payload = null;
                ManualResetEvent mre = new ManualResetEvent(false);
                StringSocket sender = null;
                SS receiver = null;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    sender.BeginSend("Hello\n", (e, p) => { payload = p; mre.Set(); }, "Payload");
                    mre.WaitOne();
                    Assert.AreEqual("Payload", payload);
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }

        /// <summary>
        /// Like Test1, but send one character at a time.
        /// </summary>
        [TestMethod()]
        public void Test5()
        {
            new Test5Class().run(4005);
        }

        public class Test5Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String line = "";
                ManualResetEvent mre = new ManualResetEvent(false);
                StringSocket receiver = null;
                SS sender = null;

                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    foreach (char c in "Hello\n")
                    {
                        sender.BeginSend(c.ToString(), (e, p) => { }, null);
                    }
                    receiver.BeginReceive((s, e, p) => { line = s; mre.Set(); }, null);
                    mre.WaitOne();
                    Assert.AreEqual("Hello", line);
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Like Test2, but send one character at a time.
        /// </summary>
        [TestMethod()]
        public void Test6()
        {
            new Test6Class().run(4006);
        }

        public class Test6Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String line = "";
                ManualResetEvent mre = new ManualResetEvent(false);
                StringSocket sender = null;
                SS receiver = null;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    foreach (char c in "Hello\n")
                    {
                        sender.BeginSend(c.ToString(), (e, p) => { }, null);
                    }
                    receiver.BeginReceive((s, e, p) => { line = s; mre.Set(); }, null);
                    mre.WaitOne();
                    Assert.AreEqual("Hello", line);
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }

        /// <summary>
        /// Like Test1, but send a very long string.
        /// </summary>
        [TestMethod()]
        public void Test7()
        {
            new Test7Class().run(4007);
        }

        public class Test7Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String line = "";
                ManualResetEvent mre = new ManualResetEvent(false);
                StringSocket receiver = null;
                SS sender = null;

                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    StringBuilder text = new StringBuilder();
                    for (int i = 0; i < 100000; i++)
                    {
                        text.Append(i);
                    }
                    String str = text.ToString();
                    text.Append('\n');
                    sender.BeginSend(text.ToString(), (e, p) => { }, null);
                    receiver.BeginReceive((s, e, p) => { line = s; mre.Set(); }, null);
                    mre.WaitOne();
                    Assert.AreEqual(str, line);
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Like Test2, but send a very long string.
        /// </summary>
        [TestMethod()]
        public void Test8()
        {
            new Test8Class().run(4008);
        }

        public class Test8Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String line = "";
                ManualResetEvent mre = new ManualResetEvent(false);
                StringSocket sender = null;
                SS receiver = null;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    StringBuilder text = new StringBuilder();
                    for (int i = 0; i < 100000; i++)
                    {
                        text.Append(i);
                    }
                    String str = text.ToString();
                    text.Append('\n');
                    sender.BeginSend(text.ToString(), (e, p) => { }, null);
                    receiver.BeginReceive((s, e, p) => { line = s; mre.Set(); }, null);
                    mre.WaitOne();
                    Assert.AreEqual(str, line);
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }

        /// <summary>
        /// Send multiple lines, make sure they're received in order.
        /// </summary>
        [TestMethod()]
        public void Test9()
        {
            new Test9Class().run(4009);
        }

        public class Test9Class
        {
            public void run(int port)
            {
                int LIMIT = 1000;
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String[] lines = new String[LIMIT];
                ManualResetEvent mre = new ManualResetEvent(false);
                int count = 0;
                StringSocket receiver = null;
                SS sender = null;

                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    for (int i = 0; i < LIMIT; i++)
                    {
                        receiver.BeginReceive((s, e, p) => { lines[(int)p] = s; Interlocked.Increment(ref count); }, i);
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        sender.BeginSend(i.ToString() + "\n", (e, p) => { }, null);
                    }
                    if (!SpinWait.SpinUntil(() => count == LIMIT, 5000))
                    {
                        Assert.Fail();
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        Assert.AreEqual(i.ToString(), lines[i]);
                    }
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Like Test9, but the class being tested does the sending.
        /// </summary>
        [TestMethod()]
        public void Test10()
        {
            new Test10Class().run(4010);
        }

        public class Test10Class
        {
            public void run(int port)
            {
                int LIMIT = 1000;
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                String[] lines = new String[LIMIT];
                ManualResetEvent mre = new ManualResetEvent(false);
                int count = 0;
                StringSocket sender = null;
                SS receiver = null;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    for (int i = 0; i < LIMIT; i++)
                    {
                        receiver.BeginReceive((s, e, p) => { lines[(int)p] = s; Interlocked.Increment(ref count); }, i);
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        sender.BeginSend(i.ToString() + "\n", (e, p) => { }, null);
                    }

                    if (!SpinWait.SpinUntil(() => count == LIMIT, 5000))
                    {
                        Assert.Fail();
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        Assert.AreEqual(i.ToString(), lines[i]);
                    }
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }

        /// <summary>
        /// Like Test9, except the receive calls are made on separate threads.
        /// </summary>
        [TestMethod()]
        public void Test11()
        {
            new Test11Class().run(4011);
        }

        public class Test11Class
        {
            public void run(int port)
            {
                int LIMIT = 1000;
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                List<int> lines = new List<int>();
                StringSocket receiver = null;
                SS sender = null;

                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    for (int i = 0; i < LIMIT; i++)
                    {
                        ThreadPool.QueueUserWorkItem(x =>
                            receiver.BeginReceive((s, e, p) => { lock (lines) { lines.Add(Int32.Parse(s)); } }, null));
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        sender.BeginSend(i.ToString() + "\n", (e, p) => { }, null);
                    }
                    if (!SpinWait.SpinUntil(() => { lock (lines) { return lines.Count == LIMIT; } }, 5000))
                    {
                        Assert.Fail();
                    }

                    lines.Sort();
                    for (int i = 0; i < LIMIT; i++)
                    {
                        Assert.AreEqual(i, lines[i]);
                    }
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Like Test10, except the send calls are made on separate threads.
        /// </summary>
        [TestMethod()]
        public void Test12()
        {
            new Test12Class().run(4012);
        }

        public class Test12Class
        {
            public void run(int port)
            {
                int LIMIT = 1000;
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                List<int> lines = new List<int>();
                StringSocket sender = null;
                SS receiver = null;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    for (int i = 0; i < LIMIT; i++)
                    {
                        receiver.BeginReceive((s, e, p) => { lock (lines) { lines.Add(Int32.Parse(s)); } }, null);
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        String s = i.ToString();
                        ThreadPool.QueueUserWorkItem(x =>
                            sender.BeginSend(s + "\n", (e, p) => { }, null));
                    }
                    if (!SpinWait.SpinUntil(() => { lock (lines) { return lines.Count == LIMIT; } }, 5000))
                    {
                        Assert.Fail();
                    }

                    lines.Sort();
                    for (int i = 0; i < LIMIT; i++)
                    {
                        Assert.AreEqual(i, lines[i]);
                    }
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }

        /// <summary>
        /// Like Test9, except the sends and receives are interleaved.
        /// </summary>
        [TestMethod()]
        public void Test13()
        {
            new Test13Class().run(4013);
        }

        public class Test13Class
        {
            public void run(int port)
            {
                int LIMIT = 1000;
                int count = LIMIT;
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                List<int> lines = new List<int>();
                for (int i = 0; i < LIMIT; i++)
                {
                    lines.Add(-1);
                }
                StringSocket receiver = null;
                SS sender = null;

                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    for (int i = 0; i < LIMIT / 4; i++)
                    {
                        int j = i;
                        receiver.BeginReceive((s, e, p) => { lock (lines) { lines[j] = Int32.Parse(s); } Interlocked.Decrement(ref count); }, null);
                    }
                    for (int i = 0; i < LIMIT / 2; i++)
                    {
                        sender.BeginSend(i.ToString() + "\n", (e, p) => { }, null);
                    }
                    for (int i = LIMIT / 4; i < 3 * LIMIT / 4; i++)
                    {
                        int j = i;
                        receiver.BeginReceive((s, e, p) => { lock (lines) { lines[j] = Int32.Parse(s); } Interlocked.Decrement(ref count); }, null);
                    }
                    for (int i = LIMIT / 2; i < LIMIT; i++)
                    {
                        sender.BeginSend(i.ToString() + "\n", (e, p) => { }, null);
                    }
                    for (int i = 3 * LIMIT / 4; i < LIMIT; i++)
                    {
                        int j = i;
                        receiver.BeginReceive((s, e, p) => { lock (lines) { lines[j] = Int32.Parse(s); } Interlocked.Decrement(ref count); }, null);
                    }

                    if (!SpinWait.SpinUntil(() => count == 0, 5000))
                    {
                        Assert.Fail();
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        Assert.AreEqual(i, lines[i]);
                    }
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Like Test10, except the sends and receives are interleaved.
        /// </summary>
        [TestMethod()]
        public void Test14()
        {
            new Test14Class().run(4014);
        }

        public class Test14Class
        {
            public void run(int port)
            {
                int LIMIT = 1000;
                int count = LIMIT;
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                List<int> lines = new List<int>();
                for (int i = 0; i < LIMIT; i++)
                {
                    lines.Add(-1);
                }
                StringSocket sender = null;
                SS receiver = null;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    for (int i = 0; i < LIMIT / 4; i++)
                    {
                        int j = i;
                        receiver.BeginReceive((s, e, p) => { lock (lines) { lines[j] = Int32.Parse(s); } Interlocked.Decrement(ref count); }, null);
                    }
                    for (int i = 0; i < LIMIT / 2; i++)
                    {
                        sender.BeginSend(i.ToString() + "\n", (e, p) => { }, null);
                    }
                    for (int i = LIMIT / 4; i < 3 * LIMIT / 4; i++)
                    {
                        int j = i;
                        receiver.BeginReceive((s, e, p) => { lock (lines) { lines[j] = Int32.Parse(s); } Interlocked.Decrement(ref count); }, null);
                    }
                    for (int i = LIMIT / 2; i < LIMIT; i++)
                    {
                        sender.BeginSend(i.ToString() + "\n", (e, p) => { }, null);
                    }
                    for (int i = 3 * LIMIT / 4; i < LIMIT; i++)
                    {
                        int j = i;
                        receiver.BeginReceive((s, e, p) => { lock (lines) { lines[j] = Int32.Parse(s); Interlocked.Decrement(ref count); } }, null);
                    }

                    if (!SpinWait.SpinUntil(() => count == 0, 5000))
                    {
                        Assert.Fail();
                    }
                    for (int i = 0; i < LIMIT; i++)
                    {
                        Assert.AreEqual(i, lines[i]);
                    }
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }

        /// <summary>
        /// Blocks the receive callbacks, makes sure receiving keeps working.
        /// </summary>
        [TestMethod()]
        public void Test15()
        {
            new Test15Class().run(4015);
        }

        public class Test15Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                StringSocket receiver = null;
                SS sender = null;
                int LIMIT = 2;

                try
                {
                    sender = new SS(s1, new UTF8Encoding());
                    receiver = new StringSocket(s2, new UTF8Encoding());
                    int count = LIMIT;

                    for (int i = 0; i < LIMIT; i++)
                    {
                        sender.BeginSend("Hello\n", (e, p) => { }, null);
                    }

                    for (int i = 0; i < LIMIT; i++)
                    {
                        Task.Run(
                            () => receiver.BeginReceive((s, e, p) => { Interlocked.Decrement(ref count); while (true) ; }, null));
                    }
                    if (!SpinWait.SpinUntil(() => count == 0, 5000))
                    {
                        Assert.Fail();
                    }
                }
                finally
                {
                    CloseSockets(server, receiver, sender);
                }
            }
        }

        /// <summary>
        /// Blocks the send callbacks, makes sure receiving keeps working.
        /// </summary>
        [TestMethod()]
        public void Test16()
        {
            new Test16Class().run(4016);
        }

        public class Test16Class
        {
            public void run(int port)
            {
                Socket s1, s2;
                TcpListener server;
                OpenSockets(port, out server, out s1, out s2);
                SS receiver = null;
                StringSocket sender = null;
                int LIMIT = 2;

                try
                {
                    sender = new StringSocket(s1, new UTF8Encoding());
                    receiver = new SS(s2, new UTF8Encoding());
                    int count1 = LIMIT;
                    int count2 = LIMIT;

                    for (int i = 0; i < LIMIT; i++)
                    {
                        sender.BeginSend("Hello\n", (e, p) => { Interlocked.Decrement(ref count1); while (true) ; }, null);
                    }

                    for (int i = 0; i < LIMIT; i++)
                    {
                        receiver.BeginReceive((s, e, p) => { Interlocked.Decrement(ref count2); }, null);
                    }

                    if (!SpinWait.SpinUntil(() => count1 == 0 && count2 == 0, 5000))
                    {
                        Assert.Fail();
                    }
                }
                finally
                {
                    CloseSockets(server, sender, receiver);
                }
            }
        }
    }
}
