﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DeepCopyCSharp {
    class Program {
        [Serializable()]
        public class item {
            public int id;
            public string name;
        }

        static void Main(string[] args) {

            item a = new item();
            a.id = 12345;
            a.name = "hello";

            item b;

            // 拡張メソッド
            b = (item)a.DeepCopy();

            // ジェネリックメソッド

            b = DeepCopyHelper.DeepCopy<item>(a);

            b.id = 54635;

            Console.ReadLine();
        }

    }

    static class DeepCopyUtils {
        public static object DeepCopy(this object target) {

            object result;
            BinaryFormatter b = new BinaryFormatter();

            MemoryStream mem = new MemoryStream();

            try {
                b.Serialize(mem, target);
                mem.Position = 0;
                result = b.Deserialize(mem);
            } finally {
                mem.Close();
            }

            return result;

        }
    }

    public static class DeepCopyHelper {
        public static T DeepCopy<T>(T target) {

            T result;
            BinaryFormatter b = new BinaryFormatter();

            MemoryStream mem = new MemoryStream();

            try {
                b.Serialize(mem, target);
                mem.Position = 0;
                result = (T)b.Deserialize(mem);
            } finally {
                mem.Close();
            }

            return result;

        }
    }

}