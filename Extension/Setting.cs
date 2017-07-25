using System.ComponentModel;
using System.Linq;

namespace RadbitMqDemo.Extension
{
    public sealed class Setting
    {
        private readonly InsensitiveDictionary<object> SettingDictionary = new InsensitiveDictionary<object>();


        public Setting()
        {
        }

        public Setting(InsensitiveDictionary<object> dictionary)
        {
            SettingDictionary = dictionary;
        }


        public object this[string key] => SettingDictionary[key];


        public void Add(string key, object value)
        {
            SettingDictionary.SetValue(key, value);
        }

        public Setting Clone()
        {
            InsensitiveDictionary<object> dictionary =
                new InsensitiveDictionary<object>(
                    SettingDictionary.ToDictionary(item => item.Key, item => item.Value));
            return new Setting(dictionary);
        }

        public string Get(string key)
        {
            return SettingDictionary[key].ToString();
        }

        public T Get<T>(string key)
        {
            object value = SettingDictionary[key];
            return value is T
                ? (T) value
                : (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value);
        }
    }
}