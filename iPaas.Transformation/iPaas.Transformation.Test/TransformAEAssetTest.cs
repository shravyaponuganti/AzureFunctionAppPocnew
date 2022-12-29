using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using iPaas.Transformation.Interfaces;
using Moq;
using System;
using Xunit;

namespace iPaas.Transformation.Test
{


    public class TransformAEAssetTest
    {
        public TransformAEAssetTest()
        {

        }


        public static BlobContainerClient GetBlobContainerClientMock()
        {
            var mock = new Mock<BlobContainerClient>();
            mock.Setup(i => i.AccountName)
                .Returns("TestAccountName");
            return mock.Object;
        }

        //public static ServiceBusClient GetServiceBusClientMock()
        //{
        //    var mock = new Mock<ServiceBusClient>();
        //    mock.Setup(i => i.CreateSender(It.IsAny<string>()))
        //        .Returns("TestAccountName");
        //    return mock.Object;
        //}
    }
}