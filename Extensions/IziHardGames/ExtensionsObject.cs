using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Xml.Serialization;

namespace System
{
    public static class ExtensionsObject
    {
        ///https://www.c-sharpcorner.com/UploadFile/ff2f08/deep-copy-of-object-in-C-Sharp/
        public static object DeepCopyWithFieldsAndProps(this object objSource)
        {
            //Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);
            //Get all the properties of source object type
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    //check whether property type is value type, enum or string type
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    else
                    {
                        object objPropertyValue = property.GetValue(objSource, null);
                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else
                        {
                            property.SetValue(objTarget, objPropertyValue.DeepCopyWithFieldsAndProps(), null);
                        }
                    }
                }
            }

            var fieldsInfos = typeSource.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var info in fieldsInfos)
            {
                if (info.FieldType.IsValueType || info.FieldType.IsEnum || info.FieldType.Equals(typeof(System.String)))
                {
                    info.SetValue(objTarget, info.GetValue(objSource));
                }
                else
                {
                    object objPropertyValue = info.GetValue(objSource);

                    if (objPropertyValue == null)
                    {
                        info.SetValue(objTarget, null);
                    }
                    else
                    {
                        info.SetValue(objTarget, objPropertyValue.DeepCopyWithFieldsAndProps());
                    }
                }
            }

            return objTarget;
        }

        public static T DeepCopyWithXml<T>(this object origin) where T : class
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new XmlSerializer(origin.GetType());
                formatter.Serialize(stream, origin);
                return formatter.Deserialize(stream) as T;
            }
        }
        public static T DeepCopyWithJson<T>(this object origin)
        {
            string json = JsonSerializer.Serialize(origin);
            return JsonSerializer.Deserialize<T>(json);
        }
#pragma warning disable
        public static T DeepCopyForSerilizable<T>(this object origin)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, origin);
                ms.Position = 0;
                return (T)bf.Deserialize(ms);
            }
        }
    }
}