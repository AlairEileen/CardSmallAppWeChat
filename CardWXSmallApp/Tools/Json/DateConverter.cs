﻿using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Tools
{
    public class DateConverter : JsonConverter
    {
        private DateTime dateTime;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }
        string format = "yyyy年MM月dd日";
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType!=JsonToken.String)
            {
                throw new Exception(String.Format("Unexpected token parsing ObjectId. Expected String, got {0}.",reader.TokenType));
            }
            var value = (string)reader.Value;
            return String.IsNullOrEmpty(value) ? DateTime.Now : DateTime.ParseExact(value, format, System.Globalization.CultureInfo.CurrentCulture); ;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                var objId = (DateTime)value;
                writer.WriteValue(objId != null&& !objId.Equals(dateTime) ? objId.ToString(format):null);
            }
            else
            {
                throw new Exception("Expected ObjectId value.");
            }
        }
    }
}