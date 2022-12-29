using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using TransformFunction.Models;
using TransformFunction.Services;
using Xunit;

namespace TransformFunctionUnitTest
{
    public class CSVConverterTests
    {
        [Fact]
        public void Valid_Message_ServiceBusTrigger_Test()
        {
            string csvData = @"Country,Country_UN,Admin_Name_Unicode,City,City_Unicode,Name" + Environment.NewLine + "Jordan,JOR,Ajlūn,Ajlun,Ajlūn,Krishna";
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            string output = @"{""CountryNameOnly"":""Jordan"",""CountryCode"":""JOR"",""AdminName"":""Ajlūn"",""CityName"":""Ajlun"",""CityCode"":""Ajlūn"",""Name"":""Krishna""}";
            CSVConverter obj = new CSVConverter();
            Assert.Equal(output, obj.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
            Assert.NotNull(obj.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
        }

        [Fact]
        public void InValid_Message_ServiceBusTrigger_Test()
        {
            string csvData = @"Country,Country_UN,Admin_Name_Unicode,City,City_Unicode,Name" + Environment.NewLine + "Jordan,JOR,Ajlūn,Ajlun,Ajlūn,Krishna";
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            string output = @"{""CountryNameOnly"":""Jordan"",""CountryCode"":""JOR"",""AdminName"":""Ajlūn"",""CityName"":""Ajlun"",""CityCode"":""Ajlūn"",""Name"":""Krishna123""}";
            CSVConverter obj = new CSVConverter();
            Assert.NotEqual(output, obj.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
            Assert.NotNull(obj.GetJson(GenerateStreamFromString(csvData), targetConfigJson));
        }

        [Fact]
        public void InValid_Message_ServiceBusTrigger_Returns_NullValue()
        {
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            CSVConverter obj = new CSVConverter();
            Assert.Null(obj.GetJson(null, targetConfigJson));
        }

        [Fact]
        public async Task InValid_Message_ServiceBusTrigger_Returns_NullValueAsync()
        {
            string targetConfigJson = @"{""Country"":""CountryNameOnly"",""Country_UN"":""CountryCode"",""Admin_Name_Unicode"":""AdminName"",""City"":""CityName"",""City_Unicode"":""CityCode"",""Name"":""Name""}";
            CSVConverter obj = new CSVConverter();
            Assert.Null(await obj.GetJsonAsync(null, targetConfigJson));
        }

        [Fact]
        public void InValid_JsonTarget_Message_ServiceBusTrigger_Returns_NullValue()
        {
            string csvData = @"Country,Country_UN,Admin_Name_Unicode,City,City_Unicode,Name" + Environment.NewLine + "Jordan,JOR,Ajlūn,Ajlun,Ajlūn,Krishna";
            CSVConverter obj = new CSVConverter();
            Assert.Null(obj.GetJson(GenerateStreamFromString(csvData), null));
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