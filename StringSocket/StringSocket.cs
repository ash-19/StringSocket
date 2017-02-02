// Written by Joe Zachary for CS 3500, November 2012
// Revised by Joe Zachary April 2016

// Modified by Snehashish Mishra on 25th April for CS 3500 
// offered by The University of Utah, Spring 2016.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace CustomNetworking
{
    /// <summary> 
    /// A StringSocket is a wrapper around a Socket.  It provides methods that
    /// asynchronously read lines of text (strings terminated by newlines) and 
    /// write strings. (As opposed to Sockets, which read and write raw bytes.)  
    ///
    /// StringSockets are thread safe.  This means that two or more threads may
    /// invoke methods on a shared StringSocket without restriction.  The
    /// StringSocket takes care of the synchronization.
    /// 
    /// Each StringSocket contains a Socket object that is provided by the client.  
    /// A StringSocket will work properly only if the client refrains from calling
    /// the contained Socket's read and write methods.
    /// 
    /// If we have an open Socket s, we can create a StringSocket by doing
    /// 
    ///    StringSocket ss = new StringSocket(s, new UTF8Encoding());
    /// 
    /// We can write a string to the StringSocket by doing
    /// 
    ///    ss.BeginSend("Hello world", callback, payload);
    ///    
    /// where callback is a SendCallback (see below) and payload is an arbitrary object.
    /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
    /// successfully written the string to the underlying Socket, or failed in the 
    /// attempt, it invokes the callback.  The parameters to the callback are a
    /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
    /// the Exception that caused the send attempt to fail.
    /// 
    /// We can read a string from the StringSocket by doing
    /// 
    ///     ss.BeginReceive(callback, payload)
    ///     
    /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
    /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
    /// string of text terminated by a newline character from the underlying Socket, or
    /// failed in the attempt, it invokes the callback.  The parameters to the callback are
    /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
    /// string or the Exception will be non-null, but nor both.  If the string is non-null, 
    /// it is the requested string (with the newline removed).  If the Exception is non-null, 
    /// it is the Exception that caused the send attempt to fail.
    /// </summary>

    public class StringSocket
    {
        /// <summary>
        /// The type of delegate that is called when a send has completed.
        /// </summary>
        public delegate void SendCallback(Exception e, object payload);

        /// <summary>
        /// The type of delegate that is called when a receive has completed.
        /// </summary>
        public delegate void ReceiveCallback(String s, Exception e, object payload);

        /// <summary>
        /// Contains the info on a single send request (in queue).
        /// </summary>
        private struct SendRequest
        {
            /// <summary>
            /// The string of text
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// The callback object (send)
            /// </summary>
            public SendCallback SendBack { get; set; }

            /// <summary>
            /// The payload.
            /// </summary>
            public object Payload { get; set; }
        }

        /// <summary>
        /// Contains the info on a single send request (in queue).
        /// </summary>
        private struct ReceiveRequest
        {
            /// <summary>
            /// The callback object (receive)
            /// </summary>
            public ReceiveCallback ReceiveBack { get; set; }

            /// <summary>
            /// The payload
            /// </summary>
            public object Payload { get; set; }
        }

        /// <summary>
        /// The underlying Socket
        /// </summary>
        private Socket socket;

        /// <summary>
        /// Queue to process sends in FIFO, thread-safe way
        /// </summary>
        private Queue<SendRequest> sendQueue;

        /// <summary>
        /// Queue to process receives in FIFO, thread-safe way
        /// </summary>
        private Queue<ReceiveRequest> receiveQueue;

        /// <summary>
        /// Contains the character encoding being used.
        /// </summary>
        private Encoding encoding;

        /// <summary>
        /// Buffer size for reading incoming bytes
        /// </summary>
        private const int BUFFER_SIZE = 1024;

        /// <summary>
        /// Array containing the bytes to be sent
        /// </summary>
        private byte[] sendBytes;

        /// <summary>
        /// Contains index of the leftmost byte whose send has not 
        /// been completed.
        /// </summary>
        private int sendCount;

        /// <summary>
        /// Buffer that contains the incoming bytes
        /// </summary>
        private byte[] incomingBytes = new byte[BUFFER_SIZE];

        /// <summary>
        /// Buffer that contains the incoming characters
        /// </summary>
        private char[] incomingChars = new char[BUFFER_SIZE];

        /// <summary>
        /// Contains the incoming text
        /// </summary>
        private string incomingLine = "";

        /// <summary>
        /// Contains the lines received in FIFO manner
        /// </summary>
        private Queue<string> linesReceivedQueue;

        /// <summary>
        /// Creates a StringSocket from a regular Socket, which should already be connected.  
        /// The read and write methods of the regular Socket must not be called after the
        /// StringSocket is created.  Otherwise, the StringSocket will not behave properly.  
        /// The encoding to use to convert between raw bytes and strings is also provided.
        /// </summary>
        public StringSocket(Socket s, Encoding e)
        {
            this.socket = s;
            this.encoding = e;
            this.sendQueue = new Queue<StringSocket.SendRequest>();
            this.receiveQueue = new Queue<StringSocket.ReceiveRequest>();
            this.linesReceivedQueue = new Queue<string>();
        }

        /// <summary>
        /// Shuts down and closes the socket.  No need to change this.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// We can write a string to a StringSocket ss by doing
        /// 
        ///    ss.BeginSend("Hello world", callback, payload);
        ///    
        /// where callback is a SendCallback (see above) and payload is an arbitrary object.
        /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
        /// successfully written the string to the underlying Socket, or failed in the 
        /// attempt, it invokes the callback.  The parameters to the callback are a
        /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
        /// the Exception that caused the send attempt to fail. 
        /// 
        /// This method is non-blocking.  This means that it does not wait until the string
        /// has been sent before returning.  Instead, it arranges for the string to be sent
        /// and then returns.  When the send is completed (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginSend
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginSend must take care of synchronization instead.  On a given StringSocket, each
        /// string arriving via a BeginSend method call must be sent (in its entirety) before
        /// a later arriving string can be sent.
        /// </summary>
        public void BeginSend(String s, SendCallback callback, object payload)
        {
            // Get exclusive access
            lock (sendQueue)
            {
                // Enqueue the received 'send' request with its parameters
                sendQueue.Enqueue(new SendRequest()
                {
                    Text = s,
                    SendBack = callback,
                    Payload = payload
                });
                if (sendQueue.Count != 1)
                    return;

                // Call the helper method to properly take care of sending the queued strings
                HandleSendQueue();
            }
        }

        /// <summary>
        /// We can read a string from the StringSocket by doing
        /// 
        ///     ss.BeginReceive(callback, payload)
        ///     
        /// where callback is a ReceiveCallback (see above) and payload is an arbitrary object.
        /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
        /// string of text terminated by a newline character from the underlying Socket, or
        /// failed in the attempt, it invokes the callback.  The parameters to the callback are
        /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
        /// string or the Exception will be null, or possibly boh.  If the string is non-null, 
        /// it is the requested string (with the newline removed).  If the Exception is non-null, 
        /// it is the Exception that caused the send attempt to fail.  If both are null, this
        /// indicates that the sending end of the remote socket has been shut down.
        ///  
        /// Alternatively, we can read a string from the StringSocket by doing
        /// 
        ///     ss.BeginReceive(callback, payload, length)
        ///     
        /// If length is negative or zero, this behaves identically to the first case.  If length
        /// is length, then instead of sending the next complete line in the callback, it sends
        /// the next length characters.  In other respects, it behaves analogously to the first case.
        /// 
        /// This method is non-blocking.  This means that it does not wait until a line of text
        /// has been received before returning.  Instead, it arranges for a line to be received
        /// and then returns.  When the line is actually received (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginReceive
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginReceive must take care of synchronization instead.  On a given StringSocket, each
        /// arriving line of text must be passed to callbacks in the order in which the corresponding
        /// BeginReceive call arrived.
        /// 
        /// Note that it is possible for there to be incoming bytes arriving at the underlying Socket
        /// even when there are no pending callbacks.  StringSocket implementations should refrain
        /// from buffering an unbounded number of incoming bytes beyond what is required to service
        /// the pending callbacks.
        /// </summary>
        public void BeginReceive(ReceiveCallback callback, object payload, int length = 0)
        {
            lock (receiveQueue)
            {
                receiveQueue.Enqueue(new ReceiveRequest()
                {
                    ReceiveBack = callback,
                    Payload = payload
                });
                if (receiveQueue.Count != 1)
                    return;
                HandleReceiveQueue();
            }
        }

        /// <summary>
        /// This helper method sends out all the strings in the queue by calling 
        /// the BytesSent callback again and again. It tries to send the string at the 
        /// front of queue. It contains a call to another helper method BytesSent 
        /// to ensure all the bytes from the front string are sent before calling this 
        /// method again to send the next string. 
        /// 
        /// IMPLEMENTATION NOTE: Should be called from within a lock for thread-safety.
        /// </summary>
        private void HandleSendQueue()
        {
            while (sendQueue.Count > 0)
            {
                sendBytes = encoding.GetBytes(sendQueue.First<SendRequest>().Text);
                try
                {
                    socket.BeginSend(sendBytes, sendCount = 0, sendBytes.Length,
                        SocketFlags.None, new AsyncCallback(BytesSent), (object)null);
                    break;
                }
                catch (Exception ex)
                {
                    SendRequest request = sendQueue.Dequeue();
                    ThreadPool.QueueUserWorkItem((WaitCallback)(x => request.SendBack(ex, request.Payload)));
                }
            }
        }

        /// <summary>
        /// The callback method used when sending bytes. It makes sure that all of the bytes 
        /// are sent before making the approriate callback and call to HandleSendQueue.
        /// 
        /// IMPLEMENTATION NOTE: Should be called from within a lock for thread-safety.
        /// </summary>
        /// <param name="arg">Object containing status of asyn operation</param>
        private void BytesSent(IAsyncResult arg)
        {
            try
            {
                sendCount = sendCount + socket.EndSend(arg);
            }
            catch (Exception ex)
            {
                SendRequest request = sendQueue.Dequeue();
                ThreadPool.QueueUserWorkItem((WaitCallback)(x => request.SendBack(ex, request.Payload)));
                HandleSendQueue();
                return;
            }

            if (sendCount == sendBytes.Length)
            {
                lock (sendQueue)
                {
                    SendRequest req = sendQueue.Dequeue();
                    ThreadPool.QueueUserWorkItem((WaitCallback)(x => req.SendBack((Exception)null, req.Payload)));
                    HandleSendQueue();
                }
            }
            else
            {
                try
                {
                    socket.BeginSend(sendBytes, sendCount, sendBytes.Length - sendCount,
                        SocketFlags.None, new AsyncCallback(BytesSent), (object)null);
                }
                catch (Exception ex)
                {
                    SendRequest request = sendQueue.Dequeue();
                    ThreadPool.QueueUserWorkItem((WaitCallback)(x => request.SendBack(ex, request.Payload)));
                    HandleSendQueue();
                }
            }
        }

        /// <summary>
        /// This helper method fulfils the requests (from queue) with the current 
        /// available text and if more request are remaining to be handled, 
        /// asks for more text via the socket.
        /// </summary>
        private void HandleReceiveQueue()
        {
            lock (receiveQueue)
            {
                while (receiveQueue.Count<ReceiveRequest>() > 0)
                {
                    if (linesReceivedQueue.Count > 0)
                    {
                        string line = linesReceivedQueue.Dequeue();
                        ReceiveRequest request = receiveQueue.Dequeue();
                        ThreadPool.QueueUserWorkItem((WaitCallback)(x => request.ReceiveBack(line,
                            (Exception)null, request.Payload)));
                    }
                    else
                        break;
                }
                while (receiveQueue.Count > 0)
                {
                    try
                    {
                        socket.BeginReceive(incomingBytes, 0, incomingBytes.Length, SocketFlags.None,
                            new AsyncCallback(BytesReceived), (object)null);
                        break;
                    }
                    catch (Exception ex)
                    {
                        ReceiveRequest request = receiveQueue.Dequeue();
                        ThreadPool.QueueUserWorkItem((WaitCallback)(x => request.ReceiveBack((string)null, ex, request.Payload)));
                        incomingLine = "";
                    }
                }
            }
        }

        /// <summary>
        /// This private method is the callback for the receive attempts.
        /// </summary>
        private void BytesReceived(IAsyncResult arg)
        {
            int byteCount;
            try
            {
                byteCount = socket.EndReceive(arg);
            }
            catch (Exception ex)
            {
                ReceiveRequest request = receiveQueue.Dequeue();
                ThreadPool.QueueUserWorkItem((WaitCallback)(x => request.ReceiveBack((string)null, ex, request.Payload)));
                HandleReceiveQueue();
                incomingLine = "";
                return;
            }
            if (byteCount == 0)
            {
                linesReceivedQueue.Enqueue((string)null);
                HandleReceiveQueue();
            }
            else
            {
                incomingLine = incomingLine + new string(incomingChars, 0,
                    encoding.GetDecoder().GetChars(incomingBytes, 0, byteCount, incomingChars, 0, false));
                int startIndex;
                int num;
                for (startIndex = 0; (num = incomingLine.IndexOf('\n', startIndex)) >= 0; startIndex = num + 1)
                    linesReceivedQueue.Enqueue(incomingLine.Substring(startIndex, num - startIndex));
                incomingLine = incomingLine.Substring(startIndex);
                HandleReceiveQueue();
            }
        }
    }
}