using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RedHaloM2B.Utils
{
    public class DictionaryEx<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// 自定义字典的反序列化规则
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            /*
            //声明键和值的“翻译器”
            XmlSerializer keySer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSer = new XmlSerializer(typeof(TValue));

            reader.Read();
            //只要没读到End节点 就一直读
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                //解析键
                TKey key = (TKey)keySer.Deserialize(reader);
                //解析值
                TValue value = (TValue)valueSer.Deserialize(reader);
                //把读到的值加进去
                this.Add(key, value);
            }
            //把End节点读了，避免影响之后的数据读取
            reader.Read();
            */

            if (reader.IsStartElement())
            {
                reader.ReadStartElement(); // 读取字典的开始标签
                while (reader.IsStartElement("Item"))
                {
                    reader.ReadStartElement("Item");

                    // 从 Item 元素的属性中读取 Key
                    TKey key = (TKey)Convert.ChangeType(reader.GetAttribute("CustomKey"), typeof(TKey));

                    // 读取 CustomValue 元素
                    reader.ReadStartElement("CustomValue");
                    TValue value = (TValue)Convert.ChangeType(reader.ReadString(), typeof(TValue));
                    reader.ReadEndElement();

                    // 添加到字典
                    this.Add(key, value);

                    reader.ReadEndElement(); // 读取 Item 的结束标签
                }
            }
        }

        /// <summary>
        /// 自定义字典的序列化规则
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            foreach (var kvp in this)
            {
                writer.WriteStartElement("Item");

                // 使用 XmlAttribute 来序列化 Key 为属性
                writer.WriteAttributeString("MyKey", kvp.Key.ToString());

                // 使用 XmlElement 来序列化 Value 为元素
                writer.WriteStartElement("CustomValue");
                writer.WriteString(kvp.Value.ToString());
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }
}
