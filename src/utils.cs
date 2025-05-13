using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using UI;

namespace Utils 
{
    public class Converter
    {
        public static byte[] StructToBytes<T>(T structure)
        {
            Int32 size = Marshal.SizeOf<T>();
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        public static T BytesToStruct<T>(byte[] bytes)
        {
            Int32 size = Marshal.SizeOf<T>();
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure<T>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

        }
    }
    public class Time
    {
        private Time(){}
        public static Int64 GetCurrentUTCTimeStamp()
        {
            long seconds = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            return seconds;
        }
    }
    public class Math
    {

        public static float Lerp(float min, float max, float progress)
        {
            return min + (max - min) * progress;
        }
    }
    public class CycleQueue<T>
    {
        protected int front;
        protected int rear;
        protected T[] _elements;
        
        public bool Empty { get { return rear == front;}}
        public bool Full {get {return (rear + 1) % _elements.Length == front;}}
        public int Size {get {return (rear - front + _elements.Length) % _elements.Length;}}
        public int Capacity {get {return _elements.Length - 1;}}
        public T this[int index]
        {
            get
            {
                return _elements[(index + front) % _elements.Length];
            }
        }
        public CycleQueue()
        {
            front = 0;
            rear = 0;
            _elements = new T[8];
        }
        public CycleQueue(int capacity)
        {
            front = 0;
            rear = 0;
            _elements = new T[capacity + 1];
        }
        public void Enqueue(T element)
        {
            if (!Full)
            {
                _elements[rear] = element;
                rear = (rear + 1) % _elements.Length;
                return;
            }
            throw new Exception("CycleQueue is full!");
        }
        public T Dequeue()
        {
            if (!Empty)
            {
                int i = front;
                front = (front + 1) % _elements.Length;
                return _elements[i];
            }
            throw new Exception("CycleQueue is empty!");
        }
        public T Peek()
        {
            if (!Empty)
            {
                return _elements[front];
            }
            throw new Exception("CycleQueue is empty!");
        }
        public void Resize(uint size)
        {
            T[] _new = new T[size];
            _elements.CopyTo(_new, 0);
            _elements = _new;
        }
        public void RemoveFromStart(uint length)
        {
            front  = (int)(front + length) % _elements.Length;
        }
        
    }
    
}