﻿using ORM_FrameWork;
using ORM_FrameWork.MetaModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Cache
{
    public class CacheTracking : Cache, ICache
    {

        protected Dictionary<Type, Dictionary<object, string>> Hashes = new Dictionary<Type, Dictionary<object, string>>();

        protected virtual Dictionary<object, string> GetHash(Type type)
        {
            if (Hashes.ContainsKey(type))
                return Hashes[type];

            Dictionary<object, string> value = new Dictionary<object, string>();
            Hashes.Add(type, value);

            return value;
        }

        protected string ComputeHash(object obj)
        {
            string value = string.Empty;
            foreach (Field f in ORMapper.GetEntity(obj).IntFields)
            {
                object o = f.GetValue(obj);

                if (o != null)
                {
                    if (f.IsFkey)
                        if (o != null)
                            value += ORMapper.GetEntity(o).PKey.GetValue(o).ToString();

                        else
                            value += $"{f.ColumnName}={o};";
                }
            }

            foreach (Field f in ORMapper.GetEntity(obj).ExtFields)
            {

                if ((IEnumerable)f.GetValue(obj) != null)
                {
                    value += $"{f.ColumnName}=";
                    foreach (object o in (IEnumerable)f.GetValue(obj))
                    {
                        value += $"{ORMapper.GetEntity(o).PKey.GetValue(o)},";
                    }
                }
            }

            return Encoding.UTF8.GetString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        public override void Put(object obj)
        {
            base.Put(obj);

            if (obj != null)
                GetHash(obj.GetType())[ORMapper.GetEntity(obj).PKey.GetValue(obj)] = ComputeHash(obj);
        }

        public override void Remove(object obj)
        {
            base.Remove(obj);
            GetHash(obj.GetType()).Remove(ORMapper.GetEntity(obj).PKey.GetValue(obj));
        }

        public override bool HasChanged(object obj)
        {
            Dictionary<object, string> hash = GetHash(obj.GetType());
            object pKey = ORMapper.GetEntity(obj).PKey.GetValue(obj);

            if (hash.ContainsKey(pKey))
                return (hash[pKey] == ComputeHash(obj));

            return true;
        }



    }
}
