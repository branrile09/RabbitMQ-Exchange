namespace RabbitMQ_Exchange
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        //this should be in a database, however this is rapid prototyping       
        static List<Exchange_Order> _orders = new ();
        static List<Exchange_Order> _completed = new ();

        static void Main()
        {
            //initialize variables
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            string exchangeCode = "Orders"; 
            bool FINISHED = false;
            string input = " ";

         
            //connect and join
            ConnectionSetup(exchangeCode, channel);            
            Console.Clear();

            //let user know how to leave
            Console.WriteLine("TYPE 'EXIT' to leave");           

            //loop for sending and receiving messages
            while (!FINISHED)
            {

                input = Console.ReadLine()!;
                if (input == "EXIT" || input == "exit")
                {
                    Console.Clear();
                    FINISHED = true;
                }
                if (input == "cls")
                {
                    Console.Clear();                
                }
                else if (input == "listcurrent")
                {
                    foreach (Exchange_Order E in _orders)
                    {
                        string bs; //bs means buy or sell
                        if (E.buyOrSell)
                        {
                            bs = "buying";
                        }
                        else 
                        {
                            bs = "selling";
                        }

                        Console.WriteLine($"{E.username}:{bs} {E.stock}*{E.quantity} @${E.price}ea");                                       
                    
                    }                
                
                }
                if (input == "listcompleted")
                {
                    foreach (Exchange_Order E in _completed)
                    {
                        string bs; //bs means buy or sell
                        if (E.buyOrSell)
                        {
                            bs = "bought";
                        }
                        else
                        {
                            bs = "sold";
                        }

                        Console.WriteLine($"{E.username}:{bs} {E.stock}*{E.savedQuantity} @${E.price}ea");

                    }

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
            EventingBasicConsumer consumer = new (channel);
            consumer.Received += (model,  ea) =>
            {
                byte[] body = ea.Body.ToArray();
                Exchange_Order newOrder = new (body);
                ExchangeRequest(newOrder);
            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }




        static void ExchangeRequest(Exchange_Order newOrder)
        {
            Console.WriteLine("new order request");

            if (newOrder.buyOrSell)
            {
                Console.WriteLine($"{newOrder.username}: is buying {newOrder.quantity}@ ${newOrder.price}ea");
            }
            else 
            {
                Console.WriteLine($"{newOrder.username}: is selling {newOrder.quantity}@ ${newOrder.price}ea");
            }
            
            for (int i = 0; i < _orders.Count; i++)
            {
                if (newOrder.buyOrSell == _orders[i].buyOrSell || newOrder.stock != _orders[i].stock)
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
                        Console.WriteLine($"{newOrder.username}: is trading {newOrder.stock} {newOrder.quantity}@ ${_orders[i].price}ea from {_orders[i].username}");
                        _completed.Add(newOrder);
                    }
                    else if (newOrder.quantity == _orders[i].quantity)
                    {
                        Console.WriteLine($"{newOrder.username}: is trading {newOrder.stock} {newOrder.quantity}@ ${_orders[i].price}ea from {_orders[i].username}");
                        _completed.Add(_orders[i]);
                        _completed.Add(newOrder);
                        _orders.RemoveAt(i);                        
                        break;
                    }
                    else if (newOrder.quantity > _orders[i].quantity)
                    {
                        newOrder.quantity -= _orders[i].quantity;
                        Console.WriteLine($"{newOrder.username}: is trading  {newOrder.stock} {_orders[i].quantity}@ ${_orders[i].price}ea from {_orders[i].username}");
                        _completed.Add(_orders[i]);
                        _orders.RemoveAt(i);
                        i--; // still making more purchases, list is now smaller, so we need to go back 1 step
                    }
                }             
            }
            //if we have left over on order, or order not fullfilled. add to the list
            if (newOrder.quantity > 0)
            {
                _orders.Add(newOrder);
                Console.WriteLine("order added to que");
                Console.WriteLine($"{newOrder.username}: is trading {newOrder.quantity} @ {newOrder.price}ea");
            }
            Console.WriteLine("Order Processed");

        }

    }
}