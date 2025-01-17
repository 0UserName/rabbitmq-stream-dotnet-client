using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RabbitMQ.Stream.Client
{
    public record StreamSystemConfig
    {
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// TLS options setting.
        /// </summary>
        public SslOption Ssl { get; set; } = new SslOption();

        public IList<EndPoint> Endpoints { get; set; } = new List<EndPoint> {new IPEndPoint(IPAddress.Loopback, 5552)};

        public AddressResolver AddressResolver { get; set; } = null;
    }

    public class StreamSystem
    {
        private readonly StreamSystemConfig config;
        private readonly ClientParameters clientParameters;
        private readonly Client client;

        private StreamSystem(StreamSystemConfig config, ClientParameters clientParameters, Client client)
        {
            this.config = config;
            this.clientParameters = clientParameters;
            this.client = client;
        }

        public bool IsClosed => client.IsClosed;

        public static async Task<StreamSystem> Create(StreamSystemConfig config)
        {
            var clientParams = new ClientParameters
            {
                UserName = config.UserName,
                Password = config.Password,
                VirtualHost = config.VirtualHost,
                Ssl = config.Ssl,
                AddressResolver = config.AddressResolver
            };
            // create the metadata client connection
            foreach (var endPoint in config.Endpoints)
            {
                try
                {
                    var client = await Client.Create(clientParams with {Endpoint = endPoint});
                    if (!client.IsClosed)
                        return new StreamSystem(config, clientParams, client);
                }
                catch (Exception e)
                {
                    if (e is ProtocolException or SslException)
                    {
                        throw;
                    }

                    //TODO log? 
                }
            }

            throw new StreamSystemInitialisationException("no endpoints could be reached");
        }

        public async Task Close()
        {
            await client.Close("system close");
        }

        public async Task<Producer> CreateProducer(ProducerConfig producerConfig)
        {
            var meta = await client.QueryMetadata(new[] {producerConfig.Stream});
            var metaStreamInfo = meta.StreamInfos[producerConfig.Stream];
            if (metaStreamInfo.ResponseCode != ResponseCode.Ok)
            {
                throw new CreateProducerException($"producer could not be created code: {metaStreamInfo.ResponseCode}");
            }
            return await Producer.Create(clientParameters, producerConfig, metaStreamInfo);
        }

        public async Task CreateStream(StreamSpec spec)
        {
            var response = await client.CreateStream(spec.Name, spec.Args);
            if (response.ResponseCode is ResponseCode.Ok or ResponseCode.StreamAlreadyExists)
                return;
            throw new CreateStreamException($"Failed to create stream, error code: {response.ResponseCode.ToString()}");
        }


        public async Task<bool> StreamExists(string stream)
        {
            var streams = new[] {stream};
            var response = await client.QueryMetadata(streams);
            return response.StreamInfos is {Count: >= 1} &&
                   response.StreamInfos[stream].ResponseCode == ResponseCode.Ok;
        }

        public async Task DeleteStream(string stream)
        {
            var response = await client.DeleteStream(stream);
            if (response.ResponseCode == ResponseCode.Ok)
                return;
            throw new DeleteStreamException($"Failed to delete stream, error code: {response.ResponseCode.ToString()}");
        }

        public async Task<Consumer> CreateConsumer(ConsumerConfig consumerConfig)
        {
            var meta = await client.QueryMetadata(new[] {consumerConfig.Stream});
            var metaStreamInfo = meta.StreamInfos[consumerConfig.Stream];
            if (metaStreamInfo.ResponseCode != ResponseCode.Ok)
            {
                throw new CreateConsumerException($"consumer could not be created code: {metaStreamInfo.ResponseCode}");
            }
            return await Consumer.Create(clientParameters, consumerConfig, metaStreamInfo);
        }
    }

    public class CreateConsumerException : Exception
    {
        public CreateConsumerException(string s) : base(s)
        {
        }
    }


    public class CreateStreamException : Exception
    {
        public CreateStreamException(string s) : base(s)
        {
        }
    }

    public class DeleteStreamException : Exception
    {
        public DeleteStreamException(string s) : base(s)
        {
        }
    }

    public class CreateProducerException : Exception
    {
        public CreateProducerException(string s) : base(s)
        {
        }
    }


   
    public readonly struct LeaderLocator
    {
        private readonly string value;

        private LeaderLocator(string value)
        {
            this.value = value;
        }

        public static LeaderLocator ClientLocal => new LeaderLocator("client-local");
        public static LeaderLocator Random => new LeaderLocator("random");
        public static LeaderLocator LeastLeaders => new LeaderLocator("least-leaders");

        public override string ToString()
        {
            return value;
        }
    }

    public class StreamSystemInitialisationException : Exception
    {
        public StreamSystemInitialisationException(string error) : base(error)
        {
        }
    }
}