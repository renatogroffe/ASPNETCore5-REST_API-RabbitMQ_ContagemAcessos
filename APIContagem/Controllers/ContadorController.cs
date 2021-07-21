using System;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using APIContagem.Models;

namespace APIContagem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContadorController : ControllerBase
    {
        private static readonly Contador _CONTADOR = new Contador();
        private readonly ILogger<ContadorController> _logger;
        private readonly IConfiguration _configuration;

        public ContadorController(ILogger<ContadorController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public ResultadoContador Get()
        {
            int valorAtualContador;
            lock (_CONTADOR)
            {
                _CONTADOR.Incrementar();
                valorAtualContador = _CONTADOR.ValorAtual;
            }

            var resultado = new ResultadoContador()
            {
                ValorAtual = valorAtualContador,
                Producer = _CONTADOR.Local,
                Kernel = _CONTADOR.Kernel,
                TargetFramework = _CONTADOR.TargetFramework,
                Mensagem = _configuration["MensagemVariavel"]
            };

            string queueName = _configuration["RabbitMQ:Queue"];
            string jsonContagem = JsonSerializer.Serialize(resultado);

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_configuration["RabbitMQ:ConnectionString"])
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
                
            channel.BasicPublish(exchange: "",
                                    routingKey: queueName,
                                    basicProperties: null,
                                    body: Encoding.UTF8.GetBytes(jsonContagem));

            _logger.LogInformation(
                $"RabbitMQ - Envio para a fila {queueName} concluído | " +
                $"{jsonContagem}");

            return resultado;
        }
    }
}