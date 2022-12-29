using iPaas.Transformation.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iPaas.Transformation.Test.Common
{
    public class CsvJsonConverterTest
    {
        [Fact]
        public void Valid_Message_ServiceBusTrigger_Test()
        {
            string csvData = @"Country,Country_UN,Admin_Name_Unicode,City,City_Unicode,Name" + Environment.NewLine + "Jordan,JOR,Ajlūn,Ajlun,Ajlūn,Krishna";
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            string output = @"{""CountryNameOnly"":""Jordan"",""CountryCode"":""JOR"",""AdminName"":""Ajlūn"",""CityName"":""Ajlun"",""CityCode"":""Ajlūn"",""Name"":""Krishna""}";
            Assert.Equal(output, CsvJsonConverter.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
            Assert.NotNull(CsvJsonConverter.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
        }

        [Fact]
        public void InValid_Message_ServiceBusTrigger_Test()
        {
            string csvData = @"Country,Country_UN,Admin_Name_Unicode,City,City_Unicode,Name" + Environment.NewLine + "Jordan,JOR,Ajlūn,Ajlun,Ajlūn,Krishna";
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            string output = @"{""CountryNameOnly"":""Jordan"",""CountryCode"":""JOR"",""AdminName"":""Ajlūn"",""CityName"":""Ajlun"",""CityCode"":""Ajlūn"",""Name"":""Krishna123""}";
            Assert.NotEqual(output, CsvJsonConverter.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
            Assert.NotNull(CsvJsonConverter.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
        }

        [Fact]
        public void InValid_Message_ServiceBusTrigger_Returns_NullValue()
        {
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            Assert.Empty(CsvJsonConverter.GetJson(null, targetConfigJson));
        }

        [Fact]
        public async Task InValid_Message_ServiceBusTrigger_Returns_NullValueAsync()
        {
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            Assert.Empty(await CsvJsonConverter.GetJsonAsync(null, targetConfigJson));
        }

        [Fact]
        public void InValid_JsonTarget_Message_ServiceBusTrigger_Returns_NullValue()
        {
            string csvData = @"Country,Country_UN,Admin_Name_Unicode,City,City_Unicode,Name" + Environment.NewLine + "Jordan,JOR,Ajlūn,Ajlun,Ajlūn,Krishna";
            Assert.Empty(CsvJsonConverter.GetJson(GenerateStreamFromString(csvData), null));
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}