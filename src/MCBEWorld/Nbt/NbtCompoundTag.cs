using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MCBEWorld.Nbt
{
    internal sealed class NbtCompoundTag : INbtStream, INbtTag, INbtDictionary
    {
        private List<KeyValuePair<string, INbtTag>> children_;
        private INbtStream source_;
        private int index_;
        private Dictionary<string, INbtTag> tags_;

        public NbtCompoundTag(string name, INbtStream sourceStream)
        {
            children_ = new List<KeyValuePair<string, INbtTag>>();
            tags_ = new Dictionary<string, INbtTag>();
            Name = name;
            source_ = sourceStream;
            index_ = -1;

            while (sourceStream.MoveNext())
            {
                var tag = sourceStream.CurrentValue;
                if (tag.Type == NbtTagType.EndTag)
                {
                    break;
                }
                KeyValuePair<string, INbtTag> pair = new KeyValuePair<string, INbtTag>(tag.Name, tag);
                children_.Add(pair);
                tags_.Add(tag.Name, tag);
            }
        }

        public bool HasValue => index_ >= 0 && index_ < children_.Count;

        public string Name { get; }

        public NbtTagType Type => NbtTagType.Compound;

        public bool MoveNext()
        {
            if (index_ < (children_.Count - 1))
            {
                index_++;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            index_ = -1;
        }

        public INbtTag CurrentValue => children_[index_].Value;

        public static INbtTag FromReader(string name, INbtStream source)
        {
            return new NbtCompoundTag(name, source);
        }

        public INbtTag this[string key] => tags_[key];
        public bool TryGetValue(string key, out INbtTag tag) => tags_.TryGetValue(key, out tag);

        public bool Contains(string key) => tags_.ContainsKey(key);
    }
}
