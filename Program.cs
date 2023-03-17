namespace RabbitMQ_Exchange
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        //this should be in a database, however this is rapid prototyping
        static List<byte[]> WorkQue = new List<byte[]>();
        static List<string> WorkQueString = new List<string>();
        static List<Exchange_Order> _orders = new List<Exchange_Order>();
        static List<Exchange_Order> _completed = new List<Exchange_Order>();

        static void Main(string[] args)
        {
            //initialize variables
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            string exchangeCode = "Exchange404"; //not found
            string username = "Default";
            bool FINISHED = false;
            string input = "";

            

            //console shiz
            Console.WriteLine("What is your username?");
           // username = Console.ReadLine();
            Console.Clear();


            //connect and join
            ConnectionSetup(exchangeCode, channel);
            //JoinRoom(username, channel, exchangeCode);
            Console.Clear();

            //let user know how to leave
            Console.WriteLine("TYPE 'EXIT' to leave");
            

            //loop for sending and receiving messages
            while (!FINISHED)
            {

                input = Console.ReadLine();
                if (input != "EXIT")
                {
                    
                }
                else
                {
                   
                    Console.Clear();
                    FINISHED = true;
                }
            }

        }

        static void ConnectionSetup(string exchangeCode, IModel channel)
        {

            channel.ExchangeDeclare(exchange: exchangeCode, type: ExchangeType.Fanout);

            // declare a server-named queue
            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                exchange: exchangeCode,
                routingKey: string.Empty);

            //consuumer listener
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model,  ea) =>
            {
                Console.WriteLine("Update");
                byte[] body = ea.Body.ToArray();
                Exchange_Order newOrder = new Exchange_Order(body);
                ExchangeRequest(newOrder);

            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }


        static void ExchangeRequest(Exchange_Order newOrder)
        {
            for (int i = 0; i < _orders.Count; i++)
            {
                if (newOrder.buyOrSell == _orders[i].buyOrSell)
                {
                    continue;                
                }
                //check buy condition, if buy condition isnt met, we check sell condition.                 
                bool buyCondition = newOrder.buyOrSell && (newOrder.price >= _orders[i].price);
                bool sellCondition = false;
                if (!buyCondition)
                {
                    sellCondition = !newOrder.buyOrSell && (newOrder.price <= _orders[i].price);
                }
                bool tradeCondition = buyCondition || sellCondition;
                //we need appropriate trade condition to make a trade. 
                if (tradeCondition)
                {
                    if (newOrder.quantity < _orders[i].quantity)
                    {
                        _orders[i].quantity -= newOrder.quantity;
                    }
                    else if (newOrder.quantity == _orders[i].quantity)
                    {
                        _orders.RemoveAt(i);
                        
                        break;
                    }
                    else if (newOrder.quantity > _orders[i].quantity)
                    {
                        newOrder.quantity -= _orders[i].quantity;
                        _orders.RemoveAt(i);
                    }
                }             
            }
            //if we have left over on order, or order not fullfilled. add to the list
            if (newOrder.quantity > 0)
            {
                _orders.Add(newOrder);
                Console.WriteLine("order added to que");
            }
            Console.WriteLine("Order Processed");

        }

    }
}