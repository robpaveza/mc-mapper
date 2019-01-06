using MCBEWorld.Nbt;
using System;
using System.IO;

namespace NbtReadingTester
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var file = File.OpenRead(@"C:\Users\Rob\Documents\Visual Studio 2017\Projects\MapGenExploration\ConsoleApp\bin\Debug\pendingticks-15.15.dat"))
            {
                var reader = NbtReader.Create(file);
                while (reader.MoveNext())
                {
                    var value = reader.CurrentValue;
                    ExploreValue(value, 0);
                }
            }
        }

        static void ExploreValue(INbtTag tag, int depth)
        {
            Console.Write("|-".PadRight(depth + 2, '-'));
            Console.Write($" {tag.Type}: {tag.Name} = ");

            switch (tag.Type)
            {
                case NbtTagType.Float32:
                    Console.WriteLine(tag.As<float>().Value);
                    break;

                case NbtTagType.Float64:
                    Console.WriteLine(tag.As<double>().Value);
                    break;

                case NbtTagType.Int16:
                    Console.WriteLine(tag.As<short>().Value);
                    break;

                case NbtTagType.Int32:
                    Console.WriteLine(tag.As<int>().Value);
                    break;

                case NbtTagType.Int64:
                    Console.WriteLine(tag.As<long>().Value);
                    break;

                case NbtTagType.UInt8:
                    Console.WriteLine(tag.As<byte>().Value);
                    break;

                case NbtTagType.Utf8String:
                    Console.WriteLine(tag.As<string>().Value);
                    break;

                case NbtTagType.Int32Array:
                    WriteArray(tag.AsArray<int>(), depth + 1);
                    break;

                case NbtTagType.Int64Array:
                    WriteArray(tag.AsArray<long>(), depth + 1);
                    break;

                case NbtTagType.UInt8Array:
                    WriteArray(tag.AsArray<byte>(), depth + 1);
                    break;

                case NbtTagType.List:
                    {
                        var listBase = tag as INbtList;
                        switch (listBase.Subtype)
                        {
                            case NbtTagType.Int16:
                                WriteSimpleList(listBase as INbtList<short>, depth + 1);
                                break;

                            case NbtTagType.Int32:
                                WriteSimpleList(listBase as INbtList<int>, depth + 1);
                                break;

                            case NbtTagType.Int64:
                                WriteSimpleList(listBase as INbtList<long>, depth + 1);
                                break;

                            case NbtTagType.UInt8:
                                WriteSimpleList(listBase as INbtList<byte>, depth + 1);
                                break;

                            case NbtTagType.Utf8String:
                                WriteSimpleList(listBase as INbtList<string>, depth + 1);
                                break;

                            case NbtTagType.Compound:
                                WriteCompoundList(listBase as INbtList<INbtStream>, depth + 1);
                                break;

                            default:
                                throw new NotSupportedException("Not yet supported.");
                        }
                        break;
                    }

                case NbtTagType.Compound:
                    Console.WriteLine("(compound item)");
                    var stream = tag.AsStream();
                    while (stream.MoveNext())
                    {
                        ExploreValue(stream.CurrentValue, depth + 1);
                    }
                    break;
            }
        }

        static void WriteArray<T>(INbtArrayTag<T> array, int depth)
            where T : struct
        {
            Console.WriteLine($"({array.Count}-item array):");
            foreach (T item in array)
            {
                Console.Write("|-".PadRight(2 + depth, '-'));
                Console.Write(":: ");
                Console.WriteLine(item);
            }
        }

        static void WriteSimpleList<T>(INbtList<T> list, int depth)
        {
            Console.WriteLine($"({list.Count}-item List of {list.Type}):");
            int index = 0;
            foreach (T item in list)
            {
                Console.Write("|-".PadRight(2 + depth, '-'));
                Console.Write($">> [{index}] ");
                Console.WriteLine(item);
            }
        }

        static void WriteCompoundList(INbtList<INbtStream> list, int depth)
        {
            Console.WriteLine($"({list.Count}-item List of Compound):");
            int index = 0;
            foreach (INbtStream item in list)
            {
                Console.Write("|-".PadRight(2 + depth, '-'));
                Console.WriteLine($">> [{index++}] (compound)");
                item.Reset();
                while (item.MoveNext())
                {
                    ExploreValue(item.CurrentValue, depth + 1);
                }
            }
        }
    }
}
